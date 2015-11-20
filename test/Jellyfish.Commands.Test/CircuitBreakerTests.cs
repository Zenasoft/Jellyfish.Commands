// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;
using Jellyfish.Commands.CircuitBreaker;
using Jellyfish.Commands.Metrics;
using System.Threading;
using System.Threading.Tasks;
using Jellyfish.Commands.Utils;

namespace Jellyfish.Commands.Tests
{
    ///
    /// A simple circuit breaker intended for unit testing of the {@link Command} object, NOT production use.
    /// <p>
    /// This uses simple logic to 'trip' the circuit after 3 subsequent failures and doesn't recover.
    ///
    internal sealed class TestCircuitBreaker : ICircuitBreaker
    {
        internal CommandMetrics Metrics;
        private bool forceShortCircuit = false;
        
        public TestCircuitBreaker(IClock clock=null)
        {
            Clock = clock ?? Jellyfish.Commands.Utils.Clock.GetInstance();
            this.Metrics = CircuitBreakerTests.getMetrics(CommandPropertiesTest.GetUnitTestPropertiesSetter(), Clock);
            forceShortCircuit = false;
        }

        public TestCircuitBreaker setForceShortCircuit(bool value)
        {
            this.forceShortCircuit = value;
            return this;
        }


        public bool IsOpen()
        {
            if (forceShortCircuit)
            {
                return true;
            }
            else
            {
                return Metrics.GetCumulativeCount(RollingNumberEvent.FAILURE) >= 3;
            }
        }


        public void MarkSuccess()
        {
            // we don't need to do anything since we're going to permanently trip the circuit
        }


        public bool AllowRequest
        {
            get
            {
                return !IsOpen();
            }
        }

        public IClock Clock { get; internal set; }

        public override string ToString()
        {
            return String.Format("TestCircuitBreaker IsOpen={0} Force={1}", IsOpen(), forceShortCircuit);
        }
    }

    public class CircuitBreakerTests
    {
  
        /// 
        /// Test that if all 'marks' are successes during the test window that it does NOT trip the circuit.
        /// Test that if all 'marks' are failures during the test window that it trips the circuit.
        ///
        [Fact]
        public void testTripCircuit()
        {
            var properties = CommandPropertiesTest.GetUnitTestPropertiesSetter();
            var clock = new MockedClock();
            CommandMetrics metrics = getMetrics(properties, clock);
            ICircuitBreaker cb = getCircuitBreaker("KEY_ONE", "OWNER_TWO", metrics, properties, clock);

            metrics.MarkSuccess(1000);
            metrics.MarkSuccess(1000);
            metrics.MarkSuccess(1000);
            metrics.MarkSuccess(1000);

            // this should still allow requests as everything has been successful
            Assert.True(cb.AllowRequest);
            Assert.False(cb.IsOpen());


            // fail
            metrics.MarkFailure(1000);
            metrics.MarkFailure(1000);
            metrics.MarkFailure(1000);
            metrics.MarkFailure(1000);

            // everything has failed in the test window so we should return false now
            Assert.False(cb.AllowRequest);
            Assert.True(cb.IsOpen());

        }

        /// Test that if the % of failures is higher than the threshold that the circuit trips.
        ///
        [Fact]
        public void testTripCircuitOnFailuresAboveThreshold()
        {

            var properties = CommandPropertiesTest.GetUnitTestPropertiesSetter();
            var clock = new MockedClock();
            CommandMetrics metrics = getMetrics(properties, clock);
            var cb = getCircuitBreaker("KEY_ONE", "OWNER_TWO", metrics, properties, clock);

            // this should start as allowing requests
            Assert.True(cb.AllowRequest);
            Assert.False(cb.IsOpen());

            // success with high latency
            metrics.MarkSuccess(400);
            metrics.MarkSuccess(400);
            metrics.MarkFailure(10);
            metrics.MarkSuccess(400);
            metrics.MarkFailure(10);
            metrics.MarkFailure(10);
            metrics.MarkSuccess(400);
            metrics.MarkFailure(10);
            metrics.MarkFailure(10);

            // this should trip the circuit as the error percentage is above the threshold
            Assert.False(cb.AllowRequest);
            Assert.True(cb.IsOpen());
        }

