using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using System.Threading;
using System.Collections.Concurrent;
using Jellyfish.Configuration;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Sample.HttpCommand.Controllers
{
    [Route("jellyfish.stream")]
    public class EventStream : Controller
    {
        private int _nbConnections;
        private IDynamicProperty<int> maxConcurrentConnection = DynamicProperties.Instance.GetOrDefaultProperty("jellyfish.stream.maxConcurrentConnections", 5);

        // GET: api/values
        [HttpGet]
        public async Task<string> Get(int? delay)
        {
            var nb = Interlocked.Increment(ref _nbConnections);
            CancellationTokenSource token = null;
            try {
                if( nb > maxConcurrentConnection.Get() )
                {
                    return "Mac concurrent connections reached.";
                }
                token = new CancellationTokenSource();
                if (!delay.HasValue) delay = 1000;
                var poller = new MetricsPoller(delay.Value, token.Token);
                poller.Start();

                Response.ContentType = "text /event-stream; charset=UTF-8";
                Response.Headers.Add("Cache-Control", new string[] { "no-cache, no-store, max-age=0, must-revalidate" });
                Response.Headers.Add("Pragma", new string[] { "no-cache" });

                while (!Context.RequestAborted.IsCancellationRequested) {
                    var events = poller.GetJsonMetrics();
                    var writer = new System.IO.StreamWriter(Response.Body);

                    if (events.Count() == 0)
                    {
                        await writer.WriteLineAsync("ping: ");
                    }
                    else
                    {
                        foreach (var json in events)
                        {
                            await writer.WriteLineAsync("data: " + json);
                        }
                    }

                    writer.Flush();
                    await Response.Body.FlushAsync(token.Token);

                    await Task.Delay(delay.Value);
                }

                if(token!=null)
                    token.Cancel();

                return "end";
            }
            finally
            {
                Interlocked.Decrement(ref _nbConnections);
            }
        }         
    }

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
                    foreach (var entry in Jellyfish.Commands.Metrics.CommandMetrics.GetInstances())
                    {
                        try
                        {
                            var publisher = new Jellyfish.Commands.Metrics.Publishers.JsonMetricsPublisherCommand(entry.Metrics);
                            publisher.Run(UpdateEvent);
                        }
                        catch { }
                    }

                    await Task.Delay(delay);
                }
            });
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
