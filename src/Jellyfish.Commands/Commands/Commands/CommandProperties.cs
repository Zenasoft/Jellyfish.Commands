// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

using Jellyfish.Configuration;
using Microsoft.Framework.Internal;

namespace Jellyfish.Commands
{
    /// <summary>
    /// Service command properties
    /// </summary>
    public class CommandProperties
    {
        private string _commandName;

        internal const int default_metricsRollingStatisticalWindow = 10000;// default => statisticalWindow: 10000 = 10 seconds (and default of 10 buckets so each bucket is 1 second) 
        internal const int default_metricsRollingStatisticalWindowBuckets = 10;// default => statisticalWindowBuckets: 10 = 10 buckets in a 10 second window so each bucket is 1 second 
        internal const int default_circuitBreakerRequestVolumeThreshold = 20;// default => statisticalWindowVolumeThreshold: 20 requests in 10 seconds must occur before statistics matter 
        internal const int default_circuitBreakerSleepWindowInMilliseconds = 5000;// default => sleepWindow: 5000 = 5 seconds that we will sleep before trying again after tripping the circuit 
        internal const int default_circuitBreakerErrorThresholdPercentage = 50;// default => errorThresholdPercentage = 50 = if 50%+ of requests in 10 seconds are failures or latent when we will trip the circuit 
        internal const bool default_circuitBreakerForceOpen = false;// default => forceCircuitOpen = false (we want to allow traffic) 
        internal const bool default_circuitBreakerForceClosed = false;// default => ignoreErrors = false  
        internal const int default_executionTimeoutInMilliseconds = 1000; // default => executionTimeoutInMilliseconds: 1000 = 1 second 
        internal const bool default_executionTimeoutEnabled = true;
        internal const bool default_executionIsolationThreadInterruptOnTimeout = true;
        internal const bool default_metricsRollingPercentileEnabled = true;
        internal const bool default_requestCacheEnabled = true;
        internal const int default_fallbackIsolationSemaphoreMaxConcurrentRequests = 10;
        internal const bool default_fallbackEnabled = true;
        internal const int default_executionIsolationSemaphoreMaxConcurrentRequests = 10;
        internal const bool default_requestLogEnabled = true;
        internal const bool default_circuitBreakerEnabled = true;
        internal const int default_metricsRollingPercentileWindow = 60000; // default to 1 minute for RollingPercentile  
        internal const int default_metricsRollingPercentileWindowBuckets = 6; // default to 6 buckets (10 seconds each in 60 second window) 
        internal const int default_metricsRollingPercentileBucketSize = 100; // default to 100 values max per bucket 
        internal const int default_metricsHealthSnapshotIntervalInMilliseconds = 500; // default to 500ms as max frequency between allowing snapshots of health (error percentage etc) 
        internal const ExecutionIsolationStrategy default_executionIsolationStratgey = Jellyfish.Commands.ExecutionIsolationStrategy.Thread;

        /// <summary>
        /// If true the <see cref="circuitBreaker.AllowRequest"/> will always return true to allow requests regardless of the error percentage from <see cref="CommandMetrics.GetHealthCounts"/>.
        /// <p>
        /// The <see cref="CircuitBreakerForceOpen"/> property takes precedence so if it set to true this property does nothing.
        /// </summary>   
        public IDynamicProperty<bool> CircuitBreakerForceClosed { get; private set; }

        /// <summary>
        ///  If true the <see cref="circuitBreaker.AllowRequest"/> will always return false, causing the circuit to be open (tripped) and reject all requests.
        ///  <p>
        ///  This property takes precedence over <see cref="CircuitBreakerForceClosed"/>;
        /// </summary> 
        public IDynamicProperty<bool> CircuitBreakerForceOpen { get; private set; }

        /// <summary>
        ///  Minimum number of requests in the <see cref="MetricsRollingStatisticalWindowInMilliseconds"/> that must exist before the <see cref="circuitBreaker"/> will trip.
        ///  <p>
        ///  If below this number the circuit will not trip regardless of error percentage.
        /// </summary> 
        public IDynamicProperty<int> CircuitBreakerRequestVolumeThreshold { get; private set; }

