using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Jellyfish.Commands;
using Jellyfish.Commands.Metrics;
using System.Threading;

//namespace Jellyfish.Commands.Test
//{
//    public class CommandTest
//    {


        //        public void prepareForTest()
        //        {
        //            /* we must call this to simulate a new request lifecycle running and clearing caches */
        //            RequestContext.initializeContext();
        //        }

        //        public void cleanup()
        //        {
        //            // instead of storing the reference from initialize we'll just get the current state and shutdown
        //            if (RequestContext.GetContextForCurrentThread() != null)
        //            {
        //                // it could have been set NULL by the test
        //                RequestContext.GetContextForCurrentThread().shutdown();
        //            }

        //            // force properties to be clean as well
        //            ConfigurationManager.GetConfigInstance().clear();

        //            //CommandKey key = .GetCurrentThreadExecutingCommand();
        //            //if (key != null) {
        //            //    Console.WriteLine("WARNING: .GetCurrentThreadExecutingCommand() should be null but got: " + key + ". Can occur when calling queue() and never retrieving.");
        //            //}
        //        }
















       
       
      

        //        /**
        //         * Test that Owner can be passed in dynamically.
        //         */
        //        [Fact]
        //        public void testDynamicOwner()
        //        {
        //            try
        //            {
        //                TestCommand<bool> command = new DynamicOwnerTestCommand(InspectableBuilder.CommandGroupForUnitTest.OWNER_ONE);
        //                Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //                Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
        //                Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //                Assert.Equal(true, command.ExecuteAsync().Wait());
        //                Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //                Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
        //                Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //            }
        //            catch (Exception e)
        //            {

        //               new Exception("We received an exception.");
        //            }
        //        }

        //        /**
        //         * Test a successful command execution.
        //         */
        //        [Fact]
        //        public void testDynamicOwnerFails()
        //        {
        //            try
        //            {
        //                TestCommand<bool> command = new DynamicOwnerTestCommand(null);
        //                Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //                Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
        //                Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //                Assert.Equal(true, command.ExecuteAsync().Wait());
        //               new Exception("we should have thrown an exception as we need an owner");
        //            }
        //            catch (Exception e)
        //            {
        //                // success if we get here
        //            }
        //        }

        //        /**
        //         * Test that CommandKey can be passed in dynamically.
        //         */
        //        [Fact]
        //        public void testDynamicKey()
        //        {
        //            try
        //            {
        //                DynamicOwnerAndKeyTestCommand command1 = new DynamicOwnerAndKeyTestCommand(InspectableBuilder.CommandGroupForUnitTest.OWNER_ONE, InspectableBuilder.CommandKeyForUnitTest.KEY_ONE);
        //                Assert.Equal(true, command1.ExecuteAsync().Wait());
        //                DynamicOwnerAndKeyTestCommand command2 = new DynamicOwnerAndKeyTestCommand(InspectableBuilder.CommandGroupForUnitTest.OWNER_ONE, InspectableBuilder.CommandKeyForUnitTest.KEY_TWO);
        //                Assert.Equal(true, command2.ExecuteAsync().Wait());

        //                // 2 different circuit breakers should be created
        //                assertNotSame(command1.GetCircuitBreaker(), command2.GetCircuitBreaker());
        //            }
        //            catch (Exception e)
        //            {

        //               new Exception("We received an exception.");
        //            }
        //        }

        //        /**
        //         * Test Request scoped caching of commands so that a 2nd duplicate call doesn't execute but returns the previous Future
        //         */
        //        [Fact]
        //        public void testRequestCache1()
        //        {
        //            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
        //            SuccessfulCacheableCommand<String> command1 = new SuccessfulCacheableCommand<String>(circuitBreaker, true, "A");
        //            SuccessfulCacheableCommand<String> command2 = new SuccessfulCacheableCommand<String>(circuitBreaker, true, "A");

        //            Assert.True(command1.isCommandRunningInThread());

        //            Future<String> f1 = command1.queue();
        //            Future<String> f2 = command2.queue();

        //            try
        //            {
        //                Assert.Equal("A", f1.Get());
        //                Assert.Equal("A", f2.Get());
        //            }
        //            catch (Exception e)
        //            {
        //                throw new RuntimeException(e);
        //            }

        //            Assert.True(command1.executed);
        //            // the second one should not have executed as it should have received the cached value instead
        //            Assert.False(command2.executed);

        //            // the execution log for command1 should show a SUCCESS
        //            Assert.Equal(1, command1.GetExecutionEvents().size());
        //            Assert.True(command1.GetExecutionEvents().contains(EventType.SUCCESS));
        //            Assert.True(command1.ExecutionTimeInMilliseconds > -1);
        //            Assert.False(command1.isResponseFromCache());

        //            // the execution log for command2 should show it came from cache
        //            Assert.Equal(2, command2.GetExecutionEvents().size()); // it will include the SUCCESS + RESPONSE_FROM_CACHE
        //            Assert.True(command2.GetExecutionEvents().contains(EventType.SUCCESS));
        //            Assert.True(command2.GetExecutionEvents().contains(EventType.RESPONSE_FROM_CACHE));
        //            Assert.True(command2.ExecutionTimeInMilliseconds == -1);
        //            Assert.True(command2.isResponseFromCache());

        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

        //            Assert.Equal(0, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);
        //            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);

        //            Assert.Equal(2, RequestLog.GetCurrentRequest().GetAllExecutedCommands().size());
        //        }

        //        /**
        //         * Test Request scoped caching doesn't prevent different ones from executing
        //         */
        //        [Fact]
        //        public void testRequestCache2()
        //        {
        //            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
        //            SuccessfulCacheableCommand<String> command1 = new SuccessfulCacheableCommand<String>(circuitBreaker, true, "A");
        //            SuccessfulCacheableCommand<String> command2 = new SuccessfulCacheableCommand<String>(circuitBreaker, true, "B");

        //            Assert.True(command1.isCommandRunningInThread());

        //            Future<String> f1 = command1.queue();
        //            Future<String> f2 = command2.queue();

        //            try
        //            {
        //                Assert.Equal("A", f1.Get());
        //                Assert.Equal("B", f2.Get());
        //            }
        //            catch (Exception e)
        //            {
        //                throw new RuntimeException(e);
        //            }

        //            Assert.True(command1.executed);
        //            // both should execute as they are different
        //            Assert.True(command2.executed);

        //            // the execution log for command1 should show a SUCCESS
        //            Assert.Equal(1, command1.GetExecutionEvents().size());
        //            Assert.True(command1.GetExecutionEvents().contains(EventType.SUCCESS));

        //            // the execution log for command2 should show a SUCCESS
        //            Assert.Equal(1, command2.GetExecutionEvents().size());
        //            Assert.True(command2.GetExecutionEvents().contains(EventType.SUCCESS));
        //            Assert.True(command2.ExecutionTimeInMilliseconds > -1);
        //            Assert.False(command2.isResponseFromCache());

        //            Assert.Equal(2, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

        //            Assert.Equal(0, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);
        //            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);

        //            Assert.Equal(2, RequestLog.GetCurrentRequest().GetAllExecutedCommands().size());
        //        }

        //        /**
        //         * Test Request scoped caching with a mixture of commands
        //         */
        //        [Fact]
        //        public void testRequestCache3()
        //        {
        //            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
        //            SuccessfulCacheableCommand<String> command1 = new SuccessfulCacheableCommand<String>(circuitBreaker, true, "A");
        //            SuccessfulCacheableCommand<String> command2 = new SuccessfulCacheableCommand<String>(circuitBreaker, true, "B");
        //            SuccessfulCacheableCommand<String> command3 = new SuccessfulCacheableCommand<String>(circuitBreaker, true, "A");

        //            Assert.True(command1.isCommandRunningInThread());

        //            Future<String> f1 = command1.queue();
        //            Future<String> f2 = command2.queue();
        //            Future<String> f3 = command3.queue();

        //            try
        //            {
        //                Assert.Equal("A", f1.Get());
        //                Assert.Equal("B", f2.Get());
        //                Assert.Equal("A", f3.Get());
        //            }
        //            catch (Exception e)
        //            {
        //                throw new RuntimeException(e);
        //            }

        //            Assert.True(command1.executed);
        //            // both should execute as they are different
        //            Assert.True(command2.executed);
        //            // but the 3rd should come from cache
        //            Assert.False(command3.executed);

        //            // the execution log for command1 should show a SUCCESS
        //            Assert.Equal(1, command1.GetExecutionEvents().size());
        //            Assert.True(command1.GetExecutionEvents().contains(EventType.SUCCESS));

        //            // the execution log for command2 should show a SUCCESS
        //            Assert.Equal(1, command2.GetExecutionEvents().size());
        //            Assert.True(command2.GetExecutionEvents().contains(EventType.SUCCESS));

        //            // the execution log for command3 should show it came from cache
        //            Assert.Equal(2, command3.GetExecutionEvents().size()); // it will include the SUCCESS + RESPONSE_FROM_CACHE
        //            Assert.True(command3.GetExecutionEvents().contains(EventType.RESPONSE_FROM_CACHE));
        //            Assert.True(command3.ExecutionTimeInMilliseconds == -1);
        //            Assert.True(command3.isResponseFromCache());

        //            Assert.Equal(2, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

        //            Assert.Equal(0, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);
        //            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);

        //            Assert.Equal(3, RequestLog.GetCurrentRequest().GetAllExecutedCommands().size());
        //        }

        //        /**
        //         * Test Request scoped caching of commands so that a 2nd duplicate call doesn't execute but returns the previous Future
        //         */
        //        [Fact]
        //        public void testRequestCacheWithSlowExecution()
        //        {
        //            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
        //            SlowCacheableCommand command1 = new SlowCacheableCommand(circuitBreaker, "A", 200);
        //            SlowCacheableCommand command2 = new SlowCacheableCommand(circuitBreaker, "A", 100);
        //            SlowCacheableCommand command3 = new SlowCacheableCommand(circuitBreaker, "A", 100);
        //            SlowCacheableCommand command4 = new SlowCacheableCommand(circuitBreaker, "A", 100);

        //            Future<String> f1 = command1.queue();
        //            Future<String> f2 = command2.queue();
        //            Future<String> f3 = command3.queue();
        //            Future<String> f4 = command4.queue();

        //            try
        //            {
        //                Assert.Equal("A", f2.Get());
        //                Assert.Equal("A", f3.Get());
        //                Assert.Equal("A", f4.Get());

        //                Assert.Equal("A", f1.Get());
        //            }
        //            catch (Exception e)
        //            {
        //                throw new RuntimeException(e);
        //            }

        //            Assert.True(command1.executed);
        //            // the second one should not have executed as it should have received the cached value instead
        //            Assert.False(command2.executed);
        //            Assert.False(command3.executed);
        //            Assert.False(command4.executed);

        //            // the execution log for command1 should show a SUCCESS
        //            Assert.Equal(1, command1.GetExecutionEvents().size());
        //            Assert.True(command1.GetExecutionEvents().contains(EventType.SUCCESS));
        //            Assert.True(command1.ExecutionTimeInMilliseconds > -1);
        //            Assert.False(command1.isResponseFromCache());

        //            // the execution log for command2 should show it came from cache
        //            Assert.Equal(2, command2.GetExecutionEvents().size()); // it will include the SUCCESS + RESPONSE_FROM_CACHE
        //            Assert.True(command2.GetExecutionEvents().contains(EventType.SUCCESS));
        //            Assert.True(command2.GetExecutionEvents().contains(EventType.RESPONSE_FROM_CACHE));
        //            Assert.True(command2.ExecutionTimeInMilliseconds == -1);
        //            Assert.True(command2.isResponseFromCache());

        //            Assert.True(command3.isResponseFromCache());
        //            Assert.True(command3.ExecutionTimeInMilliseconds == -1);
        //            Assert.True(command4.isResponseFromCache());
        //            Assert.True(command4.ExecutionTimeInMilliseconds == -1);

        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
        //            Assert.Equal(3, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

        //            Assert.Equal(0, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);
        //            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);

        //            Assert.Equal(4, RequestLog.GetCurrentRequest().GetAllExecutedCommands().size());

        //            Console.WriteLine("RequestLog: " + RequestLog.GetCurrentRequest().GetExecutedCommandsAsString());
        //        }

        //        /**
        //         * Test Request scoped caching with a mixture of commands
        //         */
        //        [Fact]
        //        public void testNoRequestCache3()
        //        {
        //            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
        //            SuccessfulCacheableCommand<String> command1 = new SuccessfulCacheableCommand<String>(circuitBreaker, false, "A");
        //            SuccessfulCacheableCommand<String> command2 = new SuccessfulCacheableCommand<String>(circuitBreaker, false, "B");
        //            SuccessfulCacheableCommand<String> command3 = new SuccessfulCacheableCommand<String>(circuitBreaker, false, "A");

        //            Assert.True(command1.isCommandRunningInThread());

        //            Future<String> f1 = command1.queue();
        //            Future<String> f2 = command2.queue();
        //            Future<String> f3 = command3.queue();

        //            try
        //            {
        //                Assert.Equal("A", f1.Get());
        //                Assert.Equal("B", f2.Get());
        //                Assert.Equal("A", f3.Get());
        //            }
        //            catch (Exception e)
        //            {
        //                throw new RuntimeException(e);
        //            }

        //            Assert.True(command1.executed);
        //            // both should execute as they are different
        //            Assert.True(command2.executed);
        //            // this should also execute since we disabled the cache
        //            Assert.True(command3.executed);

        //            // the execution log for command1 should show a SUCCESS
        //            Assert.Equal(1, command1.GetExecutionEvents().size());
        //            Assert.True(command1.GetExecutionEvents().contains(EventType.SUCCESS));

        //            // the execution log for command2 should show a SUCCESS
        //            Assert.Equal(1, command2.GetExecutionEvents().size());
        //            Assert.True(command2.GetExecutionEvents().contains(EventType.SUCCESS));

        //            // the execution log for command3 should show a SUCCESS
        //            Assert.Equal(1, command3.GetExecutionEvents().size());
        //            Assert.True(command3.GetExecutionEvents().contains(EventType.SUCCESS));

        //            Assert.Equal(3, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

        //            Assert.Equal(0, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);
        //            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);

        //            Assert.Equal(3, RequestLog.GetCurrentRequest().GetAllExecutedCommands().size());
        //        }

        //        /**
        //         * Test Request scoped caching with a mixture of commands
        //         */
        //        [Fact]
        //        public void testRequestCacheViaQueueSemaphore1()
        //        {
        //            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
        //            SuccessfulCacheableCommandViaSemaphore command1 = new SuccessfulCacheableCommandViaSemaphore(circuitBreaker, true, "A");
        //            SuccessfulCacheableCommandViaSemaphore command2 = new SuccessfulCacheableCommandViaSemaphore(circuitBreaker, true, "B");
        //            SuccessfulCacheableCommandViaSemaphore command3 = new SuccessfulCacheableCommandViaSemaphore(circuitBreaker, true, "A");

        //            Assert.False(command1.isCommandRunningInThread());

        //            Future<String> f1 = command1.queue();
        //            Future<String> f2 = command2.queue();
        //            Future<String> f3 = command3.queue();

        //            try
        //            {
        //                Assert.Equal("A", f1.Get());
        //                Assert.Equal("B", f2.Get());
        //                Assert.Equal("A", f3.Get());
        //            }
        //            catch (Exception e)
        //            {
        //                throw new RuntimeException(e);
        //            }

        //            Assert.True(command1.executed);
        //            // both should execute as they are different
        //            Assert.True(command2.executed);
        //            // but the 3rd should come from cache
        //            Assert.False(command3.executed);

        //            // the execution log for command1 should show a SUCCESS
        //            Assert.Equal(1, command1.GetExecutionEvents().size());
        //            Assert.True(command1.GetExecutionEvents().contains(EventType.SUCCESS));

        //            // the execution log for command2 should show a SUCCESS
        //            Assert.Equal(1, command2.GetExecutionEvents().size());
        //            Assert.True(command2.GetExecutionEvents().contains(EventType.SUCCESS));

        //            // the execution log for command3 should show it comes from cache
        //            Assert.Equal(2, command3.GetExecutionEvents().size()); // it will include the SUCCESS + RESPONSE_FROM_CACHE
        //            Assert.True(command3.GetExecutionEvents().contains(EventType.SUCCESS));
        //            Assert.True(command3.GetExecutionEvents().contains(EventType.RESPONSE_FROM_CACHE));

        //            Assert.True(command3.isResponseFromCache());
        //            Assert.True(command3.ExecutionTimeInMilliseconds == -1);

        //            Assert.Equal(2, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

        //            Assert.Equal(0, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);
        //            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);

        //            Assert.Equal(3, RequestLog.GetCurrentRequest().GetAllExecutedCommands().size());
        //        }

        //        /**
        //         * Test Request scoped caching with a mixture of commands
        //         */
        //        [Fact]
        //        public void testNoRequestCacheViaQueueSemaphore1()
        //        {
        //            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
        //            SuccessfulCacheableCommandViaSemaphore command1 = new SuccessfulCacheableCommandViaSemaphore(circuitBreaker, false, "A");
        //            SuccessfulCacheableCommandViaSemaphore command2 = new SuccessfulCacheableCommandViaSemaphore(circuitBreaker, false, "B");
        //            SuccessfulCacheableCommandViaSemaphore command3 = new SuccessfulCacheableCommandViaSemaphore(circuitBreaker, false, "A");

        //            Assert.False(command1.isCommandRunningInThread());

        //            Future<String> f1 = command1.queue();
        //            Future<String> f2 = command2.queue();
        //            Future<String> f3 = command3.queue();

        //            try
        //            {
        //                Assert.Equal("A", f1.Get());
        //                Assert.Equal("B", f2.Get());
        //                Assert.Equal("A", f3.Get());
        //            }
        //            catch (Exception e)
        //            {
        //                throw new RuntimeException(e);
        //            }

        //            Assert.True(command1.executed);
        //            // both should execute as they are different
        //            Assert.True(command2.executed);
        //            // this should also execute because caching is disabled
        //            Assert.True(command3.executed);

        //            // the execution log for command1 should show a SUCCESS
        //            Assert.Equal(1, command1.GetExecutionEvents().size());
        //            Assert.True(command1.GetExecutionEvents().contains(EventType.SUCCESS));

        //            // the execution log for command2 should show a SUCCESS
        //            Assert.Equal(1, command2.GetExecutionEvents().size());
        //            Assert.True(command2.GetExecutionEvents().contains(EventType.SUCCESS));

        //            // the execution log for command3 should show a SUCCESS
        //            Assert.Equal(1, command3.GetExecutionEvents().size());
        //            Assert.True(command3.GetExecutionEvents().contains(EventType.SUCCESS));

        //            Assert.Equal(3, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

        //            Assert.Equal(0, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);
        //            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);

        //            Assert.Equal(3, RequestLog.GetCurrentRequest().GetAllExecutedCommands().size());
        //        }

        //        /**
        //         * Test Request scoped caching with a mixture of commands
        //         */
        //        [Fact]
        //        public void testRequestCacheViaExecuteSemaphore1()
        //        {
        //            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
        //            SuccessfulCacheableCommandViaSemaphore command1 = new SuccessfulCacheableCommandViaSemaphore(circuitBreaker, true, "A");
        //            SuccessfulCacheableCommandViaSemaphore command2 = new SuccessfulCacheableCommandViaSemaphore(circuitBreaker, true, "B");
        //            SuccessfulCacheableCommandViaSemaphore command3 = new SuccessfulCacheableCommandViaSemaphore(circuitBreaker, true, "A");

        //            Assert.False(command1.isCommandRunningInThread());

        //            String f1 = command1.ExecuteAsync().Wait();
        //            String f2 = command2.ExecuteAsync().Wait();
        //            String f3 = command3.ExecuteAsync().Wait();

        //            Assert.Equal("A", f1);
        //            Assert.Equal("B", f2);
        //            Assert.Equal("A", f3);

        //            Assert.True(command1.executed);
        //            // both should execute as they are different
        //            Assert.True(command2.executed);
        //            // but the 3rd should come from cache
        //            Assert.False(command3.executed);

        //            // the execution log for command1 should show a SUCCESS
        //            Assert.Equal(1, command1.GetExecutionEvents().size());
        //            Assert.True(command1.GetExecutionEvents().contains(EventType.SUCCESS));

        //            // the execution log for command2 should show a SUCCESS
        //            Assert.Equal(1, command2.GetExecutionEvents().size());
        //            Assert.True(command2.GetExecutionEvents().contains(EventType.SUCCESS));

        //            // the execution log for command3 should show it comes from cache
        //            Assert.Equal(2, command3.GetExecutionEvents().size()); // it will include the SUCCESS + RESPONSE_FROM_CACHE
        //            Assert.True(command3.GetExecutionEvents().contains(EventType.RESPONSE_FROM_CACHE));

        //            Assert.Equal(2, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

        //            Assert.Equal(0, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);
        //            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);

        //            Assert.Equal(3, RequestLog.GetCurrentRequest().GetAllExecutedCommands().size());
        //        }

        //        /**
        //         * Test Request scoped caching with a mixture of commands
        //         */
        //        [Fact]
        //        public void testNoRequestCacheViaExecuteSemaphore1()
        //        {
        //            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
        //            SuccessfulCacheableCommandViaSemaphore command1 = new SuccessfulCacheableCommandViaSemaphore(circuitBreaker, false, "A");
        //            SuccessfulCacheableCommandViaSemaphore command2 = new SuccessfulCacheableCommandViaSemaphore(circuitBreaker, false, "B");
        //            SuccessfulCacheableCommandViaSemaphore command3 = new SuccessfulCacheableCommandViaSemaphore(circuitBreaker, false, "A");

        //            Assert.False(command1.isCommandRunningInThread());

        //            String f1 = command1.ExecuteAsync().Wait();
        //            String f2 = command2.ExecuteAsync().Wait();
        //            String f3 = command3.ExecuteAsync().Wait();

        //            Assert.Equal("A", f1);
        //            Assert.Equal("B", f2);
        //            Assert.Equal("A", f3);

        //            Assert.True(command1.executed);
        //            // both should execute as they are different
        //            Assert.True(command2.executed);
        //            // this should also execute because caching is disabled
        //            Assert.True(command3.executed);

        //            // the execution log for command1 should show a SUCCESS
        //            Assert.Equal(1, command1.GetExecutionEvents().size());
        //            Assert.True(command1.GetExecutionEvents().contains(EventType.SUCCESS));

        //            // the execution log for command2 should show a SUCCESS
        //            Assert.Equal(1, command2.GetExecutionEvents().size());
        //            Assert.True(command2.GetExecutionEvents().contains(EventType.SUCCESS));

        //            // the execution log for command3 should show a SUCCESS
        //            Assert.Equal(1, command3.GetExecutionEvents().size());
        //            Assert.True(command3.GetExecutionEvents().contains(EventType.SUCCESS));

        //            Assert.Equal(3, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

        //            Assert.Equal(0, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);
        //            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);

        //            Assert.Equal(3, RequestLog.GetCurrentRequest().GetAllExecutedCommands().size());
        //        }

        //        [Fact]
        //        public void testNoRequestCacheOnTimeoutThrowsException()
        //        {
        //            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
        //            NoRequestCacheTimeoutWithoutFallback r1 = new NoRequestCacheTimeoutWithoutFallback(circuitBreaker);
        //            try
        //            {
        //                Console.WriteLine("r1 value: " + r1.ExecuteAsync().Wait());
        //                // we should have thrown an exception
        //               new Exception("expected a timeout");
        //            }
        //            catch (RuntimeException e)
        //            {
        //                Assert.True(r1.isResponseTimedOut());
        //                // what we want
        //            }

        //            NoRequestCacheTimeoutWithoutFallback r2 = new NoRequestCacheTimeoutWithoutFallback(circuitBreaker);
        //            try
        //            {
        //                r2.ExecuteAsync().Wait();
        //                // we should have thrown an exception
        //               new Exception("expected a timeout");
        //            }
        //            catch (RuntimeException e)
        //            {
        //                Assert.True(r2.isResponseTimedOut());
        //                // what we want
        //            }

        //            NoRequestCacheTimeoutWithoutFallback r3 = new NoRequestCacheTimeoutWithoutFallback(circuitBreaker);
        //            Future<bool> f3 = r3.queue();
        //            try
        //            {
        //                f3.Get();
        //                // we should have thrown an exception
        //               new Exception("expected a timeout");
        //            }
        //            catch (ExecutionException e)
        //            {

        //                Assert.True(r3.isResponseTimedOut());
        //                // what we want
        //            }

        //            Thread.sleep(500); // timeout on command is set to 200ms

        //            NoRequestCacheTimeoutWithoutFallback r4 = new NoRequestCacheTimeoutWithoutFallback(circuitBreaker);
        //            try
        //            {
        //                r4.ExecuteAsync().Wait();
        //                // we should have thrown an exception
        //               new Exception("expected a timeout");
        //            }
        //            catch (RuntimeException e)
        //            {
        //                Assert.True(r4.isResponseTimedOut());
        //                Assert.False(r4.isResponseFromFallback());
        //                // what we want
        //            }

        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //            Assert.Equal(4, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
        //            Assert.Equal(4, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

        //            Assert.Equal(100, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);
        //            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);

        //            Assert.Equal(4, RequestLog.GetCurrentRequest().GetAllExecutedCommands().size());
        //        }

        //        [Fact]
        //        public void testRequestCacheOnTimeoutCausesNullPointerException()
        //        {
        //            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
        //            // Expect it to time out - all results should be false
        //            Assert.False(new RequestCacheNullPointerExceptionCase(circuitBreaker).ExecuteAsync().Wait());
        //            Assert.False(new RequestCacheNullPointerExceptionCase(circuitBreaker).ExecuteAsync().Wait()); // return from cache #1
        //            Assert.False(new RequestCacheNullPointerExceptionCase(circuitBreaker).ExecuteAsync().Wait()); // return from cache #2
        //            Thread.sleep(500); // timeout on command is set to 200ms
        //            bool value = new RequestCacheNullPointerExceptionCase(circuitBreaker).ExecuteAsync().Wait(); // return from cache #3
        //            Assert.False(value);
        //            RequestCacheNullPointerExceptionCase c = new RequestCacheNullPointerExceptionCase(circuitBreaker);
        //            Future<bool> f = c.queue(); // return from cache #4
        //                                        // the bug is that we're getting a null Future back, rather than a Future that returns false
        //            Assert.NotNull(f);
        //            Assert.False(f.Get());

        //            Assert.True(c.isResponseFromFallback());
        //            Assert.True(c.isResponseTimedOut());
        //            Assert.False(c.IsFailedExecution);
        //            Assert.False(c.isResponseShortCircuited());

        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
        //            Assert.Equal(4, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

        //            Assert.Equal(100, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);
        //            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);

        //            Assert.Equal(5, RequestLog.GetCurrentRequest().GetAllExecutedCommands().size());

        //            var executeCommands = RequestLog.GetCurrentRequest().GetAllExecutedCommands().toArray(new InvokableInfo[] { });

        //            Console.WriteLine(":executeCommands[0].GetExecutionEvents()" + executeCommands[0].GetExecutionEvents());
        //            Assert.Equal(2, executeCommands[0].GetExecutionEvents().size());
        //            Assert.True(executeCommands[0].GetExecutionEvents().contains(EventType.FALLBACK_SUCCESS));
        //            Assert.True(executeCommands[0].GetExecutionEvents().contains(EventType.TIMEOUT));
        //            Assert.True(executeCommands[0].ExecutionTimeInMilliseconds > -1);
        //            Assert.True(executeCommands[0].isResponseTimedOut());
        //            Assert.True(executeCommands[0].isResponseFromFallback());
        //            Assert.False(executeCommands[0].isResponseFromCache());

        //            Assert.Equal(3, executeCommands[1].GetExecutionEvents().size()); // it will include FALLBACK_SUCCESS/TIMEOUT + RESPONSE_FROM_CACHE
        //            Assert.True(executeCommands[1].GetExecutionEvents().contains(EventType.RESPONSE_FROM_CACHE));
        //            Assert.True(executeCommands[1].ExecutionTimeInMilliseconds == -1);
        //            Assert.True(executeCommands[1].isResponseFromCache());
        //            Assert.True(executeCommands[1].isResponseTimedOut());
        //            Assert.True(executeCommands[1].isResponseFromFallback());
        //        }

        //        [Fact]
        //        public void testRequestCacheOnTimeoutThrowsException()
        //        {
        //            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
        //            RequestCacheTimeoutWithoutFallback r1 = new RequestCacheTimeoutWithoutFallback(circuitBreaker);
        //            try
        //            {
        //                Console.WriteLine("r1 value: " + r1.ExecuteAsync().Wait());
        //                // we should have thrown an exception
        //               new Exception("expected a timeout");
        //            }
        //            catch (RuntimeException e)
        //            {
        //                Assert.True(r1.isResponseTimedOut());
        //                // what we want
        //            }

        //            RequestCacheTimeoutWithoutFallback r2 = new RequestCacheTimeoutWithoutFallback(circuitBreaker);
        //            try
        //            {
        //                r2.ExecuteAsync().Wait();
        //                // we should have thrown an exception
        //               new Exception("expected a timeout");
        //            }
        //            catch (RuntimeException e)
        //            {
        //                Assert.True(r2.isResponseTimedOut());
        //                // what we want
        //            }

        //            RequestCacheTimeoutWithoutFallback r3 = new RequestCacheTimeoutWithoutFallback(circuitBreaker);
        //            Future<bool> f3 = r3.queue();
        //            try
        //            {
        //                f3.Get();
        //                // we should have thrown an exception
        //               new Exception("expected a timeout");
        //            }
        //            catch (ExecutionException e)
        //            {

        //                Assert.True(r3.isResponseTimedOut());
        //                // what we want
        //            }

        //            Thread.sleep(500); // timeout on command is set to 200ms

        //            RequestCacheTimeoutWithoutFallback r4 = new RequestCacheTimeoutWithoutFallback(circuitBreaker);
        //            try
        //            {
        //                r4.ExecuteAsync().Wait();
        //                // we should have thrown an exception
        //               new Exception("expected a timeout");
        //            }
        //            catch (RuntimeException e)
        //            {
        //                Assert.True(r4.isResponseTimedOut());
        //                Assert.False(r4.isResponseFromFallback());
        //                // what we want
        //            }

        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
        //            Assert.Equal(3, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

        //            Assert.Equal(100, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);
        //            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);

        //            Assert.Equal(4, RequestLog.GetCurrentRequest().GetAllExecutedCommands().size());
        //        }

        //        [Fact]
        //        public void testRequestCacheOnThreadRejectionThrowsException()
        //        {
        //            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
        //            CountDownLatch completionLatch = new CountDownLatch(1);
        //            RequestCacheThreadRejectionWithoutFallback r1 = new RequestCacheThreadRejectionWithoutFallback(circuitBreaker, completionLatch);
        //            try
        //            {
        //                Console.WriteLine("r1: " + r1.ExecuteAsync().Wait());
        //                // we should have thrown an exception
        //               new Exception("expected a rejection");
        //            }
        //            catch (RuntimeException e)
        //            {
        //                Assert.True(r1.isResponseRejected());
        //                // what we want
        //            }

        //            RequestCacheThreadRejectionWithoutFallback r2 = new RequestCacheThreadRejectionWithoutFallback(circuitBreaker, completionLatch);
        //            try
        //            {
        //                Console.WriteLine("r2: " + r2.ExecuteAsync().Wait());
        //                // we should have thrown an exception
        //               new Exception("expected a rejection");
        //            }
        //            catch (RuntimeException e)
        //            {
        //                //                
        //                Assert.True(r2.isResponseRejected());
        //                // what we want
        //            }

        //            RequestCacheThreadRejectionWithoutFallback r3 = new RequestCacheThreadRejectionWithoutFallback(circuitBreaker, completionLatch);
        //            try
        //            {
        //                Console.WriteLine("f3: " + r3.queue().Get());
        //                // we should have thrown an exception
        //               new Exception("expected a rejection");
        //            }
        //            catch (RuntimeException e)
        //            {
        //                //                
        //                Assert.True(r3.isResponseRejected());
        //                // what we want
        //            }

        //            // let the command finish (only 1 should actually be blocked on this due to the response cache)
        //            completionLatch.countDown();

        //            // then another after the command has completed
        //            RequestCacheThreadRejectionWithoutFallback r4 = new RequestCacheThreadRejectionWithoutFallback(circuitBreaker, completionLatch);
        //            try
        //            {
        //                Console.WriteLine("r4: " + r4.ExecuteAsync().Wait());
        //                // we should have thrown an exception
        //               new Exception("expected a rejection");
        //            }
        //            catch (RuntimeException e)
        //            {
        //                //                
        //                Assert.True(r4.isResponseRejected());
        //                Assert.False(r4.isResponseFromFallback());
        //                // what we want
        //            }

        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
        //            Assert.Equal(3, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

        //            Assert.Equal(100, circuitBreaker.Metrics.GetHealthCounts().ErrorPercentage);
        //            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);

        //            Assert.Equal(4, RequestLog.GetCurrentRequest().GetAllExecutedCommands().size());
        //        }

        //        /**
        //         * Test that we can do basic execution without a RequestVariable being initialized.
        //         */
        //        [Fact]
        //        public void testBasicExecutionWorksWithoutRequestVariable()
        //        {
        //            try
        //            {
        //                /* force the RequestVariable to not be initialized */
        //                RequestContext.setContextOnCurrentThread(null);

        //                TestCommand<bool> command = new SuccessfulTestCommand();
        //                Assert.Equal(true, command.ExecuteAsync().Wait());

        //                TestCommand<bool> command2 = new SuccessfulTestCommand();
        //                Assert.Equal(true, command2.queue().Get());

        //                // we should be able to execute without a RequestVariable if ...
        //                // 1) We don't have a cacheKey
        //                // 2) We don't ask for the RequestLog
        //                // 3) We don't do collapsing

        //            }
        //            catch (Exception e)
        //            {

        //               new Exception("We received an exception => " + e.GetMessage());
        //            }
        //        }

        //        /**
        //         * Test that if we try and execute a command with a cacheKey without initializing RequestVariable that it gives an error.
        //         */
        //        [Fact]
        //        public void testCacheKeyExecutionRequiresRequestVariable()
        //        {
        //            try
        //            {
        //                /* force the RequestVariable to not be initialized */
        //                RequestContext.setContextOnCurrentThread(null);

        //                TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();

        //                SuccessfulCacheableCommand command = new SuccessfulCacheableCommand<String>(circuitBreaker, true, "one");
        //                Assert.Equal("one", command.ExecuteAsync().Wait());

        //                SuccessfulCacheableCommand command2 = new SuccessfulCacheableCommand<String>(circuitBreaker, true, "two");
        //                Assert.Equal("two", command2.queue().Get());

        //               new Exception("We expect an exception because cacheKey requires RequestVariable.");

        //            }
        //            catch (Exception e)
        //            {

        //            }
        //        }

        //        /**
        //         * Test that a BadRequestException can be thrown and not count towards errors and bypasses fallback.
        //         */
        //        [Fact]
        //        public void testBadRequestExceptionViaExecuteInThread()
        //        {
        //            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
        //            try
        //            {
        //                new BadRequestCommand(circuitBreaker, ExecutionIsolationStrategy.Thread).ExecuteAsync().Wait();
        //                //  new Exception("we expect to receive a " + BadRequestException.class.GetSimpleName());
        //            }
        //            catch (BadRequestException e)
        //            {
        //                // success

        //            }
        //            catch (Exception e)
        //            {

        //                //  new Exception("We expect a " + BadRequestException.class.GetSimpleName() + " but got a " + e.GetClass().GetSimpleName());
        //            }

        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));

        //            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);
        //        }

        //        /**
        //         * Test that a BadRequestException can be thrown and not count towards errors and bypasses fallback.
        //         */
        //        [Fact]
        //        public void testBadRequestExceptionViaQueueInThread()
        //        {
        //            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
        //            try
        //            {
        //                new BadRequestCommand(circuitBreaker, ExecutionIsolationStrategy.Thread).queue().Get();
        //                // new Exception("we expect to receive a " + BadRequestException.class.GetSimpleName());
        //            }
        //            catch (ExecutionException e)
        //            {

        //                if (e.InnerException is BadRequestException)
        //                {
        //                    // success    
        //                }
        //                else
        //                {
        //                    //   new Exception("We expect a " + BadRequestException.class.GetSimpleName() + " but got a " + e.GetClass().GetSimpleName());
        //                }
        //            }
        //            catch (Exception e)
        //            {

        //               new Exception();
        //            }

        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));

        //            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);
        //        }

        //        /**
        //         * Test that BadRequestException behavior works the same on a cached response.
        //         */
        //        [Fact]
        //        public void testBadRequestExceptionViaQueueInThreadOnResponseFromCache()
        //        {
        //            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();

        //            // execute once to cache the value
        //            try
        //            {
        //                new BadRequestCommand(circuitBreaker, ExecutionIsolationStrategy.Thread).ExecuteAsync().Wait();
        //            }
        //            catch (Throwable e)
        //            {
        //                // ignore
        //            }

        //            try
        //            {
        //                new BadRequestCommand(circuitBreaker, ExecutionIsolationStrategy.Thread).queue().Get();
        //                //  new Exception("we expect to receive a " + BadRequestException.class.GetSimpleName());
        //            }
        //            catch (ExecutionException e)
        //            {

        //                if (e.InnerException is BadRequestException)
        //                {
        //                    // success    
        //                }
        //                else
        //                {
        //                    // new Exception("We expect a " + BadRequestException.class.GetSimpleName() + " but got a " + e.GetClass().GetSimpleName());
        //                }
        //            }
        //            catch (Exception e)
        //            {

        //               new Exception();
        //            }

        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));

        //            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);
        //        }

        //        /**
        //         * Test that a BadRequestException can be thrown and not count towards errors and bypasses fallback.
        //         */
        //        [Fact]
        //        public void testBadRequestExceptionViaExecuteInSemaphore()
        //        {
        //            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
        //            try
        //            {
        //                new BadRequestCommand(circuitBreaker, ExecutionIsolationStrategy.SEMAPHORE).ExecuteAsync().Wait();
        //                //new Exception("we expect to receive a " + BadRequestException.class.GetSimpleName());
        //            }
        //            catch (BadRequestException e)
        //            {
        //                // success

        //            }
        //            catch (Exception e)
        //            {

        //                // new Exception("We expect a " + BadRequestException.class.GetSimpleName() + " but got a " + e.GetClass().GetSimpleName());
        //            }

        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));

        //            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);
        //        }

        //        /**
        //         * Test that a BadRequestException can be thrown and not count towards errors and bypasses fallback.
        //         */
        //        [Fact]
        //        public void testBadRequestExceptionViaQueueInSemaphore()
        //        {
        //            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
        //            try
        //            {
        //                new BadRequestCommand(circuitBreaker, ExecutionIsolationStrategy.SEMAPHORE).queue().Get();
        //                //  new Exception("we expect to receive a " + BadRequestException.class.GetSimpleName());
        //            }
        //            catch (ExecutionException e)
        //            {

        //                if (e.InnerException is BadRequestException)
        //                {
        //                    // success    
        //                }
        //                else
        //                {
        //                    //   new Exception("We expect a " + BadRequestException.class.GetSimpleName() + " but got a " + e.GetClass().GetSimpleName());
        //                }
        //            }
        //            catch (Exception e)
        //            {

        //               new Exception();
        //            }

        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));

        //            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);
        //        }

        //        /**
        //         * Test a checked Exception being thrown
        //         */
        //        [Fact]
        //        public void testCheckedExceptionViaExecute()
        //        {
        //            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
        //            CommandWithCheckedException command = new CommandWithCheckedException(circuitBreaker);
        //            try
        //            {
        //                command.ExecuteAsync().Wait();
        //                // new Exception("we expect to receive a " + Exception.class.GetSimpleName());
        //            }
        //            catch (Exception e)
        //            {
        //                Assert.Equal("simulated checked exception message", e.InnerException.GetMessage());
        //            }

        //            Assert.Equal("simulated checked exception message", command.GetFailedExecutionException().GetMessage());

        //            Assert.True(command.ExecutionTimeInMilliseconds > -1);
        //            Assert.True(command.IsFailedExecution);

        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));

        //            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);
        //        }

        //        /**
        //         * Test a java.lang.Error being thrown
        //         * 
        //         * @throws InterruptedException
        //         */
        //        [Fact]
        //        public void testCheckedExceptionViaObserve()
        //        {
        //            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
        //            CommandWithCheckedException command = new CommandWithCheckedException(circuitBreaker);
        //            AtomicReference<Throwable> t = new AtomicReference<Throwable>();
        //            CountDownLatch latch = new CountDownLatch(1);
        //            try
        //            {
        //                //command.observe().subscribe(new Observer<bool>() {


        //                //    public void onCompleted() {
        //                //        latch.countDown();
        //                //    }


        //                //    public void onError(Throwable e) {
        //                //        t.set(e);
        //                //        latch.countDown();
        //                //    }


        //                //    public void onNext(bool args) {

        //                //    }

        //                //});
        //            }
        //            catch (Exception e)
        //            {

        //               new Exception("we should not get anything thrown, it should be emitted via the Observer#onError method");
        //            }

        //            latch.await(1, TimeUnit.SECONDS);
        //            Assert.NotNull(t.Get());
        //            t.Get().printStackTrace();

        //            Assert.True(t.Get() is RuntimeException);
        //            Assert.Equal("simulated checked exception message", t.Get().InnerException.GetMessage());
        //            Assert.Equal("simulated checked exception message", command.GetFailedExecutionException().GetMessage());

        //            Assert.True(command.ExecutionTimeInMilliseconds > -1);
        //            Assert.True(command.IsFailedExecution);

        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));

        //            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);
        //        }








        //        [Fact]
        //        public void testRecoverableErrorMaskedByFallbackButLogged()
        //        {
        //            var command = getRecoverableErrorCommand(ExecutionIsolationStrategy.Thread, TestCommandFactory.FallbackResult.SUCCESS);
        //            try
        //            {
        //                Assert.Equal(FlexibleTestCommand.FALLBACK_VALUE, command.ExecuteAsync().Wait());
        //            }
        //            catch (Exception e)
        //            {
        //               new Exception("we expect to receive a valid fallback");
        //            }

        //            Assert.True(command.ExecutionTimeInMilliseconds > -1);
        //            Assert.True(command.IsFailedExecution);

        //            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //            Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
        //            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
        //            Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));

        //            Assert.Equal(0, command.Metrics.CurrentConcurrentExecutionCount);
        //        }

        //        [Fact]
        //        public void testUnrecoverableErrorThrownWithNoFallback()
        //        {
        //            var command = getUnrecoverableErrorCommand(ExecutionIsolationStrategy.Thread, TestCommandFactory.FallbackResult.UNIMPLEMENTED);
        //            try
        //            {
        //                command.ExecuteAsync().Wait();
        //                //  new Exception("we expect to receive a " + Error.class.GetSimpleName());
        //            }
        //            catch (Exception e)
        //            {
        //                // the actual error is an extra cause level deep because  needs to wrap Throwable/Error as it's public
        //                // methods only support Exception and it's not a strong enough reason to break backwards compatibility and jump to version 2.x
        //                // so RuntimeException -> wrapper Exception -> actual Error
        //                Assert.Equal("Unrecoverable Error for TestCommand", e.InnerException.InnerException.GetMessage());
        //            }

        //            Assert.Equal("Unrecoverable Error for TestCommand", command.GetFailedExecutionException().InnerException.GetMessage());

        //            Assert.True(command.ExecutionTimeInMilliseconds > -1);
        //            Assert.True(command.IsFailedExecution);

        //            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //            Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //            Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
        //            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));

        //            Assert.Equal(0, command.Metrics.CurrentConcurrentExecutionCount);
        //        }

        //        [Fact] //even though fallback is implemented, that logic never fires, as this is an unrecoverable error and should be directly propagated to the caller
        //        public void testUnrecoverableErrorThrownWithFallback()
        //        {
        //            var command = getUnrecoverableErrorCommand(ExecutionIsolationStrategy.Thread, TestCommandFactory.FallbackResult.SUCCESS);
        //            try
        //            {
        //                command.ExecuteAsync().Wait();
        //                // new Exception("we expect to receive a " + Error.class.GetSimpleName());
        //            }
        //            catch (Exception e)
        //            {
        //                // the actual error is an extra cause level deep because  needs to wrap Throwable/Error as it's public
        //                // methods only support Exception and it's not a strong enough reason to break backwards compatibility and jump to version 2.x
        //                // so RuntimeException -> wrapper Exception -> actual Error
        //                Assert.Equal("Unrecoverable Error for TestCommand", e.InnerException.InnerException.GetMessage());
        //            }

        //            Assert.Equal("Unrecoverable Error for TestCommand", command.GetFailedExecutionException().InnerException.GetMessage());

        //            Assert.True(command.ExecutionTimeInMilliseconds > -1);
        //            Assert.True(command.IsFailedExecution);

        //            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //            Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //            Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
        //            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
        //            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
        //            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
        //            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));

        //            Assert.Equal(0, command.Metrics.CurrentConcurrentExecutionCount);
        //        }

        //        //   [Fact]
        //        //    public void testFallbackRejectionOccursWithLatentFallback() {
        //        //        int numCommands = 1000;
        //        //        int semaphoreSize = 600;
        //        //        List<var> cmds = new ArrayList<var>();
        //        //         AtomicInteger exceptionsSeen = new AtomicInteger(0);
        //        //         AtomicInteger fallbacksSeen = new AtomicInteger(0);
        //        //         ConcurrentMap<RuntimeException.FailureType, AtomicInteger> exceptionTypes = new ConcurrentHashMap<RuntimeException.FailureType, AtomicInteger>();
        //        //         CountDownLatch latch = new CountDownLatch(numCommands);
        //        //
        //        //        TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
        //        //        ThreadPool largeThreadPool = new ThreadPool.ThreadPoolDefault(ThreadPoolKey.Factory.asKey("LATENT_FALLBACK"), ThreadPoolProperties.Setter.GetUnitTestPropertiesBuilder().withCoreSize(numCommands));
        //        //        TryableSemaphore executionSemaphore = new TryableSemaphoreActual(Property.Factory.asProperty(numCommands));
        //        //        TryableSemaphore fallbackSemaphore = new AbstractCommand.TryableSemaphoreActual(Property.Factory.asProperty(semaphoreSize));
        //        //
        //        //        /**
        //        //         * The goal here is for all commands to fail immediately in the run() method, and then hit the fallback path.
        //        //         * The fallback path should be latent for all commands, and the fallback semaphore should saturate and
        //        //         * reject some fallbacks from occurring
        //        //         *
        //        //         * To accomplish this, I will set
        //        //         * - the threadpool that commands run in high (so commands don't get rejected by the threadpool),
        //        //         * - the execution semaphore high (so commands don't get rejected by that semaphore)
        //        //         * - the falback semaphore lower than the number of commands (so that some get fallback-rejected)
        //        //         */
        //        //
        //        //        for (int i = 0; i < numCommands; i++) {
        //        //            cmds.add(getFallbackLatentCommand(ExecutionIsolationStrategy.Thread, TestCommandFactory.FallbackResult.SUCCESS, 1000, circuitBreaker, largeThreadPool, executionSemaphore, fallbackSemaphore));
        //        //        }
        //        //
        //        //        for ( var cmd: cmds) {
        //        //             Runnable cmdExecution = new Runnable() {
        //        //                
        //        //                public void run() {
        //        //                    try {
        //        //                        cmd.ExecuteAsync().Wait();
        //        //                        fallbacksSeen.incrementAndGet();
        //        //                    } catch (RuntimeException hre) {
        //        //                        RuntimeException.FailureType ft = hre.GetFailureType();
        //        //                        AtomicInteger found = exceptionTypes.Get(ft);
        //        //                        if (found != null) {
        //        //                            found.incrementAndGet();
        //        //                        } else {
        //        //                            exceptionTypes.put(ft, new AtomicInteger(1));
        //        //                        }
        //        //                        exceptionsSeen.incrementAndGet();
        //        //                    } finally {
        //        //                        latch.countDown();
        //        //                    }
        //        //                }
        //        //            };
        //        //
        //        //            new Thread(cmdExecution).start();
        //        //        }
        //        //
        //        //        try {
        //        //            latch.await(30, TimeUnit.SECONDS);
        //        //
        //        //            Console.WriteLine("MAP : " + exceptionTypes);
        //        //        } catch (InterruptedException ie) {
        //        //           new Exception("Interrupted!");
        //        //        }
        //        //
        //        //        Console.WriteLine("NUM EXCEPTIONS : " + exceptionsSeen.Get());
        //        //        Assert.Equal(0, circuitBreaker.Metrics.GetCumulativeCount(RollingNumberEvent.SUCCESS));
        //        //        Assert.Equal(numCommands - semaphoreSize, circuitBreaker.Metrics.GetCumulativeCount(RollingNumberEvent.EXCEPTION_THROWN));
        //        //        Assert.Equal(numCommands, circuitBreaker.Metrics.GetCumulativeCount(RollingNumberEvent.FAILURE));
        //        //        Assert.Equal(0, circuitBreaker.Metrics.GetCumulativeCount(RollingNumberEvent.BAD_REQUEST));
        //        //        Assert.Equal(numCommands - semaphoreSize, circuitBreaker.Metrics.GetCumulativeCount(RollingNumberEvent.FALLBACK_REJECTION));
        //        //        Assert.Equal(0, circuitBreaker.Metrics.GetCumulativeCount(RollingNumberEvent.FALLBACK_FAILURE));
        //        //        Assert.Equal(semaphoreSize, circuitBreaker.Metrics.GetCumulativeCount(RollingNumberEvent.FALLBACK_SUCCESS));
        //        //        Assert.Equal(0, circuitBreaker.Metrics.GetCumulativeCount(RollingNumberEvent.SEMAPHORE_REJECTED));
        //        //        Assert.Equal(0, circuitBreaker.Metrics.GetCumulativeCount(RollingNumberEvent.SHORT_CIRCUITED));
        //        //        Assert.Equal(0, circuitBreaker.Metrics.GetCumulativeCount(RollingNumberEvent.THREAD_POOL_REJECTED));
        //        //        Assert.Equal(0, circuitBreaker.Metrics.GetCumulativeCount(RollingNumberEvent.TIMEOUT));
        //        //        Assert.Equal(0, circuitBreaker.Metrics.GetCumulativeCount(RollingNumberEvent.RESPONSE_FROM_CACHE));
        //        //    }
        //        //
        //        static class EventCommand : Command
        //        {
        //            public EventCommand()
        //            {
        //                super(Setter.withGroupKey(CommandGroupKey.Factory.asKey("eventGroup")).andCommandPropertiesDefaults(new CommandProperties.Setter().withFallbackIsolationSemaphoreMaxConcurrentRequests(3)));
        //            }


        //            protected String run()
        //            {
        //                Console.WriteLine(Thread.currentThread().GetName() + " : In run()");
        //                throw new RuntimeException("run_exception");
        //            }


        //            public String getFallback()
        //            {
        //                try
        //                {
        //                    Console.WriteLine(Thread.currentThread().GetName() + " : In fallback => " + getExecutionEvents());
        //                    Thread.sleep(30000L);
        //                }
        //                catch (InterruptedException e)
        //                {
        //                    Console.WriteLine(Thread.currentThread().GetName() + " : Interruption occurred");
        //                }
        //                Console.WriteLine(Thread.currentThread().GetName() + " : CMD Success Result");
        //                return "fallback";
        //            }
        //        }

        //        //if I set fallback semaphore to same as threadpool (10), I set up a race.
        //        //instead, I set fallback sempahore to much less (3).  This should guarantee that all fallbacks only happen in the threadpool, and main thread does not block
        //        [Fact(timeout = 5000)]
        //        public void testFallbackRejection()
        //        {
        //            for (int i = 0; i < 1000; i++)
        //            {
        //                EventCommand cmd = new EventCommand();

        //                try
        //                {
        //                    if (i == 500)
        //                    {
        //                        Thread.sleep(100L);
        //                    }
        //                    cmd.queue();
        //                    Console.WriteLine("queued: " + i);
        //                }
        //                catch (Exception e)
        //                {
        //                    Console.WriteLine("Fail Fast on queue() : " + cmd.GetExecutionEvents());

        //                }
        //            }
        //        }

        //        [Fact]
        //        public void testNonBlockingCommandQueueFiresTimeout()
        //        { //see https://github.com/Netflix//issues/514
        //            var cmd = getCommand(ExecutionIsolationStrategy.Thread, TestCommandFactory.ExecutionResult.SUCCESS, 200, TestCommandFactory.FallbackResult.SUCCESS, 50);

        //            //new Thread() {

        //            //    public void run() {
        //            //        cmd.queue();
        //            //    }
        //            //}.start();

        //            try
        //            {
        //                Thread.sleep(200);
        //                //timeout should occur in 50ms, and underlying thread should run for 500ms
        //                //therefore, after 200ms, the command should have finished with a fallback on timeout
        //            }
        //            catch (InterruptedException ie)
        //            {
        //                throw new RuntimeException(ie);
        //            }

        //            Assert.True(cmd.isExecutionComplete());
        //            Assert.True(cmd.isResponseTimedOut());

        //            Assert.Equal(0, cmd.Metrics.CurrentConcurrentExecutionCount);
        //        }



        //        protected void assertHooksOnSuccess(Func0<var> ctor, Action1<var> assertion)
        //        {
        //            assertExecute(ctor.call(), assertion, true);
        //            assertBlockingQueue(ctor.call(), assertion, true);
        //            assertNonBlockingQueue(ctor.call(), assertion, true, false);
        //            assertBlockingObserve(ctor.call(), assertion, true);
        //            assertNonBlockingObserve(ctor.call(), assertion, true);
        //        }


        //        protected void assertHooksOnFailure(Func0<var> ctor, Action1<var> assertion)
        //        {
        //            assertExecute(ctor.call(), assertion, false);
        //            assertBlockingQueue(ctor.call(), assertion, false);
        //            assertNonBlockingQueue(ctor.call(), assertion, false, false);
        //            assertBlockingObserve(ctor.call(), assertion, false);
        //            assertNonBlockingObserve(ctor.call(), assertion, false);
        //        }


        //        protected void assertHooksOnFailure(Func0<var> ctor, Action1<var> assertion, boolean failFast)
        //        {
        //            assertExecute(ctor.call(), assertion, false);
        //            assertBlockingQueue(ctor.call(), assertion, false);
        //            assertNonBlockingQueue(ctor.call(), assertion, false, failFast);
        //            assertBlockingObserve(ctor.call(), assertion, false);
        //            assertNonBlockingObserve(ctor.call(), assertion, false);
        //        }

        //        /**
        //         * Run the command via {@link com.netflix.hystrix.Command#execute()} and then assert
        //         * @param command command to run
        //         * @param assertion assertions to check
        //         * @param isSuccess should the command succeed?
        //         */
        //        private void assertExecute(var command, Action1<var> assertion, boolean isSuccess)
        //        {
        //            Console.WriteLine(System.currentTimeMillis() + " : " + Thread.currentThread().GetName() + " : Running command.ExecuteAsync().Wait() and then assertions...");
        //            if (isSuccess)
        //            {
        //                command.ExecuteAsync().Wait();
        //            }
        //            else
        //            {
        //                try
        //                {
        //                    Object o = command.ExecuteAsync().Wait();
        //                   new Exception("Expected a command failure!");
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine("Received expected ex : " + ex);
        //                    ex.printStackTrace();
        //                }
        //            }

        //            assertion.call(command);
        //        }

        //        /**
        //         * Run the command via {@link com.netflix.hystrix.Command#queue()}, immediately block, and then assert
        //         * @param command command to run
        //         * @param assertion assertions to check
        //         * @param isSuccess should the command succeed?
        //         */
        //        private void assertBlockingQueue(var command, Action1<var> assertion, boolean isSuccess)
        //        {
        //            Console.WriteLine("Running command.queue(), immediately blocking and then running assertions...");
        //            if (isSuccess)
        //            {
        //                try
        //                {
        //                    command.queue().Get();
        //                }
        //                catch (Exception e)
        //                {
        //                    throw new RuntimeException(e);
        //                }
        //            }
        //            else
        //            {
        //                try
        //                {
        //                    command.queue().Get();
        //                   new Exception("Expected a command failure!");
        //                }
        //                catch (InterruptedException ie)
        //                {
        //                    throw new RuntimeException(ie);
        //                }
        //                catch (ExecutionException ee)
        //                {
        //                    Console.WriteLine("Received expected ex : " + ee.InnerException);
        //                    ee.InnerException.printStackTrace();
        //                }
        //                catch (Exception e)
        //                {
        //                    Console.WriteLine("Received expected ex : " + e);

        //                }
        //            }

        //            assertion.call(command);
        //        }

        //        /**
        //         * Run the command via {@link com.netflix.hystrix.Command#queue()}, then poll for the command to be finished.
        //         * When it is finished, assert
        //         * @param command command to run
        //         * @param assertion assertions to check
        //         * @param isSuccess should the command succeed?
        //         */
        //        private void assertNonBlockingQueue(var command, Action1<var> assertion, boolean isSuccess, boolean failFast)
        //        {
        //            Console.WriteLine("Running command.queue(), sleeping the test thread until command is complete, and then running assertions...");
        //            Future f = null;
        //            if (failFast)
        //            {
        //                try
        //                {
        //                    f = command.queue();
        //                   new Exception("Expected a failure when queuing the command");
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine("Received expected fail fast ex : " + ex);
        //                    ex.printStackTrace();
        //                }
        //            }
        //            else
        //            {
        //                try
        //                {
        //                    f = command.queue();
        //                }
        //                catch (Exception ex)
        //                {
        //                    throw new RuntimeException(ex);
        //                }
        //            }
        //            awaitCommandCompletion(command);

        //            assertion.call(command);

        //            if (isSuccess)
        //            {
        //                try
        //                {
        //                    f.Get();
        //                }
        //                catch (Exception ex)
        //                {
        //                    throw new RuntimeException(ex);
        //                }
        //            }
        //            else
        //            {
        //                try
        //                {
        //                    f.Get();
        //                   new Exception("Expected a command failure!");
        //                }
        //                catch (InterruptedException ie)
        //                {
        //                    throw new RuntimeException(ie);
        //                }
        //                catch (ExecutionException ee)
        //                {
        //                    Console.WriteLine("Received expected ex : " + ee.InnerException);
        //                    ee.InnerException.printStackTrace();
        //                }
        //                catch (Exception e)
        //                {
        //                    Console.WriteLine("Received expected ex : " + e);

        //                }
        //            }
        //        }

        //        private void awaitCommandCompletion<T>(TestCommand<T> command)
        //        {
        //            while (!command.isExecutionComplete())
        //            {
        //                try
        //                {
        //                    Thread.sleep(10);
        //                }
        //                catch (InterruptedException e)
        //                {
        //                    throw new RuntimeException("interrupted");
        //                }
        //            }
        //        }

        //        /**
        //         * Test a command execution that fails but has a fallback.
        //         */
        //        [Fact]
        //        public void testExecutionFailureWithFallbackImplementedButDisabled()
        //        {
        //            TestCommand<bool> commandEnabled = new KnownFailureTestCommandWithFallback(new TestCircuitBreaker(), true);
        //            try
        //            {
        //                Assert.Equal(false, commandEnabled.ExecuteAsync().Wait());
        //            }
        //            catch (Exception e)
        //            {

        //               new Exception("We should have received a response from the fallback.");
        //            }

        //            TestCommand<bool> commandDisabled = new KnownFailureTestCommandWithFallback(new TestCircuitBreaker(), false);
        //            try
        //            {
        //                Assert.Equal(false, commandDisabled.ExecuteAsync().Wait());
        //               new Exception("expect exception thrown");
        //            }
        //            catch (Exception e)
        //            {
        //                // expected
        //            }

        //            Assert.Equal("we failed with a simulated issue", commandDisabled.GetFailedExecutionException().GetMessage());

        //            Assert.True(commandDisabled.IsFailedExecution);

        //            Assert.Equal(0, commandDisabled.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //            Assert.Equal(1, commandDisabled.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
        //            Assert.Equal(1, commandDisabled.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //            Assert.Equal(0, commandDisabled.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
        //            Assert.Equal(0, commandDisabled.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
        //            Assert.Equal(0, commandDisabled.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
        //            Assert.Equal(0, commandDisabled.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
        //            Assert.Equal(0, commandDisabled.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
        //            Assert.Equal(0, commandDisabled.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
        //            Assert.Equal(0, commandDisabled.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
        //            Assert.Equal(0, commandDisabled.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
        //            Assert.Equal(0, commandDisabled.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

        //            Assert.Equal(100, commandDisabled.Metrics.GetHealthCounts().ErrorPercentage);
        //            Assert.Equal(0, commandDisabled.Metrics.CurrentConcurrentExecutionCount);

        //            Assert.Equal(2, RequestLog.GetCurrentRequest().GetAllExecutedCommands().size());
        //        }

        //        [Fact]
        //        public void testExecutionTimeoutValue()
        //        {
        //            Command.Setter properties = Command.Setter
        //                    .withGroupKey(CommandGroupKey.Factory.asKey("TestKey"))
        //                    .andCommandPropertiesDefaults(CommandProperties.Setter()
        //                            .withExecutionTimeoutInMilliseconds(50));

        //            Command<String> command = new Command<String>(properties)
        //            {

        //                //protected String run()  {
        //                //    Thread.sleep(3000);
        //                //    // should never reach here
        //                //    return "hello";
        //                //}


        //                //protected String getFallback() {
        //                //    if (isResponseTimedOut()) {
        //                //        return "timed-out";
        //                //    } else {
        //                //        return "abc";
        //                //    }
        //                //}
        //            };

        //            String value = command.ExecuteAsync().Wait();
        //            Assert.True(command.isResponseTimedOut());
        //            Assert.Equal("expected fallback value", "timed-out", value);

        //        }

        //        /**
        //         * See https://github.com/Netflix//issues/212
        //         */
        //        [Fact]
        //        public void testObservableTimeoutNoFallbackThreadContext()
        //        {
        //            TestSubscriber<Object> ts = new TestSubscriber<Object>();

        //            AtomicReference<Thread> onErrorThread = new AtomicReference<Thread>();
        //            AtomicBoolean isRequestContextInitialized = new AtomicBoolean();

        //            var command = getCommand(ExecutionIsolationStrategy.Thread, TestCommandFactory.ExecutionResult.SUCCESS, 200, TestCommandFactory.FallbackResult.UNIMPLEMENTED, 50);
        //            command.toObservable().doOnError(new Action1<Throwable>()
        //            {


        //                //public void call(Throwable t1) {
        //                //    Console.WriteLine("onError: " + t1);
        //                //    Console.WriteLine("onError Thread: " + Thread.currentThread());
        //                //    Console.WriteLine("ThreadContext in onError: " + RequestContext.isCurrentThreadInitialized());
        //                //    onErrorThread.set(Thread.currentThread());
        //                //    isRequestContextInitialized.set(RequestContext.isCurrentThreadInitialized());
        //                //}

        //            }).subscribe(ts);

        //            ts.awaitTerminalEvent();

        //            Assert.True(isRequestContextInitialized.Get());
        //            Assert.True(onErrorThread.Get().GetName().startsWith("Timer"));

        //            List<Throwable> errors = ts.GetOnErrorEvents();
        //            Assert.Equal(1, errors.size());
        //            Throwable e = errors.Get(0);
        //            if (errors.Get(0) is RuntimeException)
        //            {
        //                RuntimeException de = (RuntimeException)e;
        //                Assert.NotNull(de.FallbackException);
        //                Assert.True(de.FallbackException is UnsupportedOperationException);
        //                Assert.NotNull(de.CommandName);
        //                Assert.NotNull(de.InnerException);
        //                Assert.True(de.InnerException is TimeoutException);
        //            }
        //            else
        //            {
        //               new Exception("the exception should be ExecutionException with cause as RuntimeException");
        //            }

        //            Assert.True(command.ExecutionTimeInMilliseconds > -1);
        //            Assert.True(command.isResponseTimedOut());

        //            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //            Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
        //            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
        //            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
        //            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
        //            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
        //            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
        //            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
        //            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
        //            Assert.Equal(1, command.Metrics.GetRollingCount(RollingNumberEvent.TIMEOUT));
        //            Assert.Equal(0, command.Metrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));

        //            Assert.Equal(100, command.Metrics.GetHealthCounts().ErrorPercentage);
        //            Assert.Equal(0, command.Metrics.CurrentConcurrentExecutionCount);

        //            Assert.Equal(1, RequestLog.GetCurrentRequest().GetAllExecutedCommands().size());
        //        }

        //        [Fact]
        //        public void testExceptionConvertedToBadRequestExceptionInExecutionHookBypassesCircuitBreaker()
        //        {
        //            TestCircuitBreaker circuitBreaker = new TestCircuitBreaker();
        //            try
        //            {
        //                new ExceptionToBadRequestByExecutionHookCommand(circuitBreaker, ExecutionIsolationStrategy.Thread).ExecuteAsync().Wait();
        //                // new Exception("we expect to receive a " + BadRequestException.class.GetSimpleName());
        //            }
        //            catch (BadRequestException e)
        //            {
        //                // success

        //            }
        //            catch (Exception e)
        //            {

        //                // new Exception("We expect a " + BadRequestException.class.GetSimpleName() + " but got a " + e.GetClass().GetSimpleName());
        //            }

        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.SUCCESS));
        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
        //            Assert.Equal(0, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.FAILURE));
        //            Assert.Equal(1, circuitBreaker.Metrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));

        //            Assert.Equal(0, circuitBreaker.Metrics.CurrentConcurrentExecutionCount);
        //        }

        //        [Fact]
        //        public void testInterruptFutureOnTimeout()
        //        {
        //            // given
        //            InterruptibleCommand cmd = new InterruptibleCommand(new TestCircuitBreaker(), true);

        //            // when
        //            Future<bool> f = cmd.queue();

        //            // then
        //            Thread.sleep(500);
        //            Assert.True(cmd.hasBeenInterrupted());
        //        }

        //        [Fact]
        //        public void testInterruptObserveOnTimeout()
        //        {
        //            // given
        //            InterruptibleCommand cmd = new InterruptibleCommand(new TestCircuitBreaker(), true);

        //            // when
        //            cmd.observe().subscribe();

        //            // then
        //            Thread.sleep(500);
        //            Assert.True(cmd.hasBeenInterrupted());
        //        }

        //        [Fact]
        //        public void testInterruptToObservableOnTimeout()
        //        {
        //            // given
        //            InterruptibleCommand cmd = new InterruptibleCommand(new TestCircuitBreaker(), true);

        //            // when
        //            cmd.toObservable().subscribe();

        //            // then
        //            Thread.sleep(500);
        //            Assert.True(cmd.hasBeenInterrupted());
        //        }

        //        [Fact]
        //        public void testDoNotInterruptFutureOnTimeoutIfPropertySaysNotTo()
        //        {
        //            // given
        //            InterruptibleCommand cmd = new InterruptibleCommand(new TestCircuitBreaker(), false);

        //            // when
        //            Future<bool> f = cmd.queue();

        //            // then
        //            Thread.sleep(500);
        //            Assert.False(cmd.hasBeenInterrupted());
        //        }

        //        [Fact]
        //        public void testDoNotInterruptObserveOnTimeoutIfPropertySaysNotTo()
        //        {
        //            // given
        //            InterruptibleCommand cmd = new InterruptibleCommand(new TestCircuitBreaker(), false);

        //            // when
        //            cmd.observe().subscribe();

        //            // then
        //            Thread.sleep(500);
        //            Assert.False(cmd.hasBeenInterrupted());
        //        }

        //        [Fact]
        //        public void testDoNotInterruptToObservableOnTimeoutIfPropertySaysNotTo()
        //        {
        //            // given
        //            InterruptibleCommand cmd = new InterruptibleCommand(new TestCircuitBreaker(), false);

        //            // when
        //            cmd.toObservable().subscribe();

        //            // then
        //            Thread.sleep(500);
        //            Assert.False(cmd.hasBeenInterrupted());
        //        }

        //        [Fact]
        //        public void testChainedCommand()
        //        {
        //            //class SubCommand : TestCommand<Integer> {

        //            //    public SubCommand(TestCircuitBreaker circuitBreaker) {
        //            //        super(testPropsBuilder().setCircuitBreaker(circuitBreaker).setMetrics(circuitBreaker.Metrics));
        //            //    }


        //            //    protected Integer run()  {
        //            //        return 2;
        //            //    }
        //            //}

        //            //class PrimaryCommand : TestCommand<Integer> {
        //            //    public PrimaryCommand(TestCircuitBreaker circuitBreaker) {
        //            //        super(testPropsBuilder().setCircuitBreaker(circuitBreaker).setMetrics(circuitBreaker.Metrics));
        //            //    }


        //            //    protected Integer run()  {
        //            //        throw new RuntimeException("primary failure");
        //            //    }


        //            //    protected Integer getFallback() {
        //            //        SubCommand subCmd = new SubCommand(new TestCircuitBreaker());
        //            //        return subCmd.ExecuteAsync().Wait();
        //            //    }
        //            //}

        //            //Assert.True(2 == new PrimaryCommand(new TestCircuitBreaker()).ExecuteAsync().Wait());
        //        }

        //        [Fact]
        //        public void testSlowFallback()
        //        {
        //            //class PrimaryCommand : TestCommand<Integer> {
        //            //    public PrimaryCommand(TestCircuitBreaker circuitBreaker) {
        //            //        super(testPropsBuilder().setCircuitBreaker(circuitBreaker).setMetrics(circuitBreaker.Metrics));
        //            //    }


        //            //    protected Integer run()  {
        //            //        throw new RuntimeException("primary failure");
        //            //    }


        //            //    protected Integer getFallback() {
        //            //        try {
        //            //            Thread.sleep(1500);
        //            //            return 1;
        //            //        } catch (InterruptedException ie) {
        //            //            Console.WriteLine("Caught Interrupted Exception");
        //            //            i
        //            //        }
        //            //        return -1;
        //            //    }
        //            //}

        //            //Assert.True(1 == new PrimaryCommand(new TestCircuitBreaker()).ExecuteAsync().Wait());
        //        }

        //        [Fact]
        //        public void testOnRunStartHookThrows()
        //        {
        //            AtomicBoolean threadExceptionEncountered = new AtomicBoolean(false);
        //            AtomicBoolean semaphoreExceptionEncountered = new AtomicBoolean(false);
        //            AtomicBoolean onThreadStartInvoked = new AtomicBoolean(false);
        //            AtomicBoolean onThreadCompleteInvoked = new AtomicBoolean(false);

        //            //class FailureInjectionHook : CommandExecutionHook {

        //            //    public  void onExecutionStart(Invokable<T> commandInstance) {
        //            //        throw new RuntimeException(RuntimeException.FailureType.COMMAND_EXCEPTION, commandInstance.GetClass(), "Injected Failure", null, null);
        //            //    }


        //            //    public  void onThreadStart(Invokable<T> commandInstance) {
        //            //        onThreadStartInvoked.set(true);
        //            //        super.onThreadStart(commandInstance);
        //            //    }


        //            //    public  void onThreadComplete(Invokable<T> commandInstance) {
        //            //        onThreadCompleteInvoked.set(true);
        //            //        super.onThreadComplete(commandInstance);
        //            //    }
        //            //}

        //            // FailureInjectionHook failureInjectionHook = new FailureInjectionHook();

        //            //class FailureInjectedCommand : TestCommand<Integer> {
        //            //    public FailureInjectedCommand(ExecutionIsolationStrategy isolationStrategy) {
        //            //        super(testPropsBuilder().setCommandPropertiesDefaults(CommandPropertiesTest.GetUnitTestPropertiesSetter().withExecutionIsolationStrategy(isolationStrategy)), failureInjectionHook);
        //            //    }


        //            //    protected Integer run()  {
        //            //        return 3;
        //            //    }
        //            //}

        //            //TestCommand<Integer> threadCmd = new FailureInjectedCommand(ExecutionIsolationStrategy.Thread);
        //            //try {
        //            //    int result = threadCmd.ExecuteAsync().Wait();
        //            //    Console.WriteLine("RESULT : " + result);
        //            //} catch (Throwable ex) {
        //            //    ex.printStackTrace();
        //            //    threadExceptionEncountered.set(true);
        //            //}
        //            //Assert.True(threadExceptionEncountered.Get());
        //            //Assert.True(onThreadStartInvoked.Get());
        //            //Assert.True(onThreadCompleteInvoked.Get());

        //            //TestCommand<Integer> semaphoreCmd = new FailureInjectedCommand(ExecutionIsolationStrategy.SEMAPHORE);
        //            //try {
        //            //    int result = semaphoreCmd.ExecuteAsync().Wait();
        //            //    Console.WriteLine("RESULT : " + result);
        //            //} catch (Throwable ex) {
        //            //    ex.printStackTrace();
        //            //    semaphoreExceptionEncountered.set(true);
        //            //}
        //            //Assert.True(semaphoreExceptionEncountered.Get());
        //        }

        /* ******************************************************************************** */
        /* ******************************************************************************** */
        /* private Command class implementations for unit testing */
        /* ******************************************************************************** */
        /* ******************************************************************************** */


        //ServiceCommand<T> getCommand<T>(ExecutionIsolationStrategy isolationStrategy, TestCommandFactory.ExecutionResult executionResult, int executionLatency, TestCommandFactory.FallbackResult fallbackResult, int fallbackLatency, TestCircuitBreaker circuitBreaker, ThreadPool threadPool, int timeout, TestCommandFactory.CacheEnabled cacheEnabled, Object value, TryableSemaphore executionSemaphore, TryableSemaphore fallbackSemaphore, boolean circuitBreakerDisabled)
        //{
        //    return FlexibleTestCommand.from(isolationStrategy, executionResult, executionLatency, fallbackResult, fallbackLatency, circuitBreaker, threadPool, timeout, cacheEnabled, value, executionSemaphore, fallbackSemaphore, circuitBreakerDisabled);
        //}

        //private static class FlexibleTestCommand
        //{

        //    public static int EXECUTE_VALUE = 1;
        //    public static int FALLBACK_VALUE = 11;

        //    public static AbstractFlexibleTestCommand from(ExecutionIsolationStrategy isolationStrategy, TestCommandFactory.ExecutionResult executionResult, int executionLatency, TestCommandFactory.FallbackResult fallbackResult, int fallbackLatency, TestCircuitBreaker circuitBreaker, ThreadPool threadPool, int timeout, TestCommandFactory.CacheEnabled cacheEnabled, Object value, TryableSemaphore executionSemaphore, TryableSemaphore fallbackSemaphore, boolean circuitBreakerDisabled)
        //    {
        //        if (fallbackResult.equals(TestCommandFactory.FallbackResult.UNIMPLEMENTED))
        //        {
        //            return new FlexibleTestCommandNoFallback(isolationStrategy, executionResult, executionLatency, circuitBreaker, threadPool, timeout, cacheEnabled, value, executionSemaphore, fallbackSemaphore, circuitBreakerDisabled);
        //        }
        //        else
        //        {
        //            return new FlexibleTestCommandWithFallback(isolationStrategy, executionResult, executionLatency, fallbackResult, fallbackLatency, circuitBreaker, threadPool, timeout, cacheEnabled, value, executionSemaphore, fallbackSemaphore, circuitBreakerDisabled);
        //        }
        //    }
        //}

        //private class AbstractFlexibleTestCommand : ServiceCommand<int>
        //{
        //    protected TestCommandFactory.ExecutionResult executionResult;
        //    protected int executionLatency;

        //    protected bool cacheEnabled;
        //    protected Object value;

        //    AbstractFlexibleTestCommand(ExecutionIsolationStrategy isolationStrategy, TestCommandFactory.ExecutionResult executionResult, int executionLatency, TestCircuitBreaker circuitBreaker, ThreadPool threadPool, int timeout, CacheEnabled cacheEnabled, Object value, TryableSemaphore executionSemaphore, TryableSemaphore fallbackSemaphore, boolean circuitBreakerDisabled)
        //    {
        //        super(testPropsBuilder()
        //                .setCircuitBreaker(circuitBreaker)
        //                .setMetrics(circuitBreaker.Metrics)
        //                .setThreadPool(threadPool)
        //                .setCommandPropertiesDefaults(CommandPropertiesTest.GetUnitTestPropertiesSetter()
        //                        .withExecutionIsolationStrategy(isolationStrategy)
        //                        .withExecutionTimeoutInMilliseconds(timeout)
        //                        .withCircuitBreakerEnabled(!circuitBreakerDisabled))
        //                .setExecutionSemaphore(executionSemaphore)
        //                .setFallbackSemaphore(fallbackSemaphore));
        //        this.executionResult = executionResult;
        //        this.executionLatency = executionLatency;

        //        this.cacheEnabled = cacheEnabled;
        //        this.value = value;
        //    }

        //    protected override Task<int> Run(CancellationToken cancellationToken)
        //    {
        //        //  Console.WriteLine(System.currentTimeMillis() + " : " + Thread.currentThread().GetName() + " starting the run() method");
        //        addLatency(executionLatency);
        //        if (executionResult == TestCommandFactory.ExecutionResult.SUCCESS)
        //        {
        //            return Task.FromResult(FlexibleTestCommand.EXECUTE_VALUE);
        //        }
        //        else if (executionResult == TestCommandFactory.ExecutionResult.FAILURE)
        //        {
        //            throw new RuntimeException(FailureType.COMMAND_EXCEPTION, "Execution Failure for TestCommand");
        //        }
        //        else if (executionResult == TestCommandFactory.ExecutionResult.HYSTRIX_FAILURE)
        //        {
        //            throw new RuntimeException(FailureType.COMMAND_EXCEPTION, "Execution  Failure for TestCommand");//, new RuntimeException("Execution Failure for TestCommand"), new RuntimeException("Fallback Failure for TestCommand"));
        //        }
        //        else if (executionResult == TestCommandFactory.ExecutionResult.RECOVERABLE_ERROR)
        //        {
        //            throw new Exception("Execution ERROR for TestCommand");
        //        }
        //        else if (executionResult == TestCommandFactory.ExecutionResult.UNRECOVERABLE_ERROR)
        //        {
        //            throw new StackOverflowException("Unrecoverable Error for TestCommand");
        //        }
        //        else if (executionResult == TestCommandFactory.ExecutionResult.BAD_REQUEST)
        //        {
        //            throw new BadRequestException("Execution BadRequestException for TestCommand");
        //        }
        //        else
        //        {
        //            throw new RuntimeException(FailureType.COMMAND_EXCEPTION, "You passed in a executionResult enum that can't be represented in Command: " + executionResult);
        //        }
        //    }

        //    protected override string GetCacheKey()
        //    {
        //        if (cacheEnabled == true)
        //            return value.ToString();
        //        else
        //            return null;
        //    }

        //    protected void addLatency(int latency)
        //    {
        //        if (latency > 0)
        //        {
        //            try
        //            {
        //                //   Console.WriteLine(System.currentTimeMillis() + " : " + Thread.currentThread().GetName() + " About to sleep for : " + latency);
        //                Thread.Sleep(latency);
        //                //   Console.WriteLine(System.currentTimeMillis() + " : " + Thread.currentThread().GetName() + " Woke up from sleep!");
        //            }
        //            catch (OperationCanceledException e)
        //            {

        //                // ignore and sleep some more to simulate a dependency that doesn't obey interrupts
        //                try
        //                {
        //                    Thread.Sleep(latency);
        //                }
        //                catch (Exception)
        //                {
        //                    // ignore
        //                }
        //                Console.WriteLine("after interruption with extra sleep");
        //            }
        //        }
        //    }

        //}

        //private class FlexibleTestCommandWithFallback : AbstractFlexibleTestCommand
        //{
        //    protected TestCommandFactory.FallbackResult fallbackResult;
        //    protected int fallbackLatency;

        //    FlexibleTestCommandWithFallback(ExecutionIsolationStrategy isolationStrategy, TestCommandFactory.ExecutionResult executionResult, int executionLatency, FallbackResult fallbackResult, int fallbackLatency, TestCircuitBreaker circuitBreaker, ThreadPool threadPool, int timeout, CacheEnabled cacheEnabled, Object value, TryableSemaphore executionSemaphore, TryableSemaphore fallbackSemaphore, boolean circuitBreakerDisabled)
        //    {
        //        super(isolationStrategy, executionResult, executionLatency, circuitBreaker, threadPool, timeout, cacheEnabled, value, executionSemaphore, fallbackSemaphore, circuitBreakerDisabled);
        //        this.fallbackResult = fallbackResult;
        //        this.fallbackLatency = fallbackLatency;
        //    }


        //    protected override int GetFallback()
        //    {
        //        addLatency(fallbackLatency);
        //        if (fallbackResult == TestCommandFactory.FallbackResult.SUCCESS)
        //        {
        //            return FlexibleTestCommand.FALLBACK_VALUE;
        //        }
        //        else if (fallbackResult == TestCommandFactory.FallbackResult.FAILURE)
        //        {
        //            throw new RuntimeException("Fallback Failure for TestCommand");
        //        }
        //        else if (fallbackResult == FallbackResult.UNIMPLEMENTED)
        //        {
        //            return base.GetFallback();
        //        }
        //        else
        //        {
        //            throw new RuntimeException("You passed in a fallbackResult enum that can't be represented in Command: " + fallbackResult);
        //        }
        //    }
        //}

        //        private static class FlexibleTestCommandNoFallback : AbstractFlexibleTestCommand
        //        {
        //            FlexibleTestCommandNoFallback(ExecutionIsolationStrategy isolationStrategy, TestCommandFactory.ExecutionResult executionResult, int executionLatency, TestCircuitBreaker circuitBreaker, ThreadPool threadPool, int timeout, CacheEnabled cacheEnabled, Object value, TryableSemaphore executionSemaphore, TryableSemaphore fallbackSemaphore, boolean circuitBreakerDisabled)
        //            {
        //                super(isolationStrategy, executionResult, executionLatency, circuitBreaker, threadPool, timeout, cacheEnabled, value, executionSemaphore, fallbackSemaphore, circuitBreakerDisabled);
        //            }
        //        }

        //        /**
        //         * Successful execution - no fallback implementation.
        //         */
        //        private static class SuccessfulTestCommand : TestCommand<bool>
        //        {

        //            public SuccessfulTestCommand()
        //            {
        //                this(CommandPropertiesTest.GetUnitTestPropertiesSetter());
        //            }

        //            public SuccessfulTestCommand(CommandProperties.Setter properties)
        //            {
        //                super(testPropsBuilder().setCommandPropertiesDefaults(properties));
        //            }


        //            protected bool run()
        //            {
        //                return true;
        //            }

        //        }

        //        /**
        //         * Successful execution - no fallback implementation.
        //         */
        //        private static class DynamicOwnerTestCommand : TestCommand<bool>
        //        {

        //            public DynamicOwnerTestCommand(CommandGroupKey owner)
        //            {
        //                super(testPropsBuilder().setOwner(owner));
        //            }


        //            protected bool run()
        //            {
        //                Console.WriteLine("successfully executed");
        //                return true;
        //            }

        //        }

        //        /**
        //         * Successful execution - no fallback implementation.
        //         */
        //        private static class DynamicOwnerAndKeyTestCommand : TestCommand<bool>
        //        {

        //            public DynamicOwnerAndKeyTestCommand(CommandGroupKey owner, CommandKey key)
        //            {
        //                super(testPropsBuilder().setOwner(owner).setCommandKey(key).setCircuitBreaker(null).setMetrics(null));
        //                // we specifically are NOT passing in a circuit breaker here so we test that it creates a new one correctly based on the dynamic key
        //            }


        //            protected bool run()
        //            {
        //                Console.WriteLine("successfully executed");
        //                return true;
        //            }

        //        }

        //        /**
        //         * Failed execution with known exception (Exception) - no fallback implementation.
        //         */
        //        private static class KnownFailureTestCommandWithoutFallback : TestCommand<bool>
        //        {

        //            private KnownFailureTestCommandWithoutFallback(TestCircuitBreaker circuitBreaker)
        //            {
        //                super(testPropsBuilder().setCircuitBreaker(circuitBreaker).setMetrics(circuitBreaker.Metrics));
        //            }


        //            protected bool run()
        //            {
        //                Console.WriteLine("*** simulated failed execution *** ==> " + Thread.currentThread());
        //                throw new RuntimeException("we failed with a simulated issue");
        //            }

        //        }

        //        /**
        //         * Failed execution - fallback implementation successfully returns value.
        //         */
        //        private static class KnownFailureTestCommandWithFallback : TestCommand<bool>
        //        {

        //            public KnownFailureTestCommandWithFallback(TestCircuitBreaker circuitBreaker)
        //            {
        //                super(testPropsBuilder().setCircuitBreaker(circuitBreaker).setMetrics(circuitBreaker.Metrics));
        //            }

        //            public KnownFailureTestCommandWithFallback(TestCircuitBreaker circuitBreaker, boolean fallbackEnabled)
        //            {
        //                super(testPropsBuilder().setCircuitBreaker(circuitBreaker).setMetrics(circuitBreaker.Metrics)
        //                        .setCommandPropertiesDefaults(CommandPropertiesTest.GetUnitTestPropertiesSetter().withFallbackEnabled(fallbackEnabled)));
        //            }


        //            protected bool run()
        //            {
        //                Console.WriteLine("*** simulated failed execution ***");
        //                throw new RuntimeException("we failed with a simulated issue");
        //            }


        //            protected bool getFallback()
        //            {
        //                return false;
        //            }
        //        }

        //        /**
        //         * A Command implementation that supports caching.
        //         */
        //        private static class SuccessfulCacheableCommand<T> : TestCommand<T>
        //        {

        //            private boolean cacheEnabled;
        //            private volatile boolean executed = false;
        //            private T value;

        //            public SuccessfulCacheableCommand(TestCircuitBreaker circuitBreaker, boolean cacheEnabled, T value)
        //            {
        //                super(testPropsBuilder().setCircuitBreaker(circuitBreaker).setMetrics(circuitBreaker.Metrics));
        //                this.value = value;
        //                this.cacheEnabled = cacheEnabled;
        //            }


        //            protected T run()
        //            {
        //                executed = true;
        //                Console.WriteLine("successfully executed");
        //                return value;
        //            }

        //            public boolean isCommandRunningInThread()
        //            {
        //                return super.GetProperties().executionIsolationStrategy().Get().equals(ExecutionIsolationStrategy.Thread);
        //            }


        //            public String getCacheKey()
        //            {
        //                if (cacheEnabled)
        //                    return value.toString();
        //                else
        //                    return null;
        //            }
        //        }

        //        /**
        //         * A Command implementation that supports caching.
        //         */
        //        private static class SuccessfulCacheableCommandViaSemaphore : TestCommand<String>
        //        {

        //            private boolean cacheEnabled;
        //            private volatile boolean executed = false;
        //            private String value;

        //            public SuccessfulCacheableCommandViaSemaphore(TestCircuitBreaker circuitBreaker, boolean cacheEnabled, String value)
        //            {
        //                super(testPropsBuilder().setCircuitBreaker(circuitBreaker).setMetrics(circuitBreaker.Metrics)
        //                        .setCommandPropertiesDefaults(CommandPropertiesTest.GetUnitTestPropertiesSetter().withExecutionIsolationStrategy(ExecutionIsolationStrategy.SEMAPHORE)));
        //                this.value = value;
        //                this.cacheEnabled = cacheEnabled;
        //            }


        //            protected String run()
        //            {
        //                executed = true;
        //                Console.WriteLine("successfully executed");
        //                return value;
        //            }

        //            public boolean isCommandRunningInThread()
        //            {
        //                return super.GetProperties().executionIsolationStrategy().Get().equals(ExecutionIsolationStrategy.Thread);
        //            }


        //            public String getCacheKey()
        //            {
        //                if (cacheEnabled)
        //                    return value;
        //                else
        //                    return null;
        //            }
        //        }

        //        /**
        //         * A Command implementation that supports caching and execution takes a while.
        //         * <p>
        //         * Used to test scenario where Futures are returned with a backing call still executing.
        //         */
        //        private static class SlowCacheableCommand : TestCommand<String>
        //        {

        //            private String value;
        //            private int duration;
        //            private volatile boolean executed = false;

        //            public SlowCacheableCommand(TestCircuitBreaker circuitBreaker, String value, int duration)
        //            {
        //                super(testPropsBuilder().setCircuitBreaker(circuitBreaker).setMetrics(circuitBreaker.Metrics));
        //                this.value = value;
        //                this.duration = duration;
        //            }


        //            protected String run()
        //            {
        //                executed = true;
        //                try
        //                {
        //                    Thread.sleep(duration);
        //                }
        //                catch (Exception e)
        //                {

        //                }
        //                Console.WriteLine("successfully executed");
        //                return value;
        //            }


        //            public String getCacheKey()
        //            {
        //                return value;
        //            }
        //        }

        //        /**
        //         * Successful execution - no fallback implementation, circuit-breaker disabled.
        //         */
        //        private static class TestCommandWithoutCircuitBreaker : TestCommand<bool>
        //        {

        //            private TestCommandWithoutCircuitBreaker()
        //            {
        //                super(testPropsBuilder().setCommandPropertiesDefaults(CommandPropertiesTest.GetUnitTestPropertiesSetter().withCircuitBreakerEnabled(false)));
        //            }


        //            protected bool run()
        //            {
        //                Console.WriteLine("successfully executed");
        //                return true;
        //            }
        //        }

        //        /**
        //         * This has a ThreadPool that has a single thread and queueSize of 1.
        //         */
        //        private static class TestCommandRejection : TestCommand<bool>
        //        {

        //            private static int FALLBACK_NOT_IMPLEMENTED = 1;
        //            private static int FALLBACK_SUCCESS = 2;
        //            private static int FALLBACK_FAILURE = 3;

        //            private int fallbackBehavior;

        //            private int sleepTime;

        //            private TestCommandRejection(TestCircuitBreaker circuitBreaker, ThreadPool threadPool, int sleepTime, int timeout, int fallbackBehavior)
        //            {
        //                super(testPropsBuilder().setThreadPool(threadPool).setCircuitBreaker(circuitBreaker).setMetrics(circuitBreaker.Metrics)
        //                        .setCommandPropertiesDefaults(CommandPropertiesTest.GetUnitTestPropertiesSetter().withExecutionTimeoutInMilliseconds(timeout)));
        //                this.fallbackBehavior = fallbackBehavior;
        //                this.sleepTime = sleepTime;
        //            }


        //            protected bool run()
        //            {
        //                Console.WriteLine(">>> TestCommandRejection running");
        //                try
        //                {
        //                    Thread.sleep(sleepTime);
        //                }
        //                catch (InterruptedException e)
        //                {

        //                }
        //                return true;
        //            }


        //            protected bool getFallback()
        //            {
        //                if (fallbackBehavior == FALLBACK_SUCCESS)
        //                {
        //                    return false;
        //                }
        //                else if (fallbackBehavior == FALLBACK_FAILURE)
        //                {
        //                    throw new RuntimeException("failed on fallback");
        //                }
        //                else
        //                {
        //                    // FALLBACK_NOT_IMPLEMENTED
        //                    return super.GetFallback();
        //                }
        //            }
        //        }

        //        /**
        //         * Command that receives a custom thread-pool, sleepTime, timeout
        //         */
        //        private static class CommandWithCustomThreadPool : TestCommand<bool>
        //        {

        //            public boolean didExecute = false;

        //            private int sleepTime;

        //            private CommandWithCustomThreadPool(TestCircuitBreaker circuitBreaker, ThreadPool threadPool, int sleepTime, CommandProperties.Setter properties)
        //            {
        //                super(testPropsBuilder().setThreadPool(threadPool).setCircuitBreaker(circuitBreaker).setMetrics(circuitBreaker.Metrics).setCommandPropertiesDefaults(properties));
        //                this.sleepTime = sleepTime;
        //            }


        //            protected bool run()
        //            {
        //                Console.WriteLine("**** Executing CommandWithCustomThreadPool. Execution => " + sleepTime);
        //                didExecute = true;
        //                try
        //                {
        //                    Thread.sleep(sleepTime);
        //                }
        //                catch (InterruptedException e)
        //                {

        //                }
        //                return true;
        //            }
        //        }

        //        /**
        //         * The run() will fail and getFallback() take a long time.
        //         */
        //        private static class TestSemaphoreCommandWithSlowFallback : TestCommand<bool>
        //        {

        //            private long fallbackSleep;

        //            private TestSemaphoreCommandWithSlowFallback(TestCircuitBreaker circuitBreaker, int fallbackSemaphoreExecutionCount, long fallbackSleep)
        //            {
        //                super(testPropsBuilder().setCircuitBreaker(circuitBreaker).setMetrics(circuitBreaker.Metrics)
        //                        .setCommandPropertiesDefaults(CommandPropertiesTest.GetUnitTestPropertiesSetter()
        //                                    .withFallbackIsolationSemaphoreMaxConcurrentRequests(fallbackSemaphoreExecutionCount)
        //                                    .withExecutionIsolationThreadInterruptOnTimeout(false)));
        //                this.fallbackSleep = fallbackSleep;
        //            }


        //            protected bool run()
        //            {
        //                throw new RuntimeException("run fails");
        //            }


        //            protected bool getFallback()
        //            {
        //                try
        //                {
        //                    Thread.sleep(fallbackSleep);
        //                }
        //                catch (InterruptedException e)
        //                {

        //                }
        //                return true;
        //            }
        //        }

        //        private static class NoRequestCacheTimeoutWithoutFallback : TestCommand<bool>
        //        {
        //            public NoRequestCacheTimeoutWithoutFallback(TestCircuitBreaker circuitBreaker)
        //            {
        //                super(testPropsBuilder().setCircuitBreaker(circuitBreaker).setMetrics(circuitBreaker.Metrics)
        //                        .setCommandPropertiesDefaults(CommandPropertiesTest.GetUnitTestPropertiesSetter().withExecutionTimeoutInMilliseconds(200)));

        //                // we want it to timeout
        //            }


        //            protected bool run()
        //            {
        //                try
        //                {
        //                    Thread.sleep(500);
        //                }
        //                catch (InterruptedException e)
        //                {
        //                    Console.WriteLine(">>>> Sleep Interrupted: " + e.GetMessage());
        //                    //                    
        //                }
        //                return true;
        //            }


        //            public String getCacheKey()
        //            {
        //                return null;
        //            }
        //        }

        //        /**
        //         * The run() will take time. Configurable fallback implementation.
        //         */
        //        private static class TestSemaphoreCommand : TestCommand<bool>
        //        {

        //            private long executionSleep;

        //            private static int RESULT_SUCCESS = 1;
        //            private static int RESULT_FAILURE = 2;
        //            private static int RESULT_BAD_REQUEST_EXCEPTION = 3;

        //            private int resultBehavior;

        //            private static int FALLBACK_SUCCESS = 10;
        //            private static int FALLBACK_NOT_IMPLEMENTED = 11;
        //            private static int FALLBACK_FAILURE = 12;

        //            private int fallbackBehavior;

        //            private TestSemaphoreCommand(TestCircuitBreaker circuitBreaker, int executionSemaphoreCount, long executionSleep, int resultBehavior, int fallbackBehavior)
        //            {
        //                super(testPropsBuilder().setCircuitBreaker(circuitBreaker).setMetrics(circuitBreaker.Metrics)
        //                        .setCommandPropertiesDefaults(CommandPropertiesTest.GetUnitTestPropertiesSetter()
        //                                .withExecutionIsolationStrategy(ExecutionIsolationStrategy.SEMAPHORE)
        //                                .withExecutionIsolationSemaphoreMaxConcurrentRequests(executionSemaphoreCount)));
        //                this.executionSleep = executionSleep;
        //                this.resultBehavior = resultBehavior;
        //                this.fallbackBehavior = fallbackBehavior;
        //            }

        //            private TestSemaphoreCommand(TestCircuitBreaker circuitBreaker, TryableSemaphore semaphore, long executionSleep, int resultBehavior, int fallbackBehavior)
        //            {
        //                super(testPropsBuilder().setCircuitBreaker(circuitBreaker).setMetrics(circuitBreaker.Metrics)
        //                        .setCommandPropertiesDefaults(CommandPropertiesTest.GetUnitTestPropertiesSetter()
        //                                .withExecutionIsolationStrategy(ExecutionIsolationStrategy.SEMAPHORE))
        //                        .setExecutionSemaphore(semaphore));
        //                this.executionSleep = executionSleep;
        //                this.resultBehavior = resultBehavior;
        //                this.fallbackBehavior = fallbackBehavior;
        //            }


        //            protected bool run()
        //            {
        //                try
        //                {
        //                    Thread.sleep(executionSleep);
        //                }
        //                catch (InterruptedException e)
        //                {

        //                }
        //                if (resultBehavior == RESULT_SUCCESS)
        //                {
        //                    return true;
        //                }
        //                else if (resultBehavior == RESULT_FAILURE)
        //                {
        //                    throw new RuntimeException("TestSemaphoreCommand failure");
        //                }
        //                else if (resultBehavior == RESULT_BAD_REQUEST_EXCEPTION)
        //                {
        //                    throw new BadRequestException("TestSemaphoreCommand BadRequestException");
        //                }
        //                else
        //                {
        //                    throw new IllegalStateException("Didn't use a proper enum for result behavior");
        //                }
        //            }



        //            protected bool getFallback()
        //            {
        //                if (fallbackBehavior == FALLBACK_SUCCESS)
        //                {
        //                    return false;
        //                }
        //                else if (fallbackBehavior == FALLBACK_FAILURE)
        //                {
        //                    throw new RuntimeException("fallback failure");
        //                }
        //                else
        //                { //FALLBACK_NOT_IMPLEMENTED
        //                    return super.GetFallback();
        //                }
        //            }
        //        }

        //        /**
        //         * Semaphore based command that allows caller to use latches to know when it has started and signal when it
        //         * would like the command to finish
        //         */
        //        private static class LatchedSemaphoreCommand : TestCommand<bool>
        //        {

        //            private CountDownLatch startLatch, waitLatch;

        //            /**
        //             * 
        //             * @param circuitBreaker circuit breaker (passed in so it may be shared)
        //             * @param semaphore semaphore (passed in so it may be shared)
        //             * @param startLatch
        //             *            this command calls {@link java.util.concurrent.CountDownLatch#countDown()} immediately
        //             *            upon running
        //             * @param waitLatch
        //             *            this command calls {@link java.util.concurrent.CountDownLatch#await()} once it starts
        //             *            to run. The caller can use the latch to signal the command to finish
        //             */
        //            private LatchedSemaphoreCommand(TestCircuitBreaker circuitBreaker, TryableSemaphore semaphore,
        //                    CountDownLatch startLatch, CountDownLatch waitLatch)
        //            {
        //                super(testPropsBuilder().setCircuitBreaker(circuitBreaker).setMetrics(circuitBreaker.Metrics)
        //                        .setCommandPropertiesDefaults(CommandPropertiesTest.GetUnitTestPropertiesSetter().withExecutionIsolationStrategy(ExecutionIsolationStrategy.SEMAPHORE))
        //                        .setExecutionSemaphore(semaphore));
        //                this.startLatch = startLatch;
        //                this.waitLatch = waitLatch;
        //            }


        //            protected bool run()
        //            {
        //                // signals caller that run has started
        //                this.startLatch.countDown();

        //                try
        //                {
        //                    // waits for caller to countDown latch
        //                    this.waitLatch.await();
        //                }
        //                catch (InterruptedException e)
        //                {

        //                    return false;
        //                }
        //                return true;
        //            }
        //        }

        //        /**
        //         * The run() will take time. Contains fallback.
        //         */
        //        private static class TestSemaphoreCommandWithFallback : TestCommand<bool>
        //        {

        //            private long executionSleep;
        //            private bool fallback;

        //            private TestSemaphoreCommandWithFallback(TestCircuitBreaker circuitBreaker, int executionSemaphoreCount, long executionSleep, bool fallback)
        //            {
        //                super(testPropsBuilder().setCircuitBreaker(circuitBreaker).setMetrics(circuitBreaker.Metrics)
        //                        .setCommandPropertiesDefaults(CommandPropertiesTest.GetUnitTestPropertiesSetter().withExecutionIsolationStrategy(ExecutionIsolationStrategy.SEMAPHORE).withExecutionIsolationSemaphoreMaxConcurrentRequests(executionSemaphoreCount)));
        //                this.executionSleep = executionSleep;
        //                this.fallback = fallback;
        //            }


        //            protected bool run()
        //            {
        //                try
        //                {
        //                    Thread.sleep(executionSleep);
        //                }
        //                catch (InterruptedException e)
        //                {

        //                }
        //                return true;
        //            }


        //            protected bool getFallback()
        //            {
        //                return fallback;
        //            }

        //        }

        //        private static class RequestCacheNullPointerExceptionCase : TestCommand<bool>
        //        {
        //            public RequestCacheNullPointerExceptionCase(TestCircuitBreaker circuitBreaker)
        //            {
        //                super(testPropsBuilder().setCircuitBreaker(circuitBreaker).setMetrics(circuitBreaker.Metrics)
        //                        .setCommandPropertiesDefaults(CommandPropertiesTest.GetUnitTestPropertiesSetter().withExecutionTimeoutInMilliseconds(200)));
        //                // we want it to timeout
        //            }


        //            protected bool run()
        //            {
        //                try
        //                {
        //                    Thread.sleep(500);
        //                }
        //                catch (InterruptedException e)
        //                {

        //                }
        //                return true;
        //            }


        //            protected bool getFallback()
        //            {
        //                return false;
        //            }


        //            public String getCacheKey()
        //            {
        //                return "A";
        //            }
        //        }

        //        private static class RequestCacheTimeoutWithoutFallback : TestCommand<bool>
        //        {
        //            public RequestCacheTimeoutWithoutFallback(TestCircuitBreaker circuitBreaker)
        //            {
        //                super(testPropsBuilder().setCircuitBreaker(circuitBreaker).setMetrics(circuitBreaker.Metrics)
        //                        .setCommandPropertiesDefaults(CommandPropertiesTest.GetUnitTestPropertiesSetter().withExecutionTimeoutInMilliseconds(200)));
        //                // we want it to timeout
        //            }


        //            protected bool run()
        //            {
        //                try
        //                {
        //                    Thread.sleep(500);
        //                }
        //                catch (InterruptedException e)
        //                {
        //                    Console.WriteLine(">>>> Sleep Interrupted: " + e.GetMessage());
        //                    //                    
        //                }
        //                return true;
        //            }


        //            public String getCacheKey()
        //            {
        //                return "A";
        //            }
        //        }

        //        private static class RequestCacheThreadRejectionWithoutFallback : TestCommand<bool>
        //        {

        //            CountDownLatch completionLatch;

        //            public RequestCacheThreadRejectionWithoutFallback(TestCircuitBreaker circuitBreaker, CountDownLatch completionLatch)
        //            {
        //                super(testPropsBuilder()
        //                        .setCircuitBreaker(circuitBreaker)
        //                        .setMetrics(circuitBreaker.Metrics)
        //        //.setThreadPool(new ThreadPool() {


        //        //    public ThreadPoolExecutor getExecutor() {
        //        //        return null;
        //        //    }


        //        //    public void markThreadExecution() {

        //        //    }


        //        //    public void markThreadCompletion() {

        //        //    }


        //        //    public void markThreadRejection() {

        //        //    }


        //        //    public boolean isQueueSpaceAvailable() {
        //        //        // always return false so we reject everything
        //        //        return false;
        //        //    }


        //        //    public Scheduler getScheduler() {
        //        //        return new ContextScheduler(Plugins.GetInstance().GetConcurrencyStrategy(), this);
        //        //    }


        //        //    public Scheduler getScheduler(Func0<bool> shouldInterruptThread) {
        //        //        return new ContextScheduler(Plugins.GetInstance().GetConcurrencyStrategy(), this, shouldInterruptThread);
        //        //    }

        //        //                })
        //        );
        //                this.completionLatch = completionLatch;
        //            }


        //            protected bool run()
        //            {
        //                try
        //                {
        //                    if (completionLatch.await(1000, TimeUnit.MILLISECONDS))
        //                    {
        //                        throw new RuntimeException("timed out waiting on completionLatch");
        //                    }
        //                }
        //                catch (InterruptedException e)
        //                {
        //                    throw new RuntimeException(e);
        //                }
        //                return true;
        //            }


        //            public String getCacheKey()
        //            {
        //                return "A";
        //            }
        //        }

        //        private static class BadRequestCommand : TestCommand<bool>
        //        {

        //            public BadRequestCommand(TestCircuitBreaker circuitBreaker, ExecutionIsolationStrategy isolationType)
        //            {
        //                super(testPropsBuilder()
        //                        .setCircuitBreaker(circuitBreaker)
        //                        .setCommandPropertiesDefaults(CommandPropertiesTest.GetUnitTestPropertiesSetter().withExecutionIsolationStrategy(isolationType))
        //                        .setMetrics(circuitBreaker.Metrics));
        //            }


        //            protected bool run()
        //            {
        //                throw new BadRequestException("Message to developer that they passed in bad data or something like that.");
        //            }


        //            protected bool getFallback()
        //            {
        //                return false;
        //            }


        //            protected String getCacheKey()
        //            {
        //                return "one";
        //            }

        //        }

        //        private static class BusinessException : Exception
        //        {
        //            public BusinessException(String msg)
        //            {
        //                super(msg);
        //            }
        //        }

        //        private static class ExceptionToBadRequestByExecutionHookCommand : TestCommand<bool>
        //        {
        //            public ExceptionToBadRequestByExecutionHookCommand(TestCircuitBreaker circuitBreaker, ExecutionIsolationStrategy isolationType)
        //            {
        //                super(testPropsBuilder()
        //                        .setCircuitBreaker(circuitBreaker)
        //                        .setCommandPropertiesDefaults(CommandPropertiesTest.GetUnitTestPropertiesSetter().withExecutionIsolationStrategy(isolationType))
        //                        .setMetrics(circuitBreaker.Metrics)
        //                        .setExecutionHook(new TestableExecutionHook()
        //                        {

        //                            //public <T> Exception onRunError(Invokable<T> commandInstance, Exception e) {
        //                            //    super.onRunError(commandInstance, e);
        //                            //    return new BadRequestException("autoconverted exception", e);
        //                            //}
        //                        }));
        //            }


        //            protected bool run()
        //            {
        //                throw new BusinessException("invalid input by the user");
        //            }


        //            protected String getCacheKey()
        //            {
        //                return "nein";
        //            }
        //        }

        //        private static class CommandWithCheckedException : TestCommand<bool>
        //        {

        //            public CommandWithCheckedException(TestCircuitBreaker circuitBreaker)
        //            {
        //                super(testPropsBuilder()
        //                        .setCircuitBreaker(circuitBreaker).setMetrics(circuitBreaker.Metrics));
        //            }


        //            protected bool run()
        //            {
        //                throw new IOException("simulated checked exception message");
        //            }

        //        }

        //        private static class InterruptibleCommand : TestCommand<bool>
        //        {

        //            public InterruptibleCommand(TestCircuitBreaker circuitBreaker, boolean shouldInterrupt)
        //            {
        //                super(testPropsBuilder()
        //                        .setCircuitBreaker(circuitBreaker).setMetrics(circuitBreaker.Metrics)
        //                        .setCommandPropertiesDefaults(CommandPropertiesTest.GetUnitTestPropertiesSetter()
        //                                .withExecutionIsolationThreadInterruptOnTimeout(shouldInterrupt)
        //                                .withExecutionTimeoutInMilliseconds(100)));
        //            }

        //            private volatile boolean hasBeenInterrupted;

        //            public boolean hasBeenInterrupted()
        //            {
        //                return hasBeenInterrupted;
        //            }


        //            protected bool run()
        //            {
        //                try
        //                {
        //                    Thread.sleep(2000);
        //                }
        //                catch (InterruptedException e)
        //                {
        //                    Console.WriteLine("Interrupted!");

        //                    hasBeenInterrupted = true;
        //                }

        //                return hasBeenInterrupted;
        //            }
        //        }

        //        private static class CommandWithDisabledTimeout : TestCommand<bool>
        //        {
        //            private int latency;

        //            public CommandWithDisabledTimeout(int timeout, int latency)
        //            {
        //                super(testPropsBuilder().setCommandPropertiesDefaults(CommandPropertiesTest.GetUnitTestPropertiesSetter()
        //                        .withExecutionTimeoutInMilliseconds(timeout)
        //                        .withExecutionTimeoutEnabled(false)));
        //                this.latency = latency;
        //            }


        //            protected bool run()
        //            {
        //                try
        //                {
        //                    Thread.sleep(latency);
        //                    return true;
        //                }
        //                catch (InterruptedException ex)
        //                {
        //                    return false;
        //                }
        //            }


        //            protected bool getFallback()
        //            {
        //                return false;
        //            }
        //        }
        //    }
   // }

