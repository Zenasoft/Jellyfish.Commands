// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Jellyfish.Configuration;
using Xunit;

namespace Jellyfish.Commands.Tests
{
    public class CommandPropertiesTest
    {

        /**
         * Utility method for creating baseline properties for unit tests.
         */
        internal static CommandPropertiesBuilder GetUnitTestPropertiesSetter()
        {
            return new CommandPropertiesBuilder()
                    .WithExecutionTimeoutInMilliseconds(1000)// when an execution will be timed out
                    .WithExecutionTimeoutEnabled(true)
                    .WithExecutionIsolationThreadInterruptOnTimeout(true)
                    .WithCircuitBreakerForceOpen(false) // we don't want short-circuiting by default
                    .WithCircuitBreakerErrorThresholdPercentage(40) // % of 'marks' that must be failed to trip the circuit
                    .WithMetricsRollingStatisticalWindowInMilliseconds(5000)// milliseconds back that will be tracked
                    .WithMetricsRollingStatisticalWindowBuckets(5) // buckets
                    .WithCircuitBreakerRequestVolumeThreshold(0) // in testing we will not have a threshold unless we're specifically testing that feature
                    .WithCircuitBreakerSleepWindowInMilliseconds(5000000) // milliseconds after tripping circuit before allowing retry (by default set VERY long as we want it to effectively never allow a singleTest for most unit tests)
                    .WithCircuitBreakerEnabled(true)
                    .WithRequestLogEnabled(true)
                    .WithExecutionIsolationSemaphoreMaxConcurrentRequests(20)
                    .WithFallbackIsolationSemaphoreMaxConcurrentRequests(10)
                    .WithFallbackEnabled(true)
                    .WithCircuitBreakerForceClosed(false)
                    .WithMetricsRollingPercentileEnabled(true)
                    .WithRequestCacheEnabled(true)
                    .WithMetricsRollingPercentileWindowInMilliseconds(60000)
                    .WithMetricsRollingPercentileWindowBuckets(12)
                    .WithMetricsRollingPercentileBucketSize(1000)
                    .WithMetricsHealthSnapshotIntervalInMilliseconds(0);
        }

        [Fact]
        public void testBooleanBuilderOverride1()
        {
            DynamicProperties.Instance.Reset();
            CommandProperties properties = new CommandPropertiesBuilder()
                    .WithCircuitBreakerForceClosed(true)
                    .Build("unitTestPrefix");

            // the builder override should take precedence over the default
            Assert.Equal(true, properties.CircuitBreakerForceClosed.Get());
        }

        [Fact]
        public void testBooleanBuilderOverride2()
        {
            DynamicProperties.Instance.Reset();
            CommandProperties properties = new CommandPropertiesBuilder()
                     .WithCircuitBreakerForceClosed(false).Build("unitTestPrefix");

            // the builder override should take precedence over the default
            Assert.Equal(false, properties.CircuitBreakerForceClosed.Get());
        }

        [Fact]
        public void testBooleanCodeDefault()
        {
            DynamicProperties.Instance.Reset();
            CommandProperties properties = new CommandPropertiesBuilder().Build("unitTestPrefix");
            Assert.Equal(CommandProperties.default_circuitBreakerForceClosed, properties.CircuitBreakerForceClosed.Get());
        }

        [Fact]
        public void testBooleanGlobalDynamicOverrideOfCodeDefault()
        {
            DynamicProperties.Instance.Reset();
            CommandProperties properties = new CommandPropertiesBuilder().Build("unitTestPrefix");
            DynamicProperties.Instance.SetProperty("jellyfish.command.default.circuitBreaker.forceClosed", true);

            // the global dynamic property should take precedence over the default
            Assert.Equal(true, properties.CircuitBreakerForceClosed.Get());

            // cleanup 
            DynamicProperties.Instance.RemoveProperty("jellyFish.command.default.circuitBreaker.forceClosed");
        }

        [Fact]
        public void testBooleanInstanceBuilderOverrideOfGlobalDynamicOverride1()
        {
            DynamicProperties.Instance.Reset();
            CommandProperties properties = new CommandPropertiesBuilder()
                     .WithCircuitBreakerForceClosed(true).Build("unitTestPrefix");
            DynamicProperties.Instance.SetProperty("jellyFish.command.default.circuitBreaker.forceClosed", false);

            // the builder injected should take precedence over the global dynamic property
            Assert.Equal(true, properties.CircuitBreakerForceClosed.Get());

            // cleanup 
            DynamicProperties.Instance.RemoveProperty("jellyFish.command.default.circuitBreaker.forceClosed");
        }

