// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Jellyfish.Commands.Utils;
using Jellyfish.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Jellyfish.Commands.Metrics
{
    public class RollingPercentileNumber
    {
        private int numberOfBuckets;
        internal Bucket[] buckets;
        private int bucketSizeInMs;
        private int currentBucketIndex;
        private PercentileSnapshot _percentileSnapshot;
        private IClock clock;
        private object gate = new object();
        private IDynamicProperty<bool> enabled;

        public int TimeInMs { get; private set; }
        public int NumberOfBuckets { get { return numberOfBuckets; } }

        public int BucketSizeInMs { get { return bucketSizeInMs; } }

        public RollingPercentileNumber(int timeInMs, int numberOfBuckets, int dataLength, IDynamicProperty<bool> enabled)
            : this(Clock.GetInstance(), timeInMs, numberOfBuckets, dataLength, enabled)
        {
        }

        internal RollingPercentileNumber(IClock clock, int timeInMs, int numberOfBuckets, int dataLength, IDynamicProperty<bool> enabled)
        {
            this.enabled = enabled;
            this.TimeInMs = timeInMs;
            this.clock = clock;
            this.bucketSizeInMs = timeInMs / numberOfBuckets;
            var cx = numberOfBuckets + 1; // + one spare
            buckets = new Bucket[cx];
            this.numberOfBuckets = numberOfBuckets;

            for (int i = 0; i < cx; i++)
            {
                buckets[i] = new Bucket(dataLength);
            }

            buckets[0].bucketStartInMs = clock.EllapsedTimeInMs;
            _percentileSnapshot = new PercentileSnapshot(GetBuckets().Select(b=>new SnapshotItem { Length = b.Length, Data = b.data }).ToArray());
        }

        /// <summary> Compute a percentile from the underlying rolling buckets of values.
        /// <p>
        /// For performance reasons it maintains a single snapshot of the sorted values from all buckets that is re-generated each time the bucket rotates.
        /// </p>
        /// This means that if a bucket is 5000ms, then this method will re-compute a percentile at most once every 5000ms.
        /// 
        /// <param name="percentile">value such as 99 (99th percentile), 99.5 (99.5th percentile), 50 (median, 50th percentile) to compute and retrieve percentile from rolling buckets.</param>
        /// <returns>percentile value</returns>
        /// </summary>
        public int GetPercentile(double percentile)
        {
            if (!this.enabled.Get())
                return -1;

            // fetch the current snapshot
            return CurrentPercentileSnapshot.GetPercentile(percentile);
        }

        /// <summary>This returns the mean (average) of all values in the current snapshot. This is not a percentile but often desired so captured and exposed here.
        /// 
        /// <returns>mean of all values</returns>
        /// </summary>
        public int Mean
        {
            get
            {
                if (!this.enabled.Get())
                    return -1;

                // fetch the current snapshot
                return CurrentPercentileSnapshot.Mean;
            }
        }

        /// <summary>This will retrieve the current snapshot or create a new one if one does not exist.
        ///
        /// It will NOT include data from the current bucket, but all previous buckets.
        /// <p>
        /// It remains cached until the next bucket rotates at which point a new one will be created.
        /// </summary>
        private PercentileSnapshot CurrentPercentileSnapshot
        {
            get
            {
                GetCurrentBucket();
                return _percentileSnapshot;
            }
        }


        public void AddValue(int value)
        {
            if (!this.enabled.Get())
                return;

            GetCurrentBucket().AddValue(value);
        }

        internal Bucket GetCurrentBucket()
        {
            int newCurrentIndex;
            int initialCurrentIndex;
            Bucket bucket;
            Bucket newBucket;
            long currentTime;

            do
            {
                currentTime = clock.EllapsedTimeInMs;

                initialCurrentIndex = currentBucketIndex;
                bucket = buckets[initialCurrentIndex];
                if (bucket.bucketStartInMs + bucketSizeInMs > currentTime)
                {
                    return bucket;
                }

                newCurrentIndex = (currentBucketIndex + 1) % (numberOfBuckets + 1);
                newBucket = buckets[newCurrentIndex];

                bool lockTacken = false;
                try
                {
                    Monitor.TryEnter(gate, 10, ref lockTacken);                                     
                    if (!lockTacken)
                    {
                        return newBucket;
                    }
                    newBucket.Reset(currentTime);
                }
                finally
                {
                    if (lockTacken)
                        Monitor.Exit(gate);
                }
            }
            while (Interlocked.CompareExchange(ref currentBucketIndex, newCurrentIndex, initialCurrentIndex) != initialCurrentIndex);

            var items = from b in GetBuckets()
                          select new SnapshotItem { Length = b.Length, Data = b.data };
            _percentileSnapshot = new PercentileSnapshot(items.ToArray());

            return newBucket;
        }


        /// <summary>
        /// Returns all bucket in reverse order (most recent first)
        /// </summary>
        /// <param name="startWindowTime"></param>
        /// <returns></returns>
        internal IEnumerable<Bucket> GetBuckets(long startWindowTime = 0)
        {
            var idx = currentBucketIndex;
            if (startWindowTime == 0)
                startWindowTime = clock.EllapsedTimeInMs - (numberOfBuckets * bucketSizeInMs);
            var cx = numberOfBuckets;

            while (cx-- > 0)
            {
                var b = buckets[idx];
                if (b.bucketStartInMs < startWindowTime) break;
                yield return b;
                idx = (idx - 1);
                if (idx < 0) idx = numberOfBuckets;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal class Bucket
        {
            public int[] data;
            private int index;
            public long bucketStartInMs;
            private int dataLength;
            private int length;

            public Bucket(int dataLength)
            {
                this.dataLength = dataLength;
                data = new int[dataLength];
                bucketStartInMs = long.MinValue;
            }

            public void AddValue(int value)
            {
                int initial;
                int newIndex;
                do
                {
                    initial = index;
                    newIndex = (initial + 1) % dataLength;
                }
                while (Interlocked.CompareExchange(ref index, newIndex, initial) != initial);
                data[initial] = value;
                Interlocked.Increment( ref length );
            }

            public int Length
            {
                get
                {
                    return length > dataLength ? dataLength : length;
                }
            }

            internal void Reset(long currentTime)
            {
                Array.Clear(data, 0, dataLength);
                index = 0;
                length = 0;
                bucketStartInMs = currentTime;
            }
        }

    }

    internal class PercentileSnapshot
    {
        private int[] data;
        private int length;
        private int mean;

        public PercentileSnapshot(IEnumerable<SnapshotItem> dataList)
        {
            long lengthFromBuckets = 0;

            // we need to calculate it dynamically as it could have been changed by properties (rare, but possible)
            // also this way we capture the actual index size rather than the max so size the int[] to only what we need
            foreach (var b in dataList)
            {
                lengthFromBuckets += b.Length;
            }

            data = new int[lengthFromBuckets];
            int index = 0;
            int sum = 0;
            foreach (var b in dataList)
            {
                int length = b.Length;
                for (int i = 0; i < length; i++)
                {
                    int v = b.Data[i];
                    this.data[index++] = v;
                    sum += v;
                }
            }
            this.length = index;
            if (this.length == 0)
            {
                this.mean = 0;
            }
            else
            {
                this.mean = sum / this.length;
            }

            Array.Sort(this.data, 0, length);
        }

        internal PercentileSnapshot(params int[] data)
        {
            this.data = data;
            this.length = data.Length;

            int sum = 0;
            foreach (int v in data)
            {
                sum += v;
            }

            this.mean = sum / this.length;

            Array.Sort(this.data, 0, length);
        }


        public int Mean
        {
            get
            {
                return mean;
            }
        }

        /// <summary>
        /// Provides percentile computation.
        /// </summary>
        public int GetPercentile(double percentile)
        {
            if (length == 0)
            {
                return 0;
            }
            return ComputePercentile(percentile);
        }

        /// <summary>
        /// <see cref="http://en.wikipedia.org/wiki/Percentile">Percentile (Wikipedia)</see>
        /// <see cref="http://cnx.org/content/m10805/latest/">Percentile</see>
        /// 
        /// <param name="percent">percentile of data desired</param>
        /// <returns>data at the asked-for percentile.  Interpolation is used if exactness is not possible</returns>
        ///</summary>                
        private int ComputePercentile(double percent)
        {
            // Some just-in-case edge cases
            if (length <= 0)
            {
                return 0;
            }
            else if (percent <= 0.0)
            {
                return data[0];
            }
            else if (percent >= 100.0)
            {
                return data[length - 1];
            }

            // ranking (http://en.wikipedia.org/wiki/Percentile#Alternative_methods)
            double rank = (percent / 100.0) * length;

            // linear interpolation between closest ranks
            int iLow = (int)Math.Floor(rank);
            int iHigh = (int)Math.Ceiling(rank);
            if (iHigh >= length)
            {
                // Another edge case
                return data[length - 1];
            }
            else if (iLow == iHigh)
            {
                return data[iLow];
            }
            else
            {
                // Interpolate between the two bounding values
                return (int)(data[iLow] + (rank - iLow) * (data[iHigh] - data[iLow]));
            }
        }
    }

    struct SnapshotItem
    {
        public int Length;
        public int[] Data;
    }
}
