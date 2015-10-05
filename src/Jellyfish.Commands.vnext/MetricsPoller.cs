using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Jellyfish.Commands
{
    class MetricsPoller
    {
        private int delay;
        private CancellationToken token;
        private ConcurrentQueue<string> events = new ConcurrentQueue<string>();
        public MetricsPoller(int delay, CancellationToken token)
        {
            this.delay = delay;
            this.token = token;
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    foreach (var entry in Jellyfish.Commands.Metrics.CommandMetricsFactory.GetInstances())
                    {
                        try
                        {
                            var publisher = new Jellyfish.Commands.Metrics.Publishers.JsonMetricsPublisherCommand(entry);
                            publisher.Run(UpdateEvent);
                        }
                        catch
                        {
                        }
                    }

                    await Task.Delay(delay);
                }
            }

            );
        }

        private void UpdateEvent(string evt)
        {
            events.Enqueue(evt);
        }

        internal IEnumerable<string> GetJsonMetrics()
        {
            var tmp = Interlocked.Exchange(ref events, new ConcurrentQueue<string>());
            return tmp.ToList();
        }
    }
}