        ///
        /// Test that if the % of failures is higher than the threshold that the circuit trips.
        ///
        [Fact]
        public void testCircuitDoesNotTripOnFailuresBelowThreshold()
        {

            var properties = CommandPropertiesTest.GetUnitTestPropertiesSetter();
            var clock = new MockedClock();
            CommandMetrics metrics = getMetrics(properties, clock);
            ICircuitBreaker cb = getCircuitBreaker("KEY_ONE", "OWNER_TWO", metrics, properties, clock);

            // this should start as allowing requests
            Assert.True(cb.AllowRequest);
            Assert.False(cb.IsOpen());

            // success with high latency
            metrics.MarkSuccess(400);
            metrics.MarkSuccess(400);
            metrics.MarkFailure(10);
            metrics.MarkSuccess(400);
            metrics.MarkSuccess(40);
            metrics.MarkSuccess(400);
            metrics.MarkFailure(10);
            metrics.MarkFailure(10);

            // this should remain open as the failure threshold is below the percentage limit
            Assert.True(cb.AllowRequest);
            Assert.False(cb.IsOpen());
        }

        ///
        /// Test that if all 'marks' are timeouts that it will trip the circuit.
        ///
        [Fact]
        public void testTripCircuitOnTimeouts()
        {

            var properties = CommandPropertiesTest.GetUnitTestPropertiesSetter();
            var clock = new MockedClock();
            CommandMetrics metrics = getMetrics(properties, clock);
            ICircuitBreaker cb = getCircuitBreaker("KEY_ONE", "OWNER_TWO", metrics, properties, clock);

            // this should start as allowing requests
            Assert.True(cb.AllowRequest);
            Assert.False(cb.IsOpen());

            // timeouts
            metrics.MarkTimeout(2000);
            metrics.MarkTimeout(2000);
            metrics.MarkTimeout(2000);
            metrics.MarkTimeout(2000);

            // everything has been a timeout so we should not allow any requests
            Assert.False(cb.AllowRequest);
            Assert.True(cb.IsOpen());

        }

        ///
        /// Test that if the % of timeouts is higher than the threshold that the circuit trips.
        ///
        [Fact]
        public void testTripCircuitOnTimeoutsAboveThreshold()
        {

            var properties = CommandPropertiesTest.GetUnitTestPropertiesSetter();
            var clock = new MockedClock();
            CommandMetrics metrics = getMetrics(properties, clock);
            ICircuitBreaker cb = getCircuitBreaker("KEY_ONE", "OWNER_TWO", metrics, properties, clock);

            // this should start as allowing requests
            Assert.True(cb.AllowRequest);
            Assert.False(cb.IsOpen());

            // success with high latency
            metrics.MarkSuccess(400);
            metrics.MarkSuccess(400);
            metrics.MarkTimeout(10);
            metrics.MarkSuccess(400);
            metrics.MarkTimeout(10);
            metrics.MarkTimeout(10);
            metrics.MarkSuccess(400);
            metrics.MarkTimeout(10);
            metrics.MarkTimeout(10);

            // this should trip the circuit as the error percentage is above the threshold
            Assert.False(cb.AllowRequest);
            Assert.True(cb.IsOpen());

        }

        ///
        /// Test that on an open circuit that a single attempt will be allowed after a window of time to see if issues are resolved.
        ///
        [Fact]
        public void testSingleTestOnOpenCircuitAfterTimeWindow()
        {

            int sleepWindow = 200;
            var properties = CommandPropertiesTest.GetUnitTestPropertiesSetter().WithCircuitBreakerSleepWindowInMilliseconds(sleepWindow);
            var clock = new MockedClock();
            CommandMetrics metrics = getMetrics(properties, clock);
            ICircuitBreaker cb = getCircuitBreaker("KEY_ONE", "OWNER_TWO", metrics, properties, clock);

            // fail
            metrics.MarkFailure(1000);
            metrics.MarkFailure(1000);
            metrics.MarkFailure(1000);
            metrics.MarkFailure(1000);

            // everything has failed in the test window so we should return false now
            Assert.False(cb.AllowRequest);
            Assert.True(cb.IsOpen());

            // wait for sleepWindow to pass
            clock.Increment(sleepWindow + 50);

            // we should now allow 1 request
            Assert.True(cb.AllowRequest);
            // but the circuit should still be open
            Assert.True(cb.IsOpen());
            // and further requests are still blocked
            Assert.False(cb.AllowRequest);
        }

