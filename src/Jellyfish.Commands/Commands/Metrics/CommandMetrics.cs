// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Jellyfish.Commands.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Linq;

namespace Jellyfish.Commands.Metrics
{
    public struct CommandMetricsEntry
    {
        public string CommandName { get; private set; }
        public CommandMetrics Metrics { get; set; }

        public CommandMetricsEntry(string name, CommandMetrics metrics)
        {
            CommandName = name;
            Metrics = metrics;
        }
    }

    public class CommandMetrics
    {
        public static ConcurrentDictionary<string, CommandMetrics> _metrics = new ConcurrentDictionary<string, CommandMetrics>();
        private CommandProperties _properties;
        private IClock _clock;
        private long _lastReset;

        public static CommandMetrics GetInstance(string name,  CommandProperties properties, IClock clock)
        {
            Contract.Assert(!String.IsNullOrEmpty(name));
            Contract.Assert(properties != null);

            return _metrics.GetOrAdd(name, n => new CommandMetrics(n, properties, clock));
        }

        public static IEnumerable<CommandMetricsEntry> GetInstances()
        {
            return from e in _metrics
                   select new CommandMetricsEntry(e.Key, e.Value);
        }

        private readonly RollingNumber _counter;
        private readonly RollingPercentileNumber _percentileExecution;
        private readonly RollingPercentileNumber _percentileTotal;
        private int _concurrentExecutionCount;

        public CommandProperties Properties { get { return _properties; } }

        internal CommandMetrics(string name, CommandProperties properties, IClock clock=null)
        {
            _clock = clock ?? Clock.GetInstance();
            this._properties = properties;
            CommandName = name;
            _counter = new RollingNumber(_clock, properties.MetricsRollingStatisticalWindowInMilliseconds.Get(), properties.MetricsRollingStatisticalWindowBuckets.Get());
            _percentileExecution = new RollingPercentileNumber(_clock, properties.MetricsRollingPercentileWindowInMilliseconds.Get(), properties.MetricsRollingPercentileWindowBuckets.Get(), properties.MetricsRollingPercentileBucketSize.Get(), properties.MetricsRollingPercentileEnabled);
            _percentileTotal = new RollingPercentileNumber(_clock, properties.MetricsRollingPercentileWindowInMilliseconds.Get(), properties.MetricsRollingPercentileWindowBuckets.Get(), properties.MetricsRollingPercentileBucketSize.Get(), properties.MetricsRollingPercentileEnabled);
        }

        public int CurrentConcurrentExecutionCount { get { return _concurrentExecutionCount; } }

        public long GetCumulativeCount(RollingNumberEvent ev)
        {
            return this._counter.GetCumulativeSum(ev);
        }
        public long GetRollingCount(RollingNumberEvent ev)
        {
            return this._counter.GetRollingSum(ev);
        }
        public int GetExecutionTimePercentile(double percentile)
        {
            return this._percentileExecution.GetPercentile(percentile);
        }
        public int GetExecutionTimeMean()
        {
            return this._percentileExecution.Mean;
        }
        public int GetTotalTimePercentile(double percentile)
        {
            return this._percentileTotal.GetPercentile(percentile);
        }
        public int GetTotalTimeMean()
        {
            return this._percentileTotal.Mean;
        }
        public long GetRollingMaxConcurrentExecutions()
        {
            return _counter.GetRollingMaxValue(RollingNumberEvent.COMMAND_MAX_ACTIVE);
        }

        internal void MarkBadRequest(long duration)
        {
            this._counter.Increment(RollingNumberEvent.BAD_REQUEST);
        }

        internal void MarkSuccess(long duration)
        {
            this._counter.Increment(RollingNumberEvent.SUCCESS);
        }
        internal void MarkFailure(long duration)
        {
            this._counter.Increment(RollingNumberEvent.FAILURE);
        }
        internal void MarkTimeout(long duration)
        {
            this._counter.Increment(RollingNumberEvent.TIMEOUT);
        }
        internal void MarkShortCircuited()
        {
            this._counter.Increment(RollingNumberEvent.SHORT_CIRCUITED);
        }
        internal void MarkThreadPoolRejection()
        {
            this._counter.Increment(RollingNumberEvent.THREAD_POOL_REJECTED);
        }
        internal void MarkSemaphoreRejection()
        {
            //            this.eventNotifier.MarkEvent(EventType.SemaphoreRejected, this.key);
            this._counter.Increment(RollingNumberEvent.SEMAPHORE_REJECTED);
        }
        internal void MarkFallbackSuccess()
        {
            this._counter.Increment(RollingNumberEvent.FALLBACK_SUCCESS);
        }
        internal void MarkFallbackFailure()
        {
            this._counter.Increment(RollingNumberEvent.FALLBACK_FAILURE);
        }
        internal void MarkFallbackRejection()
        {
            this._counter.Increment(RollingNumberEvent.FALLBACK_REJECTION);
        }
        internal void MarkExceptionThrown()
        {
            this._counter.Increment(RollingNumberEvent.EXCEPTION_THROWN);            
        }
        //internal void MarkCollapsed(int numRequestsCollapsedToBatch)
        //{
        //    this.counter.Add(RollingNumberEvent.Collapsed, numRequestsCollapsedToBatch);
        //}
        internal void MarkResponseFromCache()
        {
            this._counter.Increment(RollingNumberEvent.RESPONSE_FROM_CACHE);
        }