        [Fact]
        public void testBooleanInstanceBuilderOverrideOfGlobalDynamicOverride2()
        {
            DynamicProperties.Instance.Reset();
            CommandProperties properties = new CommandPropertiesBuilder()
                     .WithCircuitBreakerForceClosed(false).Build("unitTestPrefix");
            DynamicProperties.Instance.SetProperty("jellyFish.command.default.circuitBreaker.forceClosed", true);

            // the builder injected should take precedence over the global dynamic property
            Assert.Equal(false, properties.CircuitBreakerForceClosed.Get());

            // cleanup 
            DynamicProperties.Instance.RemoveProperty("jellyFish.command.default.circuitBreaker.forceClosed");
        }

        [Fact]
        public void testBooleanInstanceDynamicOverrideOfEverything()
        {
            DynamicProperties.Instance.Reset();
            CommandProperties properties = new CommandPropertiesBuilder()
                     .WithCircuitBreakerForceClosed(false).Build("TEST");
            DynamicProperties.Instance.SetProperty("jellyFish.command.default.circuitBreaker.forceClosed", false);
            DynamicProperties.Instance.SetProperty("jellyFish.command.TEST.circuitBreaker.forceClosed", true);

            // the instance specific dynamic property should take precedence over everything
            Assert.Equal(true, properties.CircuitBreakerForceClosed.Get());

            // cleanup 
            DynamicProperties.Instance.RemoveProperty("jellyFish.command.default.circuitBreaker.forceClosed");
            DynamicProperties.Instance.RemoveProperty("jellyFish.command.TEST.circuitBreaker.forceClosed");
        }

        [Fact]
        public void testIntegerBuilderOverride()
        {
            DynamicProperties.Instance.Reset();
            CommandProperties properties = new CommandPropertiesBuilder()
                     .WithMetricsRollingStatisticalWindowInMilliseconds(5000).Build("unitTestPrefix");

            // the builder override should take precedence over the default
            Assert.Equal(5000, properties.MetricsRollingStatisticalWindowInMilliseconds.Get());
        }

        [Fact]
        public void testIntegerCodeDefault()
        {
            DynamicProperties.Instance.Reset();
            CommandProperties properties = new CommandPropertiesBuilder().Build("unitTestPrefix");
            Assert.Equal(CommandProperties.default_metricsRollingStatisticalWindow, properties.MetricsRollingStatisticalWindowInMilliseconds.Get());
        }

        [Fact]
        public void testIntegerGlobalDynamicOverrideOfCodeDefault()
        {
            DynamicProperties.Instance.Reset();
            CommandProperties properties = new CommandPropertiesBuilder().Build("unitTestPrefix");
            DynamicProperties.Instance.SetProperty("jellyFish.command.default.metrics.rollingStats.timeInMilliseconds", 1234);

            // the global dynamic property should take precedence over the default
            Assert.Equal(1234, properties.MetricsRollingStatisticalWindowInMilliseconds.Get());

            // cleanup 
            DynamicProperties.Instance.RemoveProperty("jellyFish.command.default.metrics.rollingStats.timeInMilliseconds");
        }

        [Fact]
        public void testIntegerInstanceBuilderOverrideOfGlobalDynamicOverride()
        {
            DynamicProperties.Instance.Reset();
            CommandProperties properties = new CommandPropertiesBuilder()
                     .WithMetricsRollingStatisticalWindowInMilliseconds(5000).Build("unitTestPrefix");
            DynamicProperties.Instance.SetProperty("jellyFish.command.default.rollingStats.timeInMilliseconds", 3456);

            // the builder injected should take precedence over the global dynamic property
            Assert.Equal(5000, properties.MetricsRollingStatisticalWindowInMilliseconds.Get());

            // cleanup 
            DynamicProperties.Instance.RemoveProperty("jellyFish.command.default.rollingStats.timeInMilliseconds");
        }

        [Fact]
        public void testIntegerInstanceDynamicOverrideOfEverything()
        {
            DynamicProperties.Instance.Reset();
            CommandProperties properties = new CommandPropertiesBuilder()
                     .WithMetricsRollingStatisticalWindowInMilliseconds(5000).Build("TEST");
            DynamicProperties.Instance.SetProperty("jellyFish.command.default.metrics.rollingStats.timeInMilliseconds", 1234);
            DynamicProperties.Instance.SetProperty("jellyFish.command.TEST.metrics.rollingStats.timeInMilliseconds", 3456);

            // the instance specific dynamic property should take precedence over everything
            Assert.Equal(3456, properties.MetricsRollingStatisticalWindowInMilliseconds.Get());

            // cleanup 
            DynamicProperties.Instance.RemoveProperty("jellyFish.command.default.metrics.rollingStats.timeInMilliseconds");
            DynamicProperties.Instance.RemoveProperty("jellyFish.command.TEST.metrics.rollingStats.timeInMilliseconds");
        }

    }
}
