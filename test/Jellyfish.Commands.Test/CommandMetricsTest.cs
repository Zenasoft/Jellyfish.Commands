using Jellyfish.Commands.Metrics;
using System.Threading.Tasks;
using Xunit;
using System.Threading;
using Jellyfish.Commands.Utils;

namespace Jellyfish.Commands.Tests
{
    public class CommandMetricsTest
    {

        /**
         * Testing the ErrorPercentage because this method could be easy to miss when making changes elsewhere.
         */
        [Fact]
        public void testErrorPercentage()
        {
            var properties = CommandPropertiesTest.GetUnitTestPropertiesSetter();
            var clock = new MockedClock();
            CommandMetrics metrics = getMetrics(properties, clock);

            metrics.MarkSuccess(100);
            Assert.Equal(0, metrics.GetHealthCounts().ErrorPercentage);
            clock.Increment(1);
            metrics.MarkFailure(1000);
            Assert.Equal(50, metrics.GetHealthCounts().ErrorPercentage);

            clock.Increment(1);
            metrics.MarkSuccess(100);
            metrics.MarkSuccess(100);
            Assert.Equal(25, metrics.GetHealthCounts().ErrorPercentage);

            clock.Increment(1);
            metrics.MarkTimeout(5000);
            metrics.MarkTimeout(5000);
            Assert.Equal(50, metrics.GetHealthCounts().ErrorPercentage);

            clock.Increment(1);
            metrics.MarkSuccess(100);
            metrics.MarkSuccess(100);
            metrics.MarkSuccess(100);

            // latent
            clock.Increment(1);
            metrics.MarkSuccess(5000);

            // 6 success + 1 latent success + 1 failure + 2 timeout = 10 total
            // latent success not considered error
            // error percentage = 1 failure + 2 timeout / 10
            Assert.Equal(30, metrics.GetHealthCounts().ErrorPercentage);
        }

        [Fact]
        public void testBadRequestsDoNotAffectErrorPercentage()
        {
            var properties = CommandPropertiesTest.GetUnitTestPropertiesSetter();
            var clock = new MockedClock();
            CommandMetrics metrics = getMetrics(properties, clock); ;

            metrics.MarkSuccess(100);
            Assert.Equal(0, metrics.GetHealthCounts().ErrorPercentage);

            metrics.MarkFailure(1000);
            Assert.Equal(50, metrics.GetHealthCounts().ErrorPercentage);

            metrics.MarkBadRequest(1);
            metrics.MarkBadRequest(2);
            Assert.Equal(50, metrics.GetHealthCounts().ErrorPercentage);

            metrics.MarkFailure(45);
            metrics.MarkFailure(55);
            Assert.Equal(75, metrics.GetHealthCounts().ErrorPercentage);
        }

        class LatentCommand : ServiceCommand<bool>
        {
            int duration;

            public LatentCommand(IJellyfishContext ctx,  int duration)
            : base(ctx, "Latent", null, null, ExecutionIsolationStrategy.Thread, new CommandPropertiesBuilder().WithExecutionTimeoutInMilliseconds(1000))
            {
                this.duration = duration;
            }

            protected override Task<bool> GetFallback()
            {
                return Task.FromResult( false );
            }

            protected override async Task<bool> Run(CancellationToken token)
            {
                await Task.Delay(duration, token);
                return true;
            }
        }

        [Fact]
        public void testCurrentConcurrentExecutionCount()
        {
            CommandMetrics metrics = null;
            var ctx = new JellyfishContext();

            int NUM_CMDS = 10;
            Task[] tasks = new Task[NUM_CMDS];
            for (int i = 0; i < NUM_CMDS; i++)
            {
                LatentCommand cmd = new LatentCommand(ctx, 400);
                if (metrics == null)
                {
                    metrics = cmd.Metrics;
                }
                tasks[i] = cmd.ExecuteAsync();
            }
            Task.WaitAll(tasks);

            Assert.Equal(NUM_CMDS, metrics.GetRollingMaxConcurrentExecutions());
        }

        /**
         * Utility method for creating {@link CommandMetrics} for unit tests.
         */
        private static CommandMetrics getMetrics(CommandPropertiesBuilder properties, IClock clock)
        {
            return new CommandMetrics("KEY_ONE", properties.Build("KEY_ONE"), clock);
        }

    }
}