        /// <summary>
        ///  Error percentage threshold (as whole number such as 50) at which point the circuit breaker will trip open and reject requests.
        ///  <p>
        ///  It will stay tripped for the duration defined in <see cref="CircuitBreakerSleepWindowInMilliseconds"/>;
        ///  <p>
        ///  The error percentage this is compared against comes from <see cref="CommandMetrics.GetHealthCounts"/>.
        /// </summary> 
        public IDynamicProperty<int> CircuitBreakerErrorThresholdPercentage { get; private set; }

        /// <summary>
        ///  The time in milliseconds after a <see cref="circuitBreaker"/> trips open that it should wait before trying requests again.
        /// </summary> 
        public IDynamicProperty<int> CircuitBreakerSleepWindowInMilliseconds { get; private set; }

        /// <summary>
        ///  Number of concurrent requests permitted to <see cref="Command.GetFallback"/>. Requests beyond the concurrent limit will fail-fast and not attempt retrieving a fallback.
        /// </summary> 
        public IDynamicProperty<int> FallbackIsolationSemaphoreMaxConcurrentRequests { get; private set; }

        /// <summary>
        ///  Number of concurrent requests permitted to <see cref="Command.run"/>. Requests beyond the concurrent limit will be rejected.
        ///  <p>
        ///  Applicable only when <see cref="ExecutionIsolationStrategy"/> == SEMAPHORE.
        /// </summary> 
        public IDynamicProperty<int> ExecutionIsolationSemaphoreMaxConcurrentRequests { get; private set; }

        /// <summary>
        ///  Time in milliseconds at which point the command will timeout and halt execution.
        ///  <p>
        ///  If <see cref="ExecutionIsolationThreadInterruptOnTimeout"/> == true and the command is thread-isolated, the executing thread will be interrupted.
        ///  If the command is semaphore-isolated and a <see cref="ObservableComman"/>, that command will get unsubscribed.
        ///  <p>
        /// </summary> 
        public IDynamicProperty<int> ExecutionIsolationThreadTimeoutInMilliseconds { get; private set; }

        public IDynamicProperty<ExecutionIsolationStrategy> ExecutionIsolationStrategy { get; private set; }

        /// <summary>
        ///  Whether to use a <see cref="circuitBreaker"/> or not. If false no circuit-breaker logic will be used and all requests permitted.
        ///  <p>
        ///  This is similar in effect to <see cref="CircuitBreakerForceClosed"/> except that continues tracking metrics and knowing whether it
        ///  should be open/closed, this property results in not even instantiating a circuit-breaker.
        /// </summary> 
        public IDynamicProperty<bool> CircuitBreakerEnabled { get; private set; }

        /// <summary>
        ///  Duration of statistical rolling window in milliseconds. This is passed into <see cref="RollingNumber"/> inside <see cref="CommandMetrics"/>.
        /// </summary> 
        public IDynamicProperty<int> MetricsRollingStatisticalWindowInMilliseconds { get; private set; }

        /// <summary>
        ///  Whether <see cref="Command.GetCacheKey"/> should be used with <see cref="RequestCache"/> to provide de-duplication functionality via request-scoped caching.
        /// </summary> 
        ///  @return {@code Property<Boolean>}
        /// 
        public IDynamicProperty<bool> RequestCacheEnabled { get; private set; }

        /// <summary>
        ///  Whether <see cref="ServiceCommand"/> execution and events should be logged to <see cref="RequestLog"/>.
        /// </summary> 
        ///  @return {@code Property<Boolean>}
        /// 
        public IDynamicProperty<bool> RequestLogEnabled { get; private set; }

        /// <summary>
        ///  Number of buckets the rolling statistical window is broken into. This is passed into <see cref="RollingNumber"/> inside <see cref="CommandMetrics"/>.
        /// </summary> 
        public IDynamicProperty<int> MetricsRollingStatisticalWindowBuckets { get; private set; }

