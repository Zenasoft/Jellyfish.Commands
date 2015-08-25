using Jellyfish.Commands.Metrics;
using Jellyfish.Commands.Utils;
using Jellyfish.Configuration;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Jellyfish.Commands.Tests
{
    public class ServiceCommandFixture
    {
        private readonly ITestOutputHelper output;

        public ServiceCommandFixture(ITestOutputHelper output)
        {
            this.output = output;
        }

        /**
         * Test a successful command execution.
         */
        [Fact]
        public async void testExecutionSuccess()
        {
            var ctx = new MockJellyfishContext();
            var command = TestCommandFactory.Get(ctx, output, ExecutionIsolationStrategy.Thread, TestCommandFactory.ExecutionResult.SUCCESS);

            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
            Assert.Equal(TestCommandFactory.EXECUTE_VALUE, await command.ExecuteAsync());
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
            Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));

            Assert.Equal(null, command.FailedExecutionException);

            Assert.True(command.ExecutionTimeInMilliseconds > -1);
            Assert.True(command.IsSuccessfulExecution);

            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

            Assert.Equal(0, command.Metrics.GetHealthCounts().ErrorPercentage);
            Assert.Equal(0, command.Metrics.CurrentConcurrentExecutionCount);

            Assert.Equal(1, ctx.GetRequestLog().GetAllExecutedCommands().Count());
        }

        /**
         * Test that a command can not be executed multiple times.
         */
        [Fact]
        public async void testExecutionMultipleTimes()
        {
            var command = TestCommandFactory.Get(null, output, ExecutionIsolationStrategy.Thread, TestCommandFactory.ExecutionResult.SUCCESS);

            Assert.False(command.IsExecutionComplete);
            // first should succeed
            Assert.Equal(TestCommandFactory.EXECUTE_VALUE, await command.ExecuteAsync());
            Assert.True(command.IsExecutionComplete);
            Assert.True(command.IsExecutedInThread);
            Assert.True(command.ExecutionTimeInMilliseconds > -1);
            Assert.True(command.IsSuccessfulExecution);
            try
            {
                // second should fail
                await command.ExecuteAsync();
                 throw new Exception("we should not allow this ... it breaks the state of request logs");
            }
            catch ( IllegalStateException)
            {

                // we want to get here
            }

            Assert.Equal(0, command.Metrics.CurrentConcurrentExecutionCount);
        }

        /**
         * Test a command execution that throws an Exception and didn't implement getFallback.
         */
        [Fact]
        public async void testExecutionFailureWithNoFallback()
        {
            var ctx = new MockJellyfishContext();
            var command = TestCommandFactory.Get(ctx, output, ExecutionIsolationStrategy.Thread, TestCommandFactory.ExecutionResult.HYSTRIX_FAILURE);

            try
            {
                await command.ExecuteAsync();
                 throw new Exception("we shouldn't get here");
            }
            catch (CommandRuntimeException e)
            {
                Assert.NotNull(e.FallbackException);
                Assert.NotNull(e.CommandName);
                Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
                Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
                Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
                Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
            }
            catch (Exception)
            {
                 throw new Exception("We should always get an RuntimeException when an error occurs.");
            }

            Assert.True(command.ExecutionTimeInMilliseconds > -1);
            Assert.True(command.IsFailedExecution);

            Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
            Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

            Assert.Equal(100, command.Metrics.GetHealthCounts().ErrorPercentage);
            Assert.Equal(0, command.Metrics.CurrentConcurrentExecutionCount);

            Assert.Equal(1, ctx.GetRequestLog().GetAllExecutedCommands().Count());
        }


        /**
         * Test a command execution that throws an unknown exception (not Exception) and didn't implement getFallback.
         */
        [Fact]
        public async void testExecutionFailureWithNoFallback2()
        {
            var ctx = new MockJellyfishContext();
            var command = TestCommandFactory.Get(ctx, output, ExecutionIsolationStrategy.Thread, TestCommandFactory.ExecutionResult.FAILURE);

            try
            {
                await command.ExecuteAsync();
                 throw new Exception("we shouldn't get here");
            }
            catch (CommandRuntimeException e)
            {
                Assert.NotNull(e.FallbackException);
                Assert.NotNull(e.CommandName);

            }
            catch (Exception)
            {
                 throw new Exception("We should always get an RuntimeException when an error occurs.");
            }

            Assert.True(command.ExecutionTimeInMilliseconds > -1);
            Assert.True(command.IsFailedExecution);

            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
            Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
            Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

            Assert.Equal(100, command.Metrics.GetHealthCounts().ErrorPercentage);
            Assert.Equal(0, command.Metrics.CurrentConcurrentExecutionCount);

            Assert.Equal(1, ctx.GetRequestLog().GetAllExecutedCommands().Count());
        }

        /**
         * Test a command execution that fails but has a fallback.
         */
        [Fact]
        public async void testExecutionFailureWithFallback()
        {
            var ctx = new MockJellyfishContext();
            var command = TestCommandFactory.Get(ctx, output, ExecutionIsolationStrategy.Thread, TestCommandFactory.ExecutionResult.FAILURE, TestCommandFactory.FallbackResult.SUCCESS);

            try
            {    
                Assert.Equal(TestCommandFactory.FALLBACK_VALUE, await command.ExecuteAsync());
            }
            catch (Exception)
            {

                 throw new Exception("We should have received a response from the fallback.");
            }

            Assert.Equal("Execution Failure for TestCommand", command.FailedExecutionException.Message);

            Assert.True(command.ExecutionTimeInMilliseconds > -1);
            Assert.True(command.IsFailedExecution);

            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
            Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
            Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

            Assert.Equal(100, command.Metrics.GetHealthCounts().ErrorPercentage);
            Assert.Equal(0, command.Metrics.CurrentConcurrentExecutionCount);

            Assert.Equal(1, ctx.GetRequestLog().GetAllExecutedCommands().Count());
        }

        /**
         * Test a command execution that fails, has getFallback implemented but that fails as well.
         */
        [Fact]
        public async void testExecutionFailureWithFallbackFailure()
        {
            var ctx = new MockJellyfishContext();
            var command = TestCommandFactory.Get(ctx, output, ExecutionIsolationStrategy.Thread, 
                TestCommandFactory.ExecutionResult.FAILURE, TestCommandFactory.FallbackResult.FAILURE);
            try
            {
                await command.ExecuteAsync();
                 throw new Exception("we shouldn't get here");
            }
            catch (CommandRuntimeException e)
            {
                Assert.NotNull(e);
                Assert.NotNull(e.FallbackException);
            }

            Assert.True(command.ExecutionTimeInMilliseconds > -1);
            Assert.True(command.IsFailedExecution);

            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
            Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
            Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
            Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

            Assert.Equal(100, command.Metrics.GetHealthCounts().ErrorPercentage);
            Assert.Equal(0, command.Metrics.CurrentConcurrentExecutionCount);

           Assert.Equal(1, ctx.GetRequestLog().GetAllExecutedCommands().Count());
        }

        /**
         * Test a successful command execution.
         */
        //[Fact]
        //public void testCallbackThreadForSemaphoreIsolation()
        //{
        //    Thread commandThread = null;

        //    var command = TestCommandFactory.Get(output, ExecutionIsolationStrategy.Semaphore);
        //    command.Action = () => { Interlocked.Exchange(ref commandThread, Thread.CurrentThread); return Task.FromResult(0); };

        //    if (!Task.WaitAll(new Task[] { command.ExecuteAsync() }, 2000))
        //         throw new Exception("timed out");


        //    Assert.NotNull(commandThread);

        //    // semaphore should be on the calling thread
        //    Assert.Equal(commandThread.ManagedThreadId, Thread.CurrentThread.ManagedThreadId);
            
        // }

        class TestCircuitBreakerReportsOpenIfForcedOpenClass : ServiceCommand<bool>
        {
            public TestCircuitBreakerReportsOpenIfForcedOpenClass(CommandPropertiesBuilder builder)
                : base(new MockJellyfishContext(), "testCircuitBreakerReportsOpenIfForcedOpen", null, null, ExecutionIsolationStrategy.Thread, builder)
            {
            }

            protected override Task<bool> Run(CancellationToken token)
            {
                return Task.FromResult(true);
            }

            protected override Task<bool> GetFallback()
            {
                return Task.FromResult(false);
            }
        }
        /**
         * Tests that the circuit-breaker reports itself as "OPEN" if set as forced-open
         */
        [Fact]
        public async void testCircuitBreakerReportsOpenIfForcedOpen()
        {
            var builder = new CommandPropertiesBuilder().WithCircuitBreakerForceOpen(true);
            var command = new TestCircuitBreakerReportsOpenIfForcedOpenClass(builder);

            Assert.False(await command.ExecuteAsync()); //fallback should fire
            Assert.True(command.IsCircuitBreakerOpen);
        }

        /*
         * Tests that the circuit-breaker reports itself as "CLOSED" if set as forced-closed
         */
        [Fact]
        public async void testCircuitBreakerReportsClosedIfForcedClosed()
        {
            var command = TestCommandFactory.Get(null, output, ExecutionIsolationStrategy.Thread, TestCommandFactory.ExecutionResult.FAILURE,
                    TestCommandFactory.FallbackResult.SUCCESS, builder => builder.WithCircuitBreakerForceOpen(false).WithCircuitBreakerForceClosed(true));

            Assert.Equal(TestCommandFactory.FALLBACK_VALUE, await command.ExecuteAsync()); //fallback should fire
            Assert.True(!command.IsCircuitBreakerOpen); // NOT THE SAME AS NETFLIX TEST !!!!!!
        }


        /**
         * Test that the circuit-breaker will 'trip' and prevent command execution on subsequent calls.
         */
        [Fact]
        public async void testCircuitBreakerTripsAfterFailures()
        {
            var ctx = new MockJellyfishContext();
            var clock = new MockedClock();
            var circuitBreaker = new TestCircuitBreaker(clock);

            /* fail 3 times and then it should trip the circuit and stop executing */
            // failure 1

            var attempt1 = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Thread, circuitBreaker); 
            await attempt1.ExecuteAsync();
            Assert.True(attempt1.IsResponseFromFallback);
            Assert.False(attempt1.IsCircuitBreakerOpen);
            Assert.False(attempt1.IsResponseShortCircuited);

            clock.Increment(1000);
            // failure 2
            var attempt2 = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Thread, circuitBreaker);
            await attempt2.ExecuteAsync();
            Assert.True(attempt2.IsResponseFromFallback);
            Assert.False(attempt2.IsCircuitBreakerOpen);
            Assert.False(attempt2.IsResponseShortCircuited);

            clock.Increment(1000);
            // failure 3
            var attempt3 = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Thread, circuitBreaker);
            await attempt3.ExecuteAsync();
            Assert.True(attempt3.IsResponseFromFallback);
            Assert.False(attempt3.IsResponseShortCircuited);
            // it should now be 'open' and prevent further executions
            Assert.True(attempt3.IsCircuitBreakerOpen);

            clock.Increment(1000);
            // attempt 4
            var attempt4 = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Thread, circuitBreaker);
            await attempt4.ExecuteAsync();
            Assert.True(attempt4.IsResponseFromFallback);
            // this should now be true as the response will be short-circuited
            Assert.True(attempt4.IsResponseShortCircuited);
            // this should remain open
            Assert.True(attempt4.IsCircuitBreakerOpen);

            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
            Assert.Equal(3, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
            Assert.Equal(4, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

            Assert.Equal(100, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);
            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);

           Assert.Equal(4, ctx.GetRequestLog().GetAllExecutedCommands().Count());
        }


        /**
         * Test that the circuit-breaker is shared across Command objects with the same CommandKey.
         * <p>
         * This will test Command objects with a single circuit-breaker (as if each injected with same CommandKey)
         * <p>
         * Multiple Command objects with the same dependency use the same circuit-breaker.
         */
        [Fact]
        public async void testCircuitBreakerAcrossMultipleCommandsButSameCircuitBreaker()
        {
            var ctx = new MockJellyfishContext();
            var clock = new MockedClock();
            var circuitBreaker = new TestCircuitBreaker(clock);
            
            /* fail 3 times and then it should trip the circuit and stop executing */
            // failure 1
            var attempt1 = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Thread, circuitBreaker);;
            await attempt1.ExecuteAsync();
            Assert.True(attempt1.IsResponseFromFallback);
            Assert.False(attempt1.IsCircuitBreakerOpen);
            Assert.False(attempt1.IsResponseShortCircuited);

            // failure 2 with a different command, same circuit breaker
            var attempt2 = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Thread, circuitBreaker, TestCommandFactory.FallbackResult.UNIMPLEMENTED);
            try
            {
                await attempt2.ExecuteAsync();
            }
            catch (Exception )
            {
                // ignore ... this doesn't have a fallback so will throw an exception
            }
            Assert.True(attempt2.IsFailedExecution);
            Assert.False(attempt2.IsResponseFromFallback); // false because no fallback
            Assert.False(attempt2.IsCircuitBreakerOpen);
            Assert.False(attempt2.IsResponseShortCircuited);

            // failure 3 of the , 2nd for this particular Command
            var attempt3 = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Thread, circuitBreaker);
            await attempt3.ExecuteAsync();
            Assert.True(attempt2.IsFailedExecution);
            Assert.True(attempt3.IsResponseFromFallback);
            Assert.False(attempt3.IsResponseShortCircuited);

            // it should now be 'open' and prevent further executions
            // after having 3 failures on the  that these 2 different Command objects are for
            Assert.True(attempt3.IsCircuitBreakerOpen);

            // attempt 4
            var attempt4 = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Thread, circuitBreaker);
            await attempt4.ExecuteAsync();
            Assert.True(attempt4.IsResponseFromFallback);
            // this should now be true as the response will be short-circuited
            Assert.True(attempt4.IsResponseShortCircuited);
            // this should remain open
            Assert.True(attempt4.IsCircuitBreakerOpen);

            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
            Assert.Equal(3, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
            Assert.Equal(3, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

            Assert.Equal(100, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);
            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);

         Assert.Equal(4, ctx.GetRequestLog().GetAllExecutedCommands().Count());
        }

        /**
                 * Test that the circuit-breaker is different between Command objects with a different .
                 */
        [Fact]
        public async void testCircuitBreakerAcrossMultipleCommandsAndDifferentDependency()
        {
            var clock = new MockedClock();
            var ctx = new MockJellyfishContext();
            TestCircuitBreaker circuitBreaker_one = new TestCircuitBreaker(clock);
            TestCircuitBreaker circuitBreaker_two = new TestCircuitBreaker(clock);
            /* fail 3 times, twice on one , once on a different  ... circuit-breaker should NOT open */

            // failure 1
            var attempt1 = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Thread, circuitBreaker_one);
            await attempt1.ExecuteAsync();
            Assert.True(attempt1.IsResponseFromFallback);
            Assert.False(attempt1.IsCircuitBreakerOpen);
            Assert.False(attempt1.IsResponseShortCircuited);

            // failure 2 with a different Command implementation and different 
            var attempt2 = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Thread, circuitBreaker_two);
            await attempt2.ExecuteAsync();
            Assert.True(attempt2.IsResponseFromFallback);
            Assert.False(attempt2.IsCircuitBreakerOpen);
            Assert.False(attempt2.IsResponseShortCircuited);

            // failure 3 but only 2nd of the .ONE
            var attempt3 = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Thread, circuitBreaker_one);
            await attempt3.ExecuteAsync();
            Assert.True(attempt3.IsResponseFromFallback);
            Assert.False(attempt3.IsResponseShortCircuited);

            // it should remain 'closed' since we have only had 2 failures on .ONE
            Assert.False(attempt3.IsCircuitBreakerOpen);

            // this one should also remain closed as it only had 1 failure for .TWO
            Assert.False(attempt2.IsCircuitBreakerOpen);

            // attempt 4 (3rd attempt for .ONE)
            var attempt4 = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Thread, circuitBreaker_one);
            await attempt4.ExecuteAsync();
            // this should NOW flip to true as this is the 3rd failure for .ONE
            Assert.True(attempt3.IsCircuitBreakerOpen);
            Assert.True(attempt3.IsResponseFromFallback);
            Assert.False(attempt3.IsResponseShortCircuited);

            // .TWO should still remain closed
            Assert.False(attempt2.IsCircuitBreakerOpen);

            Assert.Equal(0, circuitBreaker_one.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
            Assert.Equal(0, circuitBreaker_one.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
            Assert.Equal(3, circuitBreaker_one.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
            Assert.Equal(0, circuitBreaker_one.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
            Assert.Equal(0, circuitBreaker_one.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
            Assert.Equal(0, circuitBreaker_one.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
            Assert.Equal(3, circuitBreaker_one.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
            Assert.Equal(0, circuitBreaker_one.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
            Assert.Equal(0, circuitBreaker_one.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
            Assert.Equal(0, circuitBreaker_one.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
            Assert.Equal(0, circuitBreaker_one.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
            Assert.Equal(0, circuitBreaker_one.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

            Assert.Equal(100, circuitBreaker_one.Metrics.GetHealthCounts().ErrorPercentage);
            Assert.Equal(0, circuitBreaker_one.Metrics.CurrentConcurrentExecutionCount);

            Assert.Equal(0, circuitBreaker_two.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
            Assert.Equal(0, circuitBreaker_two.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
            Assert.Equal(1, circuitBreaker_two.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
            Assert.Equal(0, circuitBreaker_two.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
            Assert.Equal(0, circuitBreaker_two.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
            Assert.Equal(0, circuitBreaker_two.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
            Assert.Equal(1, circuitBreaker_two.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
            Assert.Equal(0, circuitBreaker_two.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
            Assert.Equal(0, circuitBreaker_two.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
            Assert.Equal(0, circuitBreaker_two.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
            Assert.Equal(0, circuitBreaker_two.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
            Assert.Equal(0, circuitBreaker_two.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

            Assert.Equal(100, circuitBreaker_two.Metrics.GetHealthCounts().ErrorPercentage);
            Assert.Equal(0, circuitBreaker_two.Metrics.CurrentConcurrentExecutionCount);

           Assert.Equal(4, ctx.GetRequestLog().GetAllExecutedCommands().Count());
        }

        /**
         * Test that the circuit-breaker being disabled doesn't wreak havoc.
         */
        [Fact]
        public async void testExecutionSuccessWithCircuitBreakerDisabled()
        {
            var ctx = new MockJellyfishContext();
            var clock = new MockedClock();
            var circuitBreaker = new TestCircuitBreaker(clock);
            var command = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Thread, circuitBreaker,
                executionResult: TestCommandFactory.ExecutionResult.SUCCESS,
                setter: builder => builder.WithCircuitBreakerEnabled(false));

            try
            {
                Assert.Equal(TestCommandFactory.EXECUTE_VALUE, await command.ExecuteAsync());
            }
            catch (Exception )
            {

                 throw new Exception("We received an exception.");
            }

            // we'll still get metrics ... just not the circuit breaker opening/closing
            Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

            Assert.Equal(0, command.Metrics.GetHealthCounts().ErrorPercentage);
            Assert.Equal(0, command.Metrics.CurrentConcurrentExecutionCount);

        Assert.Equal(1, ctx.GetRequestLog().GetAllExecutedCommands().Count());
        }

        /**
                 * Test a command execution timeout where the command didn't implement getFallback.
                 */
        [Fact]
        public async void testExecutionTimeoutWithNoFallback()
        {
            var ctx = new MockJellyfishContext();
            var circuitBreaker = new TestCircuitBreaker(Clock.GetInstance());
            var command = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Thread, circuitBreaker,
                executionResult: TestCommandFactory.ExecutionResult.SUCCESS, executionLatency: 200, 
                fallbackResult: TestCommandFactory.FallbackResult.UNIMPLEMENTED, fallbackLatency: 50,
                setter: builder => builder.WithExecutionTimeoutInMilliseconds(100));

            try
            {
                await command.ExecuteAsync();
                throw new Exception("we shouldn't get here");
            }
            catch (CommandRuntimeException e)
            {
                var de = (CommandRuntimeException)e;
                Assert.NotNull(de.FallbackException);
                Assert.True(de.FallbackException is NotImplementedException);
                Assert.NotNull(de.CommandName);
                Assert.NotNull(de.InnerException);
                Assert.True(de.InnerException is TimeoutException); // timeout
            }
            catch (Exception)
            {
                throw new Exception("the exception should be RuntimeException");
            }

            // the time should be 50+ since we timeout at 50ms
            Assert.True(command.ExecutionTimeInMilliseconds >= 50);

            Assert.True(command.IsResponseTimedOut);
            Assert.False(command.IsResponseFromFallback);
            Assert.False(command.IsResponseRejected);

            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
            Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
            Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));
            Assert.Equal(100,command.Metrics.GetHealthCounts().ErrorPercentage);
            Assert.Equal(0, command.Metrics.CurrentConcurrentExecutionCount);

           Assert.Equal(1, ctx.GetRequestLog().GetAllExecutedCommands().Count());
        }


        [Fact]
        public async void testSemaphoreExecutionWithTimeout()
        {
            var ctx = new MockJellyfishContext();
            var circuitBreaker = new TestCircuitBreaker(Clock.GetInstance());
            var cmd = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Semaphore, circuitBreaker,
                executionResult: TestCommandFactory.ExecutionResult.SUCCESS, executionLatency: 2000,
                fallbackResult: TestCommandFactory.FallbackResult.UNIMPLEMENTED, fallbackLatency: 50);

            try
            {
                await cmd.ExecuteAsync();
                 throw new Exception("Should throw");
            }
            catch (Exception)
            {
                Assert.Equal(0, cmd.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
                Assert.Equal(1, cmd.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
                Assert.Equal(0, cmd.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
                Assert.Equal(0, cmd.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
                Assert.Equal(0, cmd.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
                Assert.Equal(0, cmd.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
                Assert.Equal(0, cmd.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
                Assert.Equal(0, cmd.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
                Assert.Equal(0, cmd.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
                Assert.Equal(0, cmd.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
                Assert.Equal(1, cmd.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
                Assert.Equal(0, cmd.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));
              Assert.Equal(100, cmd.Metrics.GetHealthCounts().ErrorPercentage);
                Assert.Equal(0, cmd.Metrics.CurrentConcurrentExecutionCount);

            Assert.Equal(1, ctx.GetRequestLog().GetAllExecutedCommands().Count());
            }

        }

        /**
         * Test a recoverable java.lang.Error being thrown with no fallback
         */
        //[Fact]
        //public void testRecoverableErrorWithNoFallbackThrowsError()
        //{
        //    var command = getRecoverableErrorCommand(ExecutionIsolationStrategy.Thread, AbstractTestCommand.FallbackResult.UNIMPLEMENTED);
        //    try
        //    {
        //        await command.ExecuteAsync();
        //        // new Exception("we expect to receive a " + Error.class.GetSimpleName());
        //    }
        //    catch (Exception e)
        //    {
        //        // the actual error is an extra cause level deep because  needs to wrap Throwable/Error as it's public
        //        // methods only support Exception and it's not a strong enough reason to break backwards compatibility and jump to version 2.x
        //        // so RuntimeException -> wrapper Exception -> actual Error
        //        Assert.Equal("Execution ERROR for TestCommand", e.InnerException.Message);
        //    }

        //    Assert.Equal("Execution ERROR for TestCommand", command.GetFailedExecutionException().GetCause().GetMessage());

        //    Assert.True(command.ExecutionTimeInMilliseconds > -1);
        //    Assert.True(command.IsFailedExecution);

        //    Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //    Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //    Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
        //    Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));

        //    Assert.Equal(0, command.Metrics.CurrentConcurrentExecutionCount);
        //}

        /**
         * Test a command execution timeout where the command implemented getFallback.
         */
        [Fact]
        public async void testExecutionTimeoutWithFallback()
        {
            var ctx = new MockJellyfishContext();
            var circuitBreaker = new TestCircuitBreaker(Clock.GetInstance());
            var command = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Thread, circuitBreaker,
                executionResult: TestCommandFactory.ExecutionResult.SUCCESS, executionLatency: 200,
                fallbackResult: TestCommandFactory.FallbackResult.SUCCESS, fallbackLatency: 50,
                setter: builder => builder.WithExecutionTimeoutInMilliseconds(100));

            try
            {
                Assert.Equal(TestCommandFactory.FALLBACK_VALUE, await command.ExecuteAsync());
                // the time should be 50+ since we timeout at 50ms
                Assert.True( command.ExecutionTimeInMilliseconds >= 50);
                Assert.False(command.IsCircuitBreakerOpen);
                Assert.False(command.IsResponseShortCircuited);
                Assert.True(command.IsResponseTimedOut);
                Assert.True(command.IsResponseFromFallback);
            }
            catch (Exception )
            {
                 throw new Exception("We should have received a response from the fallback.");
            }

            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
            Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
            Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

            Assert.Equal(100, command.Metrics.GetHealthCounts().ErrorPercentage);
            Assert.Equal(0, command.Metrics.CurrentConcurrentExecutionCount);

          Assert.Equal(1, ctx.GetRequestLog().GetAllExecutedCommands().Count());
        }

        /**
         * Test a command execution timeout where the command implemented getFallback but it fails.
         */
        [Fact]
        public async void testExecutionTimeoutFallbackFailure()
        {
            var ctx = new MockJellyfishContext();
            var circuitBreaker = new TestCircuitBreaker(Clock.GetInstance());
            var command = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Thread, circuitBreaker,
                executionResult: TestCommandFactory.ExecutionResult.SUCCESS, executionLatency: 200,
                fallbackResult: TestCommandFactory.FallbackResult.FAILURE, fallbackLatency: 50,
                setter : (builder) => builder.WithExecutionTimeoutInMilliseconds(100));

            try
            {
                await command.ExecuteAsync();
                throw new Exception("we shouldn't get here");
            }
            catch (CommandRuntimeException e)
            {
                Assert.NotNull(e.FallbackException);
                Assert.False(e.FallbackException is NotImplementedException);
                Assert.NotNull(e.CommandName);
                Assert.NotNull(e.InnerException);
                Assert.True(e.InnerException is TimeoutException);
            }
            catch (Exception)
            {
                throw new Exception("the exception should be RuntimeException");
            }
            
            // the time should be 50+ since we timeout at 50ms
            Assert.True( command.ExecutionTimeInMilliseconds >= 50);
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
            Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
            Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
            Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

            Assert.Equal(100, command.Metrics.GetHealthCounts().ErrorPercentage);
            Assert.Equal(0, command.Metrics.CurrentConcurrentExecutionCount);

          Assert.Equal(1, ctx.GetRequestLog().GetAllExecutedCommands().Count());
        }

        /**
         * Test that the circuit-breaker counts a command execution timeout as a 'timeout' and not just failure.
         */
        [Fact]
        public async void testShortCircuitFallbackCounter()
        {
            var ctx = new MockJellyfishContext();
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker().setForceShortCircuit(true);
            var command = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Thread, circuitBreaker,
                                    executionResult: TestCommandFactory.ExecutionResult.FAILURE, 
                                    fallbackResult: TestCommandFactory.FallbackResult.SUCCESS);

            try
            {
                await command.ExecuteAsync();

                Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));

                command = getSharedCircuitBreakerCommand(ctx,ExecutionIsolationStrategy.Thread, circuitBreaker,
                                    executionResult: TestCommandFactory.ExecutionResult.FAILURE,
                                    fallbackResult: TestCommandFactory.FallbackResult.SUCCESS);
                await command.ExecuteAsync();
                Assert.Equal(2, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));

                // will be -1 because it never attempted execution
                Assert.True(command.ExecutionTimeInMilliseconds == -1);
                Assert.True(command.IsResponseShortCircuited);
                Assert.False(command.IsResponseTimedOut);

                // because it was short-circuited to a fallback we don't count an error
                Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
                Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
                Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));

            }
            catch (Exception )
            {
                throw new Exception("We should have received a response from the fallback.");
            }

            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
            Assert.Equal(2, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
            Assert.Equal(2, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

            Assert.Equal(100, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);
            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);

            Assert.Equal(2, ctx.GetRequestLog().GetAllExecutedCommands().Count());
        }


        /**
         * Test when a command fails to get queued up in the threadpool where the command didn't implement getFallback.
         * <p>
         * We specifically want to protect against developers getting random thread exceptions and instead just correctly receiving RuntimeException when no fallback exists.
         */

        [Fact]
        private async void testRejectedThreadWithNoFallback()
        { 
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            var ctx = new MockJellyfishContext();
            var taskScheduler =  new BulkheadTaskScheduler(1, "test");
            TestServiceCommand command = null;
            Task<int> f=null;
            try
            {
                command = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Thread, circuitBreaker,
                        executionResult: TestCommandFactory.ExecutionResult.SUCCESS, executionLatency: 500,
                        fallbackResult: TestCommandFactory.FallbackResult.UNIMPLEMENTED);
                command.TaskScheduler = taskScheduler;
                f = command.ExecuteAsync();

                command = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Thread, circuitBreaker,
                        executionResult: TestCommandFactory.ExecutionResult.SUCCESS, executionLatency: 0,
                        fallbackResult: TestCommandFactory.FallbackResult.UNIMPLEMENTED);
                command.TaskScheduler = taskScheduler;
                await command.ExecuteAsync();

                throw new Exception("we shouldn't get here");
            }
            catch (CommandRuntimeException e)
            {
                // will be -1 because it never attempted execution
                Assert.Equal(-1, command.ExecutionTimeInMilliseconds);
                Assert.True(command.IsResponseRejected);
                Assert.False(command.IsResponseShortCircuited);
                Assert.False(command.IsResponseTimedOut);

                Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
                Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
                Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
                Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));

                Assert.True(e.InnerException is RejectedExecutionException);

                Assert.NotNull(e.FallbackException);
                Assert.True(e.FallbackException is NotImplementedException);
                Assert.NotNull(e.CommandName);
                Assert.NotNull(e.InnerException);
            }
            catch (Exception)
            {
                throw new Exception("the exception should be RuntimeException with cause as RejectedExecutionException");
            }
            

            try
            {
                Assert.Equal(TestCommandFactory.EXECUTE_VALUE, f.Result);
            }
            catch (Exception)
            {
                throw new Exception("The first one should succeed.");
            }

            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

            Assert.Equal(50, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);
            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);

            Assert.Equal(2, ctx.GetRequestLog().GetAllExecutedCommands().Count());
        }

        /**
         * Test when a command fails to get queued up in the threadpool where the command implemented getFallback.
         * <p>
         * We specifically want to protect against developers getting random thread exceptions and instead just correctly receives a fallback.
         */
        [Fact]
        public async void testRejectedThreadWithFallback()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();

            var taskScheduler = new BulkheadTaskScheduler(1, "test");
            var ctx = new MockJellyfishContext();
            try
            {
                var command1 = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Thread, circuitBreaker,
                        executionResult: TestCommandFactory.ExecutionResult.SUCCESS, executionLatency: 500,
                        fallbackResult: TestCommandFactory.FallbackResult.SUCCESS,
                setter: builder => builder.WithExecutionTimeoutInMilliseconds(100));
                command1.TaskScheduler = taskScheduler;
                var f = command1.ExecuteAsync();

                var command2 = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Thread, circuitBreaker,
                        executionResult: TestCommandFactory.ExecutionResult.SUCCESS, executionLatency: 0,
                        fallbackResult: TestCommandFactory.FallbackResult.SUCCESS);
                command2.TaskScheduler = taskScheduler;

                Assert.Equal(TestCommandFactory.FALLBACK_VALUE, await command2.ExecuteAsync());
                Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
                Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
                Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
                Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
                Assert.False(command1.IsResponseRejected);
                Assert.False(command1.IsResponseFromFallback);
                Assert.True(command2.IsResponseRejected);
                Assert.True(command2.IsResponseFromFallback);
            }
            catch (Exception e)
            {

                throw new Exception("We should have received a response from the fallback. " + e.InnerException);
            }

            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

            Assert.Equal(100, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);
            Assert.Equal(1, circuitBreaker.Metrics.CurrentConcurrentExecutionCount); //pool-filler still going

            Assert.Equal(2, ctx.GetRequestLog().GetAllExecutedCommands().Count());
        }

        /**
         * Test when a command fails to get queued up in the threadpool where the command implemented getFallback but it fails.
         * <p>
         * We specifically want to protect against developers getting random thread exceptions and instead just correctly receives an RuntimeException.
         */
        [Fact]
        public async void testRejectedThreadWithFallbackFailure()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            var ctx = new MockJellyfishContext();
            var taskScheduler = new BulkheadTaskScheduler(1, "test");

            try
            {
                var command = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Thread, circuitBreaker,
                        executionResult: TestCommandFactory.ExecutionResult.SUCCESS, executionLatency: 500,
                        fallbackResult: TestCommandFactory.FallbackResult.FAILURE);
                command.TaskScheduler = taskScheduler;
                var f = command.ExecuteAsync();

                command = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Thread, circuitBreaker,
                        executionResult: TestCommandFactory.ExecutionResult.SUCCESS, executionLatency: 0,
                        fallbackResult: TestCommandFactory.FallbackResult.FAILURE);
                command.TaskScheduler = taskScheduler;
                await command.ExecuteAsync();

                Assert.Equal(TestCommandFactory.FALLBACK_VALUE, await command.ExecuteAsync());
                throw new Exception("we shouldn't get here");
            }
            catch (CommandRuntimeException e)
            {
                Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
                Assert.True(e.InnerException is RejectedExecutionException);
                Assert.NotNull(e.FallbackException);
                Assert.False(e.FallbackException is NotImplementedException);
                Assert.NotNull(e.CommandName);
                Assert.NotNull(e.InnerException);
            }
            catch (Exception)
            {
                throw new Exception("the exception should be RuntimeException with cause as RejectedExecutionException");
            }
            

            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

            Assert.Equal(100, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);
            Assert.Equal(1, circuitBreaker.Metrics.CurrentConcurrentExecutionCount); //pool-filler still going

                Assert.Equal(2, ctx.GetRequestLog().GetAllExecutedCommands().Count());
        }

        /**
         * If it has been sitting in the queue, it should not execute if timed out by the time it hits the queue.
         */
        [Fact]
        public void testTimedOutCommandDoesNotExecute()
        {
            var ctx = new MockJellyfishContext();
            TestCircuitBreaker s1 = new TestCircuitBreaker(new MockedClock());
            TestCircuitBreaker s2 = new TestCircuitBreaker(new MockedClock());

            // execution will take 100ms, thread pool has a 600ms timeout
            var c1 = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Thread, s1,
                       executionResult: TestCommandFactory.ExecutionResult.SUCCESS, executionLatency: 100,
                       fallbackResult: TestCommandFactory.FallbackResult.FAILURE,
                       setter: (CommandPropertiesBuilder builder) => builder.WithExecutionTimeoutInMilliseconds(600));

            // execution will take 200ms, thread pool has a 20ms timeout
            var c2 = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Thread, s2,
                    executionResult: TestCommandFactory.ExecutionResult.SUCCESS, executionLatency: 200,
                    fallbackResult: TestCommandFactory.FallbackResult.FAILURE,                    
                    setter: (CommandPropertiesBuilder builder) => builder.WithExecutionTimeoutInMilliseconds(20));

            // queue up c1 first
            var c1f = c1.ExecuteAsync();
            // now queue up c2 and wait on it
            bool receivedException = false;
            Task<int> c2f = c2.ExecuteAsync();

            // c1 will complete after 100ms
            int c1DidExecute=0;
            try
            {
                c1DidExecute = c1f.Result;
            }
            catch (Exception)
            {
                throw new Exception("we should not have failed while getting c1");
            }

            // c1 is expected to executed b
            Assert.Equal(TestCommandFactory.EXECUTE_VALUE, c1DidExecute);

            // c2 will timeout after 20 ms ... we'll wait longer than the 200ms time to make sure
            // the thread doesn't keep running in the background and execute
            try
            {
                Thread.Sleep(400);
            }
            catch (Exception)
            {
                throw new Exception("Failed to sleep");
            }
            
            // c2 is not expected to execute
            try { 
                Assert.NotEqual(TestCommandFactory.EXECUTE_VALUE, c2f.Result);
            }
            catch (Exception)
            {
                // we expect to get an exception here
                receivedException = true;
            }

            if (!receivedException)
            {
                throw new Exception("We expect to receive an exception for c2 as it's supposed to timeout.");
            }

            Assert.Equal(1, s1.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
            Assert.Equal(0, s1.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
            Assert.Equal(0, s1.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
            Assert.Equal(0, s1.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
            Assert.Equal(0, s1.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
            Assert.Equal(0, s1.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
            Assert.Equal(0, s1.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
            Assert.Equal(0, s1.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
            Assert.Equal(0, s1.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
            Assert.Equal(0, s1.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
            Assert.Equal(0, s1.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
            Assert.Equal(0, s1.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

            Assert.Equal(0, s1.Metrics.GetHealthCounts().ErrorPercentage);
            Assert.Equal(0, s1.Metrics.CurrentConcurrentExecutionCount);

            Assert.Equal(0, s2.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
            Assert.Equal(1, s2.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
            Assert.Equal(0, s2.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
            Assert.Equal(0, s2.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
            Assert.Equal(0, s2.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
            Assert.Equal(0, s2.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
            Assert.Equal(0, s2.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
            Assert.Equal(0, s2.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
            Assert.Equal(0, s2.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
            Assert.Equal(0, s2.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
            Assert.Equal(1, s2.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
            Assert.Equal(0, s2.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

            Assert.Equal(100, s2.Metrics.GetHealthCounts().ErrorPercentage);
            Assert.Equal(0, s2.Metrics.CurrentConcurrentExecutionCount);

            Assert.Equal(2, ctx.GetRequestLog().GetAllExecutedCommands().Count());
        }

        [Fact]
        public async void testDisabledTimeoutWorks()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            var ctx = new MockJellyfishContext();
            var command = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Thread, circuitBreaker,
                        executionResult: TestCommandFactory.ExecutionResult.SUCCESS, executionLatency: 900,
                        fallbackResult: TestCommandFactory.FallbackResult.SUCCESS,
                        setter : (CommandPropertiesBuilder builder) => builder.WithExecutionTimeoutInMilliseconds(100)
                                                                              .WithExecutionTimeoutEnabled(false));
            try
            {
               Assert.Equal(TestCommandFactory.EXECUTE_VALUE, await command.ExecuteAsync());
            }
            catch (Exception )
            {
                throw new Exception("should not fail");
            }

            Assert.False(command.IsResponseTimedOut);
            Assert.True(command.ExecutionTimeInMilliseconds >= 900);
        }

        [Fact]
        public async void testFallbackSemaphore()
        {
            var ctx = new MockJellyfishContext();
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker(new MockedClock());
            // single thread should work
            var command = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Semaphore, circuitBreaker,
                                executionResult: TestCommandFactory.ExecutionResult.FAILURE,
                                fallbackResult: TestCommandFactory.FallbackResult.SUCCESS,
                                setter: (CommandPropertiesBuilder builder) => builder.WithFallbackIsolationSemaphoreMaxConcurrentRequests(1)
                                                                                     .WithExecutionIsolationThreadInterruptOnTimeout(false));
            try
            {
                Assert.Equal(TestCommandFactory.FALLBACK_VALUE, await command.ExecuteAsync());
            }
            catch (Exception)
            {
                // we shouldn't fail on this one
                throw;
            }
            Thread.Sleep(50);

            // 2 threads, the second should be rejected by the fallback semaphore
            var exceptionReceived = false;
            Task<int> f = null;
            try
            {
                //  Console.WriteLine("c2 start: " + System.currentTimeMillis());
                command = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Semaphore, circuitBreaker,
                      executionResult: TestCommandFactory.ExecutionResult.FAILURE,
                      fallbackResult: TestCommandFactory.FallbackResult.SUCCESS, fallbackLatency: 200,
                      setter: (CommandPropertiesBuilder builder) => builder.WithFallbackIsolationSemaphoreMaxConcurrentRequests(1)
                                                                           .WithExecutionIsolationThreadInterruptOnTimeout(false));

                f = command.ExecuteAsync();
              //  Console.WriteLine("c2 after queue: " + System.currentTimeMillis());
                // make sure that thread gets a chance to run before queuing the next one
                Thread.Sleep(20);
                //   Console.WriteLine("c3 start: " + System.currentTimeMillis());
                command = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Semaphore, circuitBreaker,
                       executionResult: TestCommandFactory.ExecutionResult.FAILURE,
                       fallbackResult: TestCommandFactory.FallbackResult.SUCCESS, fallbackLatency: 200,
                       setter: (CommandPropertiesBuilder builder) => builder.WithFallbackIsolationSemaphoreMaxConcurrentRequests(1)
                                                                            .WithExecutionIsolationThreadInterruptOnTimeout(false));
                //  Console.WriteLine("c3 after queue: " + System.currentTimeMillis());
                await command.ExecuteAsync();
            }
            catch (Exception)
            {
                exceptionReceived = true;
            }

            Assert.Equal(TestCommandFactory.FALLBACK_VALUE, f.Result);

            if (!exceptionReceived)
            {
                throw new Exception("We expected an exception on the 2nd get");
            }

            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
            // TestSemaphoreCommandWithSlowFallback always fails so all 3 should show failure
            Assert.Equal(3, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
            // the 1st thread executes single-threaded and gets a fallback, the next 2 are concurrent so only 1 of them is permitted by the fallback semaphore so 1 is rejected
            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
            // whenever a fallback_rejection occurs it is also a fallback_failure
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
            Assert.Equal(2, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
            // we should not have rejected any via the "execution semaphore" but instead via the "fallback semaphore"
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
            // the rest should not be involved in this test
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);

            Assert.Equal(3, ctx.GetRequestLog().GetAllExecutedCommands().Count());
        }

        [Fact]
        public async void testExecutionSemaphoreWithQueue()
        {
            var ctx = new MockJellyfishContext();
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            // single thread should work
            var command = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Semaphore, circuitBreaker,
                                executionResult: TestCommandFactory.ExecutionResult.SUCCESS, executionLatency:200,
                                fallbackResult: TestCommandFactory.FallbackResult.UNIMPLEMENTED,
                                setter: (CommandPropertiesBuilder builder) => builder.WithFallbackIsolationSemaphoreMaxConcurrentRequests(1));            // single thread should work
            try
            {
                Assert.Equal(TestCommandFactory.EXECUTE_VALUE, await command.ExecuteAsync());
            }
            catch (Exception)
            {
                // we shouldn't fail on this one
                throw;
            }

            bool exceptionReceived=false;

            var semaphore =  new TryableSemaphoreActual(DynamicProperties.Factory.AsProperty(1));

            var tasks = new Task[2];
            for (int i = 0; i < 2; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    try
                    {
                        await getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Semaphore, circuitBreaker,
                            executionResult: TestCommandFactory.ExecutionResult.SUCCESS, executionLatency: 200,
                            fallbackResult: TestCommandFactory.FallbackResult.UNIMPLEMENTED,
                            semaphore: semaphore).ExecuteAsync();
                    }
                    catch (Exception)
                    {
                        exceptionReceived = true;
                    }
                });
            }

            // 2 threads, the second should be rejected by the semaphore
            Task.WaitAll(tasks);

            if (!exceptionReceived)
            {
                throw new Exception("We expected an exception on the 2nd get");
            }

            Assert.Equal(2, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
            // we don't have a fallback so threw an exception when rejected
            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
            // not a failure as the command never executed so can't fail
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
            // no fallback failure as there isn't a fallback implemented
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
            // we should have rejected via semaphore
            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
            // the rest should not be involved in this test
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);

            Assert.Equal(3, ctx.GetRequestLog().GetAllExecutedCommands().Count());
        }

        [Fact]
        public async void testExecutionSemaphoreWithExecution()
        {
            var ctx = new MockJellyfishContext();
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            // single thread should work
            var command = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Semaphore, circuitBreaker,
                    executionResult: TestCommandFactory.ExecutionResult.SUCCESS, executionLatency: 200,
                    fallbackResult: TestCommandFactory.FallbackResult.UNIMPLEMENTED,
                    setter: (CommandPropertiesBuilder builder) => builder.WithFallbackIsolationSemaphoreMaxConcurrentRequests(1));            // single thread should work

            try
            {
                Assert.Equal(TestCommandFactory.EXECUTE_VALUE, await command.ExecuteAsync());
                Assert.False(command.IsExecutedInThread);
            }
            catch (Exception)
            {
                // we shouldn't fail on this one
                throw;
            }

            var semaphore = new TryableSemaphoreActual(DynamicProperties.Factory.AsProperty(1));
            var tasks = new Task[2];

            var results = new System.Collections.Concurrent.ConcurrentQueue<int>();

            bool exceptionReceived = false;
            for (int i = 0; i < 2; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    try
                    {
                        results.Enqueue(
                            await getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Semaphore, circuitBreaker,
                                executionResult: TestCommandFactory.ExecutionResult.SUCCESS, executionLatency: 200,
                                fallbackResult: TestCommandFactory.FallbackResult.UNIMPLEMENTED,
                                semaphore: semaphore).ExecuteAsync());
                    }
                    catch (Exception)
                    {
                        exceptionReceived = true;
                    }
                });
            }
            // 2 threads, the second should be rejected by the semaphore
            Task.WaitAll(tasks);

            if (!exceptionReceived)
            {
                throw new Exception("We expected an exception on the 2nd get");
            }

            // only 1 value is expected as the other should have thrown an exception
            Assert.Equal(1, results.Count());

            Assert.Equal(2, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
            // no failure ... we throw an exception because of rejection but the command does not fail execution
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
            // there is no fallback implemented so no failure can occur on it
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
            // we rejected via semaphore
            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
            // the rest should not be involved in this test
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);

            Assert.Equal(3, ctx.GetRequestLog().GetAllExecutedCommands().Count());
        }


        [Fact]
        public void testRejectedExecutionSemaphoreWithFallbackViaExecute()
        {
            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
            var ctx = new MockJellyfishContext();
            var tasks = new Task[2];
            var results = new System.Collections.Concurrent.ConcurrentQueue<int>();

            bool exceptionReceived = false;
            for (int i = 0; i < 2; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    try
                    {
                        var cmd = getSharedCircuitBreakerCommand(ctx, ExecutionIsolationStrategy.Semaphore, circuitBreaker,
                                executionResult: TestCommandFactory.ExecutionResult.SUCCESS, executionLatency: 200,
                                fallbackResult: TestCommandFactory.FallbackResult.SUCCESS,
                                setter: (CommandPropertiesBuilder builder) => builder.WithExecutionIsolationSemaphoreMaxConcurrentRequests(1)
                        );
                        results.Enqueue(await cmd.ExecuteAsync());
                    }
                    catch (Exception)
                    {
                        exceptionReceived = true;
                    }
                });
            }
            // 2 threads, the second should be rejected by the semaphore
            Task.WaitAll(tasks);

            if (exceptionReceived)
            {
                throw new Exception("We should have received a fallback response");
            }

            // both threads should have returned values
            Assert.Equal(2, results.Count());
            int r;
            results.TryDequeue(out r);
            int cx = r;
            results.TryDequeue(out r);
            cx += r;
            Assert.Equal(TestCommandFactory.FALLBACK_VALUE + TestCommandFactory.EXECUTE_VALUE, cx);

            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
            // the rest should not be involved in this test
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);

            Console.WriteLine("**** DONE");

            Assert.Equal(2, ctx.GetRequestLog().GetAllExecutedCommands().Count());
        }


        /**
         * Tests that semaphores are counted separately for commands with unique keys
         */
        //[Fact]
        //public void testSemaphorePermitsInUse()
        //{
        //    TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();

        //    // this semaphore will be shared across multiple command instances
        //    var sharedSemaphore = new TryableSemaphoreActual(DynamicProperties.Factory.AsProperty(3));

        //    // used to wait until all commands have started
        //    CountDownLatch startLatch = new CountDownLatch((sharedSemaphore.NumberOfPermits.Get() * 2) + 1);

        //    // used to signal that all command can finish
        //    CountDownLatch sharedLatch = new CountDownLatch(1);

        //    // tracks failures to obtain semaphores
        //    AtomicInteger failureCount = new AtomicInteger();

        //    Runnable sharedSemaphoreRunnable = new ContextRunnable(Plugins.GetInstance().GetConcurrencyStrategy(), new Runnable()
        //    {
        //        //public void run() {
        //        //    try {
        //        //        new LatchedSemaphoreCommand(circuitBreaker, sharedSemaphore, startLatch, sharedLatch)await .ExecuteAsync();
        //        //    } catch (Exception e) {
        //        //        startLatch.countDown();
        //        //        
        //        //        failureCount.incrementAndGet();
        //        //    }
        //        //}
        //    });

        //    // creates group of threads each using command sharing a single semaphore
        //    // I create extra threads and commands so that I can verify that some of them fail to obtain a semaphore
        //    int sharedThreadCount = sharedSemaphore.NumberOfPermits.Get() * 2;
        //    var sharedSemaphoreTasks = new Task[sharedThreadCount];
        //    for (int i = 0; i < sharedThreadCount; i++)
        //    {
        //        sharedSemaphoreTasks[i] = new Thread(sharedSemaphoreRunnable);
        //    }

        //    // creates thread using isolated semaphore
        //    var isolatedSemaphore = new TryableSemaphoreActual(DynamicProperties.Factory.AsProperty(1));

        //    var isolatedLatch = new CountDownLatch(1);

        //    Thread isolatedThread = new Thread(new ContextRunnable(Plugins.GetInstance().GetConcurrencyStrategy(), new Runnable()
        //    {
        //        //public void run() {
        //        //    try {
        //        //        new LatchedSemaphoreCommand(circuitBreaker, isolatedSemaphore, startLatch, isolatedLatch)await .ExecuteAsync();
        //        //    } catch (Exception e) {
        //        //        startLatch.countDown();
        //        //        
        //        //        failureCount.incrementAndGet();
        //        //    }
        //        //}
        //    }));

        //    // verifies no permits in use before starting threads
        //    Assert.Equal(// "before threads start, shared semaphore should be unused",
        //        0, sharedSemaphore.NumberOfPermitsUsed);
        //    Assert.Equal(// "before threads start, isolated semaphore should be unused",
        //        0, isolatedSemaphore.NumberOfPermitsUsed);

        //    for (int i = 0; i < sharedThreadCount; i++)
        //    {
        //        sharedSemaphoreTasks[i].start();
        //    }
        //    isolatedThread.start();

        //    // waits until all commands have started
        //    try
        //    {
        //        startLatch.await(1000, TimeUnit.MILLISECONDS);
        //    }
        //    catch (InterruptedException e)
        //    {
        //        throw new RuntimeException(e);
        //    }

        //    // verifies that all semaphores are in use

        //    Assert.Equal( // "immediately after command start, all shared semaphores should be in-use"
        //            sharedSemaphore.NumberOfPermits.Get(), sharedSemaphore.NumberOfPermitsUsed);
        //    Assert.Equal( // "immediately after command start, isolated semaphore should be in-use"
        //            isolatedSemaphore.NumberOfPermits.Get(), isolatedSemaphore.NumberOfPermitsUsed);

        //    // signals commands to finish
        //    sharedLatch.countDown();
        //    isolatedLatch.countDown();

        //    try
        //    {
        //        for (int i = 0; i < sharedThreadCount; i++)
        //        {
        //            sharedSemaphoreTasks[i].join();
        //        }
        //        isolatedThread.join();
        //    }
        //    catch (Exception e)
        //    {

        //        throw new Exception("failed waiting on threads");
        //    }

        //    // verifies no permits in use after finishing threads
        //    Assert.Equal(// "after all threads have finished, no shared semaphores should be in-use",
        //        0, sharedSemaphore.NumberOfPermitsUsed);
        //    Assert.Equal(// "after all threads have finished, isolated semaphore not in-use",
        //        0, isolatedSemaphore.NumberOfPermitsUsed);

        //    // verifies that some executions failed
        //    Assert.Equal(// "expected some of shared semaphore commands to get rejected",
        //        sharedSemaphore.NumberOfPermits.Get(), failureCount.Get());

        //    Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);
        //}


        private TestServiceCommand getSharedCircuitBreakerCommand(IJellyfishContext ctx, ExecutionIsolationStrategy isolationStrategy, TestCircuitBreaker circuitBreaker, TestCommandFactory.FallbackResult  fallbackResult = TestCommandFactory.FallbackResult.SUCCESS, TestCommandFactory.ExecutionResult executionResult=TestCommandFactory.ExecutionResult.FAILURE, Action<CommandPropertiesBuilder> setter=null, int executionLatency=0, int fallbackLatency=0, ITryableSemaphore semaphore=null, [System.Runtime.CompilerServices.CallerMemberName] string commandName = null)
        {
            var cmd = TestCommandFactory.Get(ctx, output, isolationStrategy,
                        executionResult,
                        fallbackResult,
                        builder => { builder.WithExecutionTimeoutInMilliseconds(1000); if (setter!=null) setter(builder); },
                        circuitBreaker, 
                        circuitBreaker.Clock,
                        commandName:commandName);

            cmd.ExecutionSemaphore = semaphore;
            cmd.ExecutionLatency = executionLatency;
            cmd.FallbackLatency = fallbackLatency;

            var sb = new StringBuilder();
            sb.AppendFormat("Command {0}, timeout {1}", cmd.CommandName, cmd.Properties.ExecutionTimeoutEnabled.Get() ? cmd.Properties.ExecutionTimeoutInMilliseconds.Get().ToString() : "<none>");
            sb.AppendLine();
            sb.AppendFormat("exec max {0}", cmd.Properties.ExecutionIsolationSemaphoreMaxConcurrentRequests.Get().ToString());
            sb.AppendLine(); sb.AppendFormat("fallback max {0}", cmd.Properties.FallbackEnabled.Get() ? cmd.Properties.FallbackIsolationSemaphoreMaxConcurrentRequests.Get().ToString() : "<none>");
            sb.AppendLine();
            sb.AppendFormat("exec semaphore {0}", cmd.ExecutionSemaphore.ToString());
            sb.AppendLine();
            sb.AppendFormat("fallback semaphore {0}", cmd.FallBackSemaphore.ToString());
            sb.AppendLine();
            sb.AppendFormat("Circuit breaker {0}", circuitBreaker.ToString());
            sb.AppendLine();
            output.WriteLine(sb.ToString());
            return cmd;
        }
    }
}