        ///
        /// Test that an open circuit is closed after 1 success.
        ///
        [Fact]
        public void testCircuitClosedAfterSuccess()
        {
            int sleepWindow = 1000;
            var properties = CommandPropertiesTest.GetUnitTestPropertiesSetter()
                                .WithCircuitBreakerSleepWindowInMilliseconds(sleepWindow);
            var clock = new MockedClock();
            CommandMetrics metrics = getMetrics(properties, clock);
            ICircuitBreaker cb = getCircuitBreaker("KEY_ONE", "OWNER_TWO", metrics, properties, clock);

            // fail
            metrics.MarkFailure(1000);
            metrics.MarkFailure(1000);
            metrics.MarkFailure(1000);
            metrics.MarkTimeout(1000);

            // everything has failed in the test window so we should return false now
            Assert.False(cb.AllowRequest);
            Assert.True(cb.IsOpen());

            // wait for sleepWindow to pass
            clock.Increment(sleepWindow + 50);

            // we should now allow 1 request
            Assert.True(cb.AllowRequest);
            // but the circuit should still be open
            Assert.True(cb.IsOpen());
            clock.Increment(sleepWindow);
            // and further requests are still blocked
            Assert.False(cb.AllowRequest);

            // the 'singleTest' succeeds so should cause the circuit to be closed
            metrics.MarkSuccess(500);
            clock.Increment(sleepWindow);
            cb.MarkSuccess();

            // all requests should be open again
            Assert.True(cb.AllowRequest);
            clock.Increment(sleepWindow);
            Assert.True(cb.AllowRequest);
            clock.Increment(sleepWindow);
            Assert.True(cb.AllowRequest);
            // and the circuit should be closed again
            Assert.False(cb.IsOpen());
        }

        ///
        /// Test that an open circuit is closed after 1 success... when the sleepWindow is smaller than the statisticalWindow and 'failure' stats are still sticking around.
        /// <p>
        /// This means that the statistical window needs to be cleared otherwise it will still calculate the failure percentage below the threshold and immediately open the circuit again.
        ///
        [Fact]
        public void testCircuitClosedAfterSuccessAndClearsStatisticalWindow()
        {

            int statisticalWindow = 200;
            int sleepWindow = 10; // this is set very low so that returning from a retry still ends up having data in the buckets for the statisticalWindow
            var properties = CommandPropertiesTest.GetUnitTestPropertiesSetter()
                                    .WithCircuitBreakerSleepWindowInMilliseconds(sleepWindow)
                                    .WithMetricsRollingStatisticalWindowInMilliseconds(statisticalWindow);
            var clock = new MockedClock();
            CommandMetrics metrics = getMetrics(properties, clock);
            ICircuitBreaker cb = getCircuitBreaker("KEY_ONE", "OWNER_TWO", metrics, properties, clock);

            // fail
            metrics.MarkFailure(1000);
            metrics.MarkFailure(1000);
            metrics.MarkFailure(1000);
            metrics.MarkFailure(1000);

            // everything has failed in the test window so we should return false now
            Assert.False(cb.AllowRequest);
            Assert.True(cb.IsOpen());

            // wait for sleepWindow to pass
            clock.Increment(sleepWindow + 50);

            // we should now allow 1 request
            Assert.True(cb.AllowRequest);
            // but the circuit should still be open
            Assert.True(cb.IsOpen());
            // and further requests are still blocked
            Assert.False(cb.AllowRequest);

            // the 'singleTest' succeeds so should cause the circuit to be closed
            metrics.MarkSuccess(500);
            cb.MarkSuccess();

            // all requests should be open again
            Assert.True(cb.AllowRequest);
            Assert.True(cb.AllowRequest);
            Assert.True(cb.AllowRequest);
            // and the circuit should be closed again
            Assert.False(cb.IsOpen());
        }

        ///
        /// Over a period of several 'windows' a single attempt will be made and fail and then finally succeed and close the circuit.
        /// <p>
        /// Ensure the circuit is kept open through the entire testing period and that only the single attempt in each window is made.
        ///
        [Fact]
        public void testMultipleTimeWindowRetriesBeforeClosingCircuit()
        {

            int sleepWindow = 200;
            var properties = CommandPropertiesTest.GetUnitTestPropertiesSetter().WithCircuitBreakerSleepWindowInMilliseconds(sleepWindow);
            var clock = new MockedClock();
            CommandMetrics metrics = getMetrics(properties, clock);
            ICircuitBreaker cb = getCircuitBreaker("KEY_ONE", "OWNER_TWO", metrics, properties, clock);

            // fail
            metrics.MarkFailure(1000);
            metrics.MarkFailure(1000);
            metrics.MarkFailure(1000);
            metrics.MarkFailure(1000);

            // everything has failed in the test window so we should return false now
            Assert.False(cb.AllowRequest);
            Assert.True(cb.IsOpen());

            // wait for sleepWindow to pass
            clock.Increment(sleepWindow + 50);

            // we should now allow 1 request
            Assert.True(cb.AllowRequest);
            // but the circuit should still be open
            Assert.True(cb.IsOpen());
            // and further requests are still blocked
            Assert.False(cb.AllowRequest);

            // the 'singleTest' fails so it should go back to Sleep and not allow any requests again until another 'singleTest' after the Sleep
            metrics.MarkFailure(1000);

            Assert.False(cb.AllowRequest);
            Assert.False(cb.AllowRequest);
            Assert.False(cb.AllowRequest);

            // wait for sleepWindow to pass
            clock.Increment(sleepWindow + 50);

            // we should now allow 1 request
            Assert.True(cb.AllowRequest);
            // but the circuit should still be open
            Assert.True(cb.IsOpen());
            // and further requests are still blocked
            Assert.False(cb.AllowRequest);

            // the 'singleTest' fails again so it should go back to Sleep and not allow any requests again until another 'singleTest' after the Sleep
            metrics.MarkFailure(1000);

            Assert.False(cb.AllowRequest);
            Assert.False(cb.AllowRequest);
            Assert.False(cb.AllowRequest);

            // wait for sleepWindow to pass
            clock.Increment(sleepWindow + 50);

            // we should now allow 1 request
            Assert.True(cb.AllowRequest);
            // but the circuit should still be open
            Assert.True(cb.IsOpen());
            // and further requests are still blocked
            Assert.False(cb.AllowRequest);

            // now it finally succeeds
            metrics.MarkSuccess(200);
            cb.MarkSuccess();

            // all requests should be open again
            Assert.True(cb.AllowRequest);
            Assert.True(cb.AllowRequest);
            Assert.True(cb.AllowRequest);
            // and the circuit should be closed again
            Assert.False(cb.IsOpen());

        }