        /// <summary>
        ///  Number of buckets the rolling percentile window is broken into. This is passed into <see cref="RollingPercentile"/> inside <see cref="CommandMetrics"/>.
        /// </summary> 
        public IDynamicProperty<int> MetricsRollingPercentileWindowBuckets { get; private set; }

        /// <summary>
        ///  Duration of percentile rolling window in milliseconds. This is passed into <see cref="RollingPercentile"/> inside <see cref="CommandMetrics"/>.
        /// </summary> 
        public IDynamicProperty<int> MetricsRollingPercentileWindowInMilliseconds { get; private set; }

        /// <summary>
        ///  Whether percentile metrics should be captured using <see cref="RollingPercentile"/> inside <see cref="CommandMetrics"/>.
        /// </summary> 
        public IDynamicProperty<bool> MetricsRollingPercentileEnabled { get; private set; }

        /// <summary> 
        ///  Maximum number of values stored in each bucket of the rolling percentile. This is passed into <see cref="RollingPercentile"/> inside <see cref="CommandMetrics"/>.
        ///  
        /// </summary> 
        public IDynamicProperty<int> MetricsRollingPercentileBucketSize { get; private set; }

        /// <summary>
        ///  Time in milliseconds to wait between allowing health snapshots to be taken that calculate success and error percentages and affect <see cref="circuitBreaker.isOpen"/> status.
        ///  <p>
        ///  On high-volume circuits the continual calculation of error percentage can become CPU intensive thus this controls how often it is calculated.
        /// </summary> 
        public IDynamicProperty<int> MetricsHealthSnapshotIntervalInMilliseconds { get; private set; }

        /// <summary>
        ///  Whether <see cref="Command.GetFallback"/> should be attempted when failure occurs.
        /// </summary> 
        public IDynamicProperty<bool> FallbackEnabled { get; private set; }

        /// <summary>
        ///  Whether the timeout mechanism is enabled for this command
        /// </summary> 
        public IDynamicProperty<bool> ExecutionTimeoutEnabled { get; private set; }


      private IDynamicProperty<TValue> Get<TValue>([NotNull]string name, TValue defaultValue) where TValue : struct
        {
            return DynamicProperties.Factory.AsChainedProperty<TValue>(
                "jellyFish.command." + _commandName + "." + name,
                defaultValue,
                "jellyFish.command.default." + name);
        }

