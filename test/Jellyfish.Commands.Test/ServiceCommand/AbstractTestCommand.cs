using System;
using System.Threading.Tasks;
using System.Threading;
using Jellyfish.Commands.Utils;
using Xunit.Abstractions;

namespace Jellyfish.Commands.Tests
{
    class TestServiceCommand : ServiceCommand<int>
    {
        public Func<Task<int>> Action;
        private int executionLatency=0;
        private int fallbackLatency=0;
        private bool cacheEnabled=false;
        private int value=0;
        private static int cx;
        public int Id;

        private Func<Task<int>> fallback;
        public Func<Task<int>> Fallback
        {
            get { return fallback; }
            set
            {
                fallback = value;
                SetFlag(ServiceCommand<int>.ServiceCommandOptions.HasFallBack, value != null);
            }
        }

        public int ExecutionLatency
        {
            get
            {
                return executionLatency;
            }

            set
            {
                executionLatency = value;
            }
        }

        public int FallbackLatency
        {
            get
            {
                return fallbackLatency;
            }

            set
            {
                fallbackLatency = value;
            }
        }

        public bool CacheEnabled
        {
            get
            {
                return cacheEnabled;
            }

            set
            {
                cacheEnabled = value;
            }
        }

        public int Value
        {
            get
            {
                return value;
            }

            set
            {
                this.value = value;
            }
        }

        public Action<string> Log { get; internal set; }

        public TestServiceCommand(IJellyfishContext ctx, string name, CommandPropertiesBuilder builder, ExecutionIsolationStrategy strategy = ExecutionIsolationStrategy.Thread, IClock clock=null, TestCircuitBreaker circuitBreaker =null)
            : base(ctx ?? new MockJellyfishContext(), name, null, null, strategy, builder, clock, circuitBreaker, circuitBreaker?.Metrics)
        {
            SetFlag(ServiceCommand<int>.ServiceCommandOptions.HasFallBack, false);
            SetFlag(ServiceCommand<int>.ServiceCommandOptions.HasCacheKey, false);
            Id = cx++;
        }

        protected override async Task<int> Run(CancellationToken token)
        {
            await addLatency(ExecutionLatency, token);
            return await Action();
        }

        protected override async Task<int> GetFallback()
        {
            await addLatency(fallbackLatency, CancellationToken.None);
            return await Fallback();
        }

        protected override string GetCacheKey()
        {
            if (CacheEnabled == true)
                return Value.ToString();
            else
                return null;
        }

        protected async Task addLatency(int latency, CancellationToken token)
        {
            if (latency > 0)
            {
                    Log(Clock.GetInstance().EllapsedTimeInMs + " : " + this.CommandName + " About to sleep for : " + latency);
                    await Task.Delay(latency, token);
                    Log(Clock.GetInstance().EllapsedTimeInMs + " : " + this.CommandName + " Woke up from sleep!");
            }
        }
    }

    /**
     * A Command implementation that supports caching.
     */
    internal class SuccessfulCacheableCommand<T> : ServiceCommand<T>
    {

        private bool cacheEnabled;
        public volatile bool executed = false;
        private T value;

        public SuccessfulCacheableCommand(IJellyfishContext ctx, TestCircuitBreaker circuitBreaker, bool cacheEnabled, T value) 
            : base(ctx, "SuccessfulCacheableCommand", null, clock: circuitBreaker.Clock, metrics:circuitBreaker.Metrics, circuitBreaker:circuitBreaker, properties: CommandPropertiesTest.GetUnitTestPropertiesSetter().WithRequestCacheEnabled(true))
        {
            this.value = value;
            this.cacheEnabled = cacheEnabled;
        }

        protected override Task<T> Run(CancellationToken token)
        {
            executed = true;
            return Task.FromResult(value);
        }


        protected override string GetCacheKey()
        {
            if (cacheEnabled)
                return value.ToString();
            else
                return null;
        }
    }

    static class TestCommandFactory
    {
        public static int EXECUTE_VALUE = 1;
        public static int FALLBACK_VALUE = 11;