        ///
        /// When volume of reporting during a statistical window is lower than a defined threshold the circuit
        /// will not trip regardless of whatever statistics are calculated.
        ///
        [Fact]
        public void testLowVolumeDoesNotTripCircuit()
        {

            int sleepWindow = 200;
            int lowVolume = 5;

            var properties = CommandPropertiesTest.GetUnitTestPropertiesSetter().WithCircuitBreakerSleepWindowInMilliseconds(sleepWindow).WithCircuitBreakerRequestVolumeThreshold(lowVolume);
            var clock = new MockedClock();
            CommandMetrics metrics = getMetrics(properties, clock);
            ICircuitBreaker cb = getCircuitBreaker("KEY_ONE", "OWNER_TWO", metrics, properties, clock);

            // fail
            metrics.MarkFailure(1000);
            metrics.MarkFailure(1000);
            metrics.MarkFailure(1000);
            metrics.MarkFailure(1000);

            // even though it has all failed we won't trip the circuit because the volume is low
            Assert.True(cb.AllowRequest);
            Assert.False(cb.IsOpen());

        }

        internal static CommandMetrics getMetrics(CommandPropertiesBuilder properties, IClock clock)
        {
            return new CommandMetrics("KEY_ONE", "GROUP_ONE", properties.Build("KEY_ONE"), clock);
        }

        private static ICircuitBreaker getCircuitBreaker(string key, string commandGroup, CommandMetrics metrics, CommandPropertiesBuilder properties, IClock clock)
        {
            return new DefaultCircuitBreaker(properties.Build(key), metrics, clock);
        }

        // ignoring since this never ends ... useful for testing https://github.com/Netflix//issues/236

        //  [Fact]
        //public void testSuccessClosesCircuitWhenBusy() 
        //    {
        //        Plugins.getInstance().registerCommandExecutionHook(new MyCommandExecutionHook());
        //    try {
        //            performLoad(200, 0, 40);
        //            performLoad(250, 100, 40);
        //            performLoad(600, 0, 40);
        //        } finally {
        //        .reset();
        //        }

        //    }

        async Task performLoad(IJellyfishContext ctx, int totalNumCalls, int errPerc, int waitMillis)
        {

            Random rnd = new Random();

            for (int i = 0; i < totalNumCalls; i++)
            {
                //System.out.println(i);

                try
                {
                    var err = rnd.NextDouble() * 100 < errPerc;

                    TestCommand cmd = new TestCommand(ctx, err);
                    await cmd.ExecuteAsync();

                }
                catch (Exception)
                {
                    //System.err.println(e.getMessage());
                }

                Thread.Sleep(waitMillis);
            }
        }

        public class TestCommand : ServiceCommand<String>
        {

            bool error;

            public TestCommand(IJellyfishContext ctx, bool error) : base( ctx ?? new JellyfishContext(), "group" )
            {
                this.error = error;
            }

            protected override Task<string> Run(CancellationToken token)
            {
                if (error)
                {
                    throw new Exception("forced failure");
                }
                else
                {
                    return Task.FromResult("success");
                }
            }

            protected override Task<string> GetFallback()
            {
                //if (isFailedExecution())
                //{
                //    return getFailedExecutionException().getMessage();
                //}
                //else
                {
               
                    return Task.FromResult( "other fail reason" );
                }
            }

        }
    }
}