        internal CommandProperties([NotNull]string commandName)
        {
            this._commandName = commandName;
            CircuitBreakerForceClosed = this.Get<bool>("circuitBreaker.forceClosed", default_circuitBreakerForceClosed);
            CircuitBreakerForceOpen = this.Get<bool>("circuitBreaker.forceOpen", default_circuitBreakerForceOpen);
            CircuitBreakerRequestVolumeThreshold = this.Get<int>("circuitBreaker.requestVolumeThreshold", default_circuitBreakerRequestVolumeThreshold);
            CircuitBreakerErrorThresholdPercentage = this.Get<int>("circuitBreaker.errorThresholdPercentage", default_circuitBreakerErrorThresholdPercentage);
            CircuitBreakerSleepWindowInMilliseconds = this.Get<int>("circuitBreaker.sleepWindowInMilliseconds", default_circuitBreakerSleepWindowInMilliseconds);
            FallbackIsolationSemaphoreMaxConcurrentRequests = this.Get<int>("fallback.isolation.semaphore.maxConcurrentRequests", default_fallbackIsolationSemaphoreMaxConcurrentRequests);
            ExecutionIsolationSemaphoreMaxConcurrentRequests = this.Get<int>("execution.isolation.semaphore.maxConcurrentRequests", default_executionIsolationSemaphoreMaxConcurrentRequests);
            ExecutionIsolationThreadTimeoutInMilliseconds = this.Get<int>("execution.isolation.thread.timeoutInMilliseconds", default_executionTimeoutInMilliseconds);
            CircuitBreakerEnabled = this.Get<bool>("circuitBreaker.enabled", default_circuitBreakerEnabled);
            MetricsRollingStatisticalWindowInMilliseconds = this.Get<int>("metrics.rollingStats.timeInMilliseconds", default_metricsRollingStatisticalWindow);
            RequestCacheEnabled = this.Get<bool>("requestCache.enabled", default_requestCacheEnabled);
            RequestLogEnabled = this.Get<bool>("requestLog.enabled", default_requestLogEnabled);
            MetricsRollingStatisticalWindowBuckets = this.Get<int>("metrics.rollingStats.numBuckets", default_metricsRollingStatisticalWindowBuckets);
            MetricsRollingPercentileWindowBuckets = this.Get<int>("metrics.rollingPercentile.numBuckets", default_metricsRollingPercentileWindowBuckets);
            MetricsRollingPercentileWindowInMilliseconds = this.Get<int>("metrics.rollingPercentile.timeInMilliseconds", default_metricsRollingPercentileWindow);
            MetricsRollingPercentileEnabled = this.Get<bool>("metrics.rollingPercentile.enabled", default_metricsRollingPercentileEnabled);
            MetricsRollingPercentileBucketSize = this.Get<int>("metrics.rollingPercentile.bucketSize", default_metricsRollingPercentileBucketSize);
            MetricsHealthSnapshotIntervalInMilliseconds = this.Get<int>("metrics.healthSnapshot.intervalInMilliseconds", default_metricsHealthSnapshotIntervalInMilliseconds);
            FallbackEnabled = this.Get<bool>("fallback.enabled", default_fallbackEnabled);
            ExecutionTimeoutEnabled = this.Get<bool>("execution.timeout.enabled", default_executionTimeoutEnabled);
            ExecutionIsolationStrategy = this.Get<ExecutionIsolationStrategy>("execution.isolation.strategy", default_executionIsolationStratgey);
        }
    }

    public sealed class CommandPropertiesBuilder
    {
        public bool? circuitBreakerEnabled { get; private set; } = null;
        public int? circuitBreakerErrorThresholdPercentage { get; private set; } = null; 
        public bool? circuitBreakerForceClosed { get; private set; } = null; 
        public bool? circuitBreakerForceOpen { get; private set; } = null; 
        public int? circuitBreakerRequestVolumeThreshold { get; private set; } = null; 
        public int? circuitBreakerSleepWindowInMilliseconds { get; private set; } = null; 
        public int? executionIsolationSemaphoreMaxConcurrentRequests { get; private set; } = null; 
        public ExecutionIsolationStrategy? executionIsolationStrategy { get; private set; } = null; 
        public bool? executionIsolationThreadInterruptOnTimeout { get; private set; } = null; 
        public int? executionTimeoutInMilliseconds { get; private set; } = null; 
        public bool? executionTimeoutEnabled { get; private set; } = null; 
        public int? fallbackIsolationSemaphoreMaxConcurrentRequests { get; private set; } = null; 
        public bool? fallbackEnabled { get; private set; } = null; 
        public int? metricsHealthSnapshotIntervalInMilliseconds { get; private set; } = null; 
        public int? metricsRollingPercentileBucketSize { get; private set; } = null; 
        public bool? metricsRollingPercentileEnabled { get; private set; } = null; 
        public int? metricsRollingPercentileWindowInMilliseconds { get; private set; } = null; 
        public int? metricsRollingPercentileWindowBuckets { get; private set; } = null; 
        public int? metricsRollingStatisticalWindowInMilliseconds { get; private set; } = null; 
        public int? metricsRollingStatisticalWindowBuckets { get; private set; } = null; 
        public bool? requestCacheEnabled { get; private set; } = null; 
        public bool? requestLogEnabled { get; private set; } = null;