        public static TestServiceCommand Get(IJellyfishContext ctx, ITestOutputHelper output, ExecutionIsolationStrategy strategy, ExecutionResult executionResult=ExecutionResult.NONE, FallbackResult fallbackResult = FallbackResult.UNIMPLEMENTED, Action<CommandPropertiesBuilder> setter=null, TestCircuitBreaker circuitBreaker=null, IClock clock=null, [System.Runtime.CompilerServices.CallerMemberName] string commandName=null)
        {
            var builder = CommandPropertiesTest.GetUnitTestPropertiesSetter();
            if (setter != null)
                setter(builder);
            if (clock == null)
                clock = new MockedClock();

            var cmd = new TestServiceCommand(ctx, commandName, builder, strategy, clock, circuitBreaker ?? new TestCircuitBreaker(clock));
            cmd.Log = msg => output.WriteLine("({0}) - {1}", cmd.Id, msg);

            if (executionResult == ExecutionResult.SUCCESS)
            {
                cmd.Action = () =>
                {
                    cmd.Log("Execution success"); return Task.FromResult(TestCommandFactory.EXECUTE_VALUE);
                };
            }
            else if (executionResult == ExecutionResult.FAILURE)
            {
                
                cmd.Action = () => { cmd.Log("Execution failure"); ; throw new Exception( "Execution Failure for TestCommand"); };
            }
            else if (executionResult == ExecutionResult.HYSTRIX_FAILURE)
            {                
                cmd.Action = () =>
                {
                    cmd.Log("Execution hystrix failure");
                    throw new Exception("Execution  Failure for TestCommand",  new Exception("Fallback Failure for TestCommand"));
                };
            }
            else if (executionResult == ExecutionResult.RECOVERABLE_ERROR)
            {
                cmd.Action = () =>
                {
                    cmd.Log("Execution recoverable error");
                    throw new Exception("Execution ERROR for TestCommand");
                };
            }
            else if (executionResult == ExecutionResult.UNRECOVERABLE_ERROR)
            {
                cmd.Action = () =>
                {
                    cmd.Log("Execution unrecoverable error");
                    throw new StackOverflowException("Unrecoverable Error for TestCommand");
                };
            }
            else if (executionResult == ExecutionResult.BAD_REQUEST)
            {
                cmd.Action = () =>
                {
                    cmd.Log("Execution bad request");
                    throw new BadRequestException("Execution BadRequestException for TestCommand");
                };
            }

            if (fallbackResult == FallbackResult.SUCCESS)
            {
                cmd.Fallback = () =>
                {
                    cmd.Log("Execution fallback success");
                    return Task.FromResult(TestCommandFactory.FALLBACK_VALUE);
                };
            }
            else if (fallbackResult == FallbackResult.FAILURE)
            {              
                cmd.Fallback = () => { cmd.Log("Execution fallback error"); throw new Exception( "Fallback Failure for TestCommand"); };
            }
            else if (fallbackResult == FallbackResult.UNIMPLEMENTED)
            {
                cmd.Fallback = () =>
                {
                    cmd.Log("Execution fallback unimplemented");
                    throw new NotImplementedException();
                };
            }

            return cmd;
        }

    public enum ExecutionResult
    {
        NONE, SUCCESS, FAILURE, ASYNC_FAILURE, HYSTRIX_FAILURE, ASYNC_HYSTRIX_FAILURE, RECOVERABLE_ERROR, ASYNC_RECOVERABLE_ERROR, UNRECOVERABLE_ERROR, ASYNC_UNRECOVERABLE_ERROR, BAD_REQUEST, ASYNC_BAD_REQUEST, MULTIPLE_EMITS_THEN_SUCCESS, MULTIPLE_EMITS_THEN_FAILURE, NO_EMITS_THEN_SUCCESS
    }

    public enum FallbackResult
    {
        UNIMPLEMENTED, SUCCESS, FAILURE, ASYNC_FAILURE, MULTIPLE_EMITS_THEN_SUCCESS, MULTIPLE_EMITS_THEN_FAILURE, NO_EMITS_THEN_SUCCESS
    }

    public enum CacheEnabled
    {
        YES, NO
    }

    //static HystrixPropertiesStrategy TEST_PROPERTIES_FACTORY = new TestPropertiesFactory();

    //static class TestPropertiesFactory : HystrixPropertiesStrategy
    //{

    //    @Override
    //    public HystrixCommandProperties getCommandProperties(HystrixCommandKey commandKey, HystrixCommandProperties.Setter builder) {
    //        if (builder == null)
    //        {
    //            builder = HystrixCommandPropertiesTest.getUnitTestPropertiesSetter();
    //        }
    //        return HystrixCommandPropertiesTest.asMock(builder);
    //    }

    //    @Override
    //    public HystrixThreadPoolProperties getThreadPoolProperties(HystrixThreadPoolKey threadPoolKey, HystrixThreadPoolProperties.Setter builder) {
    //        if (builder == null)
    //        {
    //            builder = HystrixThreadPoolProperties.Setter.getUnitTestPropertiesBuilder();
    //        }
    //        return HystrixThreadPoolProperties.Setter.asMock(builder);
    //    }

    //    @Override
    //    public HystrixCollapserProperties getCollapserProperties(HystrixCollapserKey collapserKey, HystrixCollapserProperties.Setter builder) {
    //        throw new IllegalStateException("not expecting collapser properties");
    //    }

    //    @Override
    //    public String getCommandPropertiesCacheKey(HystrixCommandKey commandKey, HystrixCommandProperties.Setter builder) {
    //        return null;
    //    }

    //    @Override
    //    public String getThreadPoolPropertiesCacheKey(HystrixThreadPoolKey threadPoolKey, com.netflix.hystrix.HystrixThreadPoolProperties.Setter builder) {
    //        return null;
    //    }

    //    @Override
    //    public String getCollapserPropertiesCacheKey(HystrixCollapserKey collapserKey, com.netflix.hystrix.HystrixCollapserProperties.Setter builder) {
    //        return null;
    //    }

    }

}