        internal void IncrementConcurrentExecutionCount()
        {
            var cx = Interlocked.Increment(ref this._concurrentExecutionCount);
            this._counter.UpdateRollingMax(RollingNumberEvent.COMMAND_MAX_ACTIVE, cx);
        }

        internal void DecrementConcurrentExecutionCount()
        {
            Interlocked.Decrement(ref this._concurrentExecutionCount);
        }

        internal void AddCommandExecutionTime(long duration)
        {
            this._percentileExecution.AddValue((int)duration);
        }

        internal void AddUserThreadExecutionTime(long duration)
        {
            this._percentileTotal.AddValue((int)duration);
        }

        private HealthCounts healthCountsSnapshot = HealthCounts.Empty;
        private long lastHealthCountsSnapshot = 0;

        public string CommandName { get; private set; }

        internal void Reset()
        {
            Interlocked.Exchange(ref _lastReset, _clock.EllapsedTimeInMs);
            healthCountsSnapshot = HealthCounts.Empty;
            _counter.Reset();
        }

        internal HealthCounts GetHealthCounts()
        { 
            // we put an interval between snapshots so high-volume commands don't 
            // spend too much unnecessary time calculating metrics in very small time periods
            long lastTime = this.lastHealthCountsSnapshot;
            long currentTime = _clock.EllapsedTimeInMs;

            if ( currentTime - lastTime >= this._properties.MetricsHealthSnapshotIntervalInMilliseconds.Get() || healthCountsSnapshot.IsEmpty)
            {
                if (Interlocked.CompareExchange(ref this.lastHealthCountsSnapshot, currentTime, lastTime) == lastTime)
                {
                    long lastReset = _lastReset;
                    // our thread won setting the snapshot time so we will proceed with generating a new snapshot
                    // losing threads will continue using the old snapshot
                    long success = _counter.GetRollingSum(RollingNumberEvent.SUCCESS, lastReset);
                    long failure = _counter.GetRollingSum(RollingNumberEvent.FAILURE, lastReset); // fallbacks occur on this
                    long timeout = _counter.GetRollingSum(RollingNumberEvent.TIMEOUT, lastReset); // fallbacks occur on this
                    long threadPoolRejected = _counter.GetRollingSum(RollingNumberEvent.THREAD_POOL_REJECTED, lastReset); // fallbacks occur on this
                    long semaphoreRejected = _counter.GetRollingSum(RollingNumberEvent.SEMAPHORE_REJECTED, lastReset); // fallbacks occur on this
                    long shortCircuited = _counter.GetRollingSum(RollingNumberEvent.SHORT_CIRCUITED, lastReset); // fallbacks occur on this

                    long totalCount = failure + success + timeout + threadPoolRejected + shortCircuited + semaphoreRejected;
                    long errorCount = failure + timeout + threadPoolRejected + shortCircuited + semaphoreRejected;
                    int errorPercentage = 0;

                    if (totalCount > 0)
                    {
                        errorPercentage = (int)((double)errorCount / totalCount * 100);
                    }
                    healthCountsSnapshot = new HealthCounts(totalCount, errorCount, errorPercentage);
                }
            }
            return healthCountsSnapshot;
        }
    }

    /**
 * Number of requests during rolling window.
 * Number that failed (failure + success + timeout + threadPoolRejected + shortCircuited + semaphoreRejected).
 * Error percentage;
 */
    public struct HealthCounts
    {
        private long totalCount;
        private long errorCount;
        private int errorPercentage;
        public static HealthCounts Empty = new HealthCounts(true);

        public bool IsEmpty { get; private set; }

        public HealthCounts(bool isEmpty) : this()
        {
            IsEmpty = isEmpty;
        }

        public HealthCounts(long total, long error, int errorPercentage) : this(false)
        {
            this.totalCount = total;
            this.errorCount = error;
            this.errorPercentage = errorPercentage;
        }

        public long TotalRequests
        {
            get {return totalCount;}
        }

        public long ErrorCount
        {
            get { return errorCount;}
        }

        public int ErrorPercentage
        {
            get { return errorPercentage;}
        }
    }
}