        public CommandProperties Build(string commandName)
        {
            var cmd = new CommandProperties(commandName);
            if( circuitBreakerEnabled.HasValue ) cmd.CircuitBreakerEnabled.Set(circuitBreakerEnabled.Value);

            if( circuitBreakerErrorThresholdPercentage.HasValue ) cmd.CircuitBreakerErrorThresholdPercentage.Set(circuitBreakerErrorThresholdPercentage.Value);

            if( circuitBreakerForceClosed.HasValue ) cmd.CircuitBreakerForceClosed.Set(circuitBreakerForceClosed.Value);

            if( circuitBreakerForceOpen.HasValue ) cmd.CircuitBreakerForceOpen.Set(circuitBreakerForceOpen.Value);

            if( circuitBreakerRequestVolumeThreshold.HasValue ) cmd.CircuitBreakerRequestVolumeThreshold.Set(circuitBreakerRequestVolumeThreshold.Value);


            if( circuitBreakerSleepWindowInMilliseconds.HasValue ) cmd.CircuitBreakerSleepWindowInMilliseconds.Set(circuitBreakerSleepWindowInMilliseconds.Value);


            if( executionIsolationSemaphoreMaxConcurrentRequests.HasValue ) cmd.ExecutionIsolationSemaphoreMaxConcurrentRequests.Set(executionIsolationSemaphoreMaxConcurrentRequests.Value);

            if( executionIsolationStrategy.HasValue ) cmd.ExecutionIsolationStrategy.Set(executionIsolationStrategy.Value);

 //           if( executionIsolationThreadInterruptOnTimeout.HasValue ) cmd.executionIsolationThreadInterruptOnTimeout.Set(executionIsolationThreadInterruptOnTimeout.Value);


            if( executionTimeoutInMilliseconds.HasValue ) cmd.ExecutionIsolationThreadTimeoutInMilliseconds.Set(executionTimeoutInMilliseconds.Value);


            if( executionTimeoutEnabled.HasValue ) cmd.ExecutionTimeoutEnabled.Set(executionTimeoutEnabled.Value);


            if( fallbackIsolationSemaphoreMaxConcurrentRequests.HasValue ) cmd.FallbackIsolationSemaphoreMaxConcurrentRequests.Set(fallbackIsolationSemaphoreMaxConcurrentRequests.Value);


            if( fallbackEnabled.HasValue ) cmd.FallbackEnabled.Set(fallbackEnabled.Value);


            if( metricsHealthSnapshotIntervalInMilliseconds.HasValue ) cmd.MetricsHealthSnapshotIntervalInMilliseconds.Set(metricsHealthSnapshotIntervalInMilliseconds.Value);


            if( metricsRollingPercentileBucketSize.HasValue ) cmd.MetricsRollingPercentileBucketSize.Set(metricsRollingPercentileBucketSize.Value);


            if( metricsRollingPercentileEnabled.HasValue ) cmd.MetricsRollingPercentileEnabled.Set(metricsRollingPercentileEnabled.Value);

            
            if( metricsRollingPercentileWindowInMilliseconds.HasValue ) cmd.MetricsRollingPercentileWindowInMilliseconds.Set(metricsRollingPercentileWindowInMilliseconds.Value);


            if( metricsRollingPercentileWindowBuckets.HasValue ) cmd.MetricsRollingPercentileWindowBuckets.Set(metricsRollingPercentileWindowBuckets.Value);


            if( metricsRollingStatisticalWindowInMilliseconds.HasValue ) cmd.MetricsRollingStatisticalWindowInMilliseconds.Set(metricsRollingStatisticalWindowInMilliseconds.Value);


            if( metricsRollingStatisticalWindowBuckets.HasValue ) cmd.MetricsRollingStatisticalWindowBuckets.Set(metricsRollingStatisticalWindowBuckets.Value);


            if( requestCacheEnabled.HasValue ) cmd.RequestCacheEnabled.Set(requestCacheEnabled.Value);


            if( requestLogEnabled.HasValue ) cmd.RequestLogEnabled.Set(requestLogEnabled.Value);


            return cmd;
        }

        public CommandPropertiesBuilder WithCircuitBreakerEnabled(bool? value)
        {
            this.circuitBreakerEnabled = value;
            return this;
        }

