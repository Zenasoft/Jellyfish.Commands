// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Jellyfish.Commands.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Jellyfish.Commands.Metrics
{
    public class RollingNumber
    {
        private int numberOfBuckets;
        internal Bucket[] buckets;
        private int bucketSizeInMs;
        private volatile int currentBucketIndex;
        private Bucket cumulativeSum;
        private IClock clock;
        private object gate = new object();

        public int TimeInMs { get; private set; }
        public int NumberOfBuckets { get { return numberOfBuckets; } }

        public int BucketSizeInMs { get { return bucketSizeInMs; } }

        public RollingNumber(int timeInMs, int numberOfBuckets) : this(Clock.GetInstance(), timeInMs, numberOfBuckets)
        {
        }

        internal RollingNumber(IClock clock, int timeInMs, int numberOfBuckets)
        {
            this.TimeInMs = timeInMs;
            this.clock = clock;
            this.bucketSizeInMs = timeInMs/numberOfBuckets;
            var cx = numberOfBuckets + 1; // + one spare
            buckets = new Bucket[cx];
            this.numberOfBuckets = numberOfBuckets;
            cumulativeSum = new Bucket();

            for (int i = 0; i < cx; i++)
            {
                buckets[i] = new Bucket();
            }

            buckets[0].bucketStartInMs = clock.EllapsedTimeInMs;
        }

        internal void Reset()
        {
            var bucket = GetCurrentBucket();
            cumulativeSum.AddBucket(bucket);
            bucket.Reset(null, 0);
        }

        public void Increment(RollingNumberEvent ev)
        {
            GetCurrentBucket().Increment(ev);
        }

        public void UpdateRollingMax(RollingNumberEvent ev, long value)
        {
            GetCurrentBucket().UpdateMaxMax(ev, value);
        }

        internal long GetValueOfLatestBucket(RollingNumberEvent ev)
        {
            Bucket lastBucket = GetCurrentBucket();
            if (lastBucket == null)
                return 0;
            // we have bucket data so we'll return the lastBucket
            if ((int)ev > (int)RollingNumberEvent.MAX_COUNTER)
                return lastBucket.GetMaxUpdater(ev);
            else
                return lastBucket.GetAdder(ev);
        }

        internal Bucket GetCurrentBucket()
        {
            int newCurrentIndex;
            int initialCurrentIndex;
            Bucket bucket;
            Bucket newBucket;
            long currentTime;
            
            currentTime = clock.EllapsedTimeInMs;
            do {
                initialCurrentIndex = currentBucketIndex;
                bucket = buckets[initialCurrentIndex];
                if (bucket.bucketStartInMs + bucketSizeInMs > currentTime)
                {
                    return bucket;
                }

                newCurrentIndex = (initialCurrentIndex + 1) % (numberOfBuckets+1);
                newBucket = buckets[newCurrentIndex];

                bool lockTacken=false;
                try
                {
                    Monitor.TryEnter(gate, 10, ref lockTacken);
                    if (!lockTacken)
                    {
                        return newBucket;
                    }
                    newBucket.Reset(bucket, currentTime);
                }
                finally
                {
                    if (lockTacken)
                        Monitor.Exit(gate);
                }
            }
            while ( Interlocked.CompareExchange(ref currentBucketIndex, newCurrentIndex, initialCurrentIndex) != initialCurrentIndex);

          //  Console.WriteLine("Change bucket to {0}, {1}", newCurrentIndex, currentTime);
            cumulativeSum.AddBucket(bucket);

            return newBucket;
        }

        public long GetCumulativeSum(RollingNumberEvent ev)
        {
            return GetCurrentBucket().GetAdder(ev) + cumulativeSum.GetAdder(ev);
        }

        public long GetRollingSum(RollingNumberEvent ev, long startWindowTime = 0L)
        {
            GetCurrentBucket();
            return GetValues(ev, startWindowTime).Sum();
        }

        public long GetRollingMaxValue(RollingNumberEvent ev, long startWindowTime = 0L)
        {
            GetCurrentBucket();
            return GetValues(ev, startWindowTime).Max();
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

        internal IEnumerable<long> GetValues(RollingNumberEvent ev, long startWindowTime=0)
        { 
            foreach(var b in GetBuckets(startWindowTime))
            {
                if ((int)ev < (int)RollingNumberEvent.MAX_COUNTER)
                    yield return b.adders[(int)ev];
                else
                    yield return b.maxAdders[(int)ev];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal class Bucket
        {
            public long[] adders;
            public long[] maxAdders;
            public long bucketStartInMs;

            public Bucket()
            {
                var cx = Enum.GetValues(typeof(RollingNumberEvent)).Length;
                adders = new long[cx];
                maxAdders = new long[cx];
                bucketStartInMs = long.MinValue;
            }

            public long Total
            {
                get
                {
                    return adders.Sum();
                }
            }

            public long GetMaxUpdater(RollingNumberEvent ev)
            {
                return this.maxAdders[(int)ev];
            }

            public long GetAdder(RollingNumberEvent ev)
            {
                return this.adders[(int)ev];
            }

            public void AddBucket(Bucket bucket)
            {
                for(var i=0;i<bucket.adders.Length;i++)
                {
                    adders[i] += bucket.adders[i];
                    maxAdders[i] = bucket.maxAdders[i];
                }
            }

            public void UpdateMaxMax(RollingNumberEvent ev, long value)
            {
                long max;
                do
                {
                    max = maxAdders[(int)ev];
                    if (value <= max) return;
                }
                while (Interlocked.CompareExchange(ref maxAdders[(int)ev], value, max) != max);
            }

            public void Increment(RollingNumberEvent ev)
            {
                Interlocked.Increment(ref adders[(int)ev]);
            }

            internal void Reset(Bucket b, long currentTime)
            {
                for (var i = 0; i < adders.Length; i++)
                {
                    adders[i] = 0L;
                    if(b!= null)
                        maxAdders[i] = b.maxAdders[i];
                }

                if(b!= null)
                    bucketStartInMs = currentTime;
            }
        }
    }
}