        public CommandPropertiesBuilder WithCircuitBreakerErrorThresholdPercentage(int? value)
        {
            this.circuitBreakerErrorThresholdPercentage = value;
            return this;
        }

        public CommandPropertiesBuilder WithCircuitBreakerForceClosed(bool? value)
        {
            this.circuitBreakerForceClosed = value;
            return this;
        }

        public CommandPropertiesBuilder WithCircuitBreakerForceOpen(bool? value)
        {
            this.circuitBreakerForceOpen = value;
            return this;
        }

        public CommandPropertiesBuilder WithCircuitBreakerRequestVolumeThreshold(int? value)
        {
            this.circuitBreakerRequestVolumeThreshold = value;
            return this;
        }

        public CommandPropertiesBuilder WithCircuitBreakerSleepWindowInMilliseconds(int? value)
        {
            this.circuitBreakerSleepWindowInMilliseconds = value;
            return this;
        }

        public CommandPropertiesBuilder WithExecutionIsolationSemaphoreMaxConcurrentRequests(int? value)
        {
            this.executionIsolationSemaphoreMaxConcurrentRequests = value;
            return this;
        }

        public CommandPropertiesBuilder WithExecutionIsolationStrategy(ExecutionIsolationStrategy value)
        {
            this.executionIsolationStrategy = value;
            return this;
        }

        public CommandPropertiesBuilder WithExecutionIsolationThreadInterruptOnTimeout(bool? value)
        {
            this.executionIsolationThreadInterruptOnTimeout = value;
            return this;
        }

        public CommandPropertiesBuilder WithExecutionTimeoutInMilliseconds(int? value)
        {
            this.executionTimeoutInMilliseconds = value;
            return this;
        }

        public CommandPropertiesBuilder WithExecutionTimeoutEnabled(bool? value)
        {
            this.executionTimeoutEnabled = value;
            return this;
        }

        public CommandPropertiesBuilder WithFallbackIsolationSemaphoreMaxConcurrentRequests(int? value)
        {
            this.fallbackIsolationSemaphoreMaxConcurrentRequests = value;
            return this;
        }

        public CommandPropertiesBuilder WithFallbackEnabled(bool? value)
        {
            this.fallbackEnabled = value;
            return this;
        }

        public CommandPropertiesBuilder WithMetricsHealthSnapshotIntervalInMilliseconds(int? value)
        {
            this.metricsHealthSnapshotIntervalInMilliseconds = value;
            return this;
        }

        public CommandPropertiesBuilder WithMetricsRollingPercentileBucketSize(int? value)
        {
            this.metricsRollingPercentileBucketSize = value;
            return this;
        }

        public CommandPropertiesBuilder WithMetricsRollingPercentileEnabled(bool? value)
        {
            this.metricsRollingPercentileEnabled = value;
            return this;
        }

        public CommandPropertiesBuilder WithMetricsRollingPercentileWindowInMilliseconds(int? value)
        {
            this.metricsRollingPercentileWindowInMilliseconds = value;
            return this;
        }

        public CommandPropertiesBuilder WithMetricsRollingPercentileWindowBuckets(int? value)
        {
            this.metricsRollingPercentileWindowBuckets = value;
            return this;
        }

        public CommandPropertiesBuilder WithMetricsRollingStatisticalWindowInMilliseconds(int? value)
        {
            this.metricsRollingStatisticalWindowInMilliseconds = value;
            return this;
        }

        public CommandPropertiesBuilder WithMetricsRollingStatisticalWindowBuckets(int? value)
        {
            this.metricsRollingStatisticalWindowBuckets = value;
            return this;
        }

        public CommandPropertiesBuilder WithRequestCacheEnabled(bool? value)
        {
            this.requestCacheEnabled = value;
            return this;
        }

        public CommandPropertiesBuilder WithRequestLogEnabled(bool? value)
        {
            this.requestLogEnabled = value;
            return this;
        }

    }
}
