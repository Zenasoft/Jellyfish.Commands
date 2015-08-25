using System.Collections.Concurrent;

namespace Jellyfish.Commands
{
    public class MockJellyfishContext : IJellyfishContext
    {
        private RequestLog _requestLog;
        private ConcurrentDictionary<string, IRequestCache> _cache = new ConcurrentDictionary<string, IRequestCache>();

        public MockJellyfishContext()
        {
            _requestLog = new RequestLog();
        }

        public RequestLog GetRequestLog()
        {
            return _requestLog;
        }

        public RequestCache<T> GetCache<T>(string commandName)
        {
            return (RequestCache<T>)_cache.GetOrAdd(commandName, new RequestCache<T>());
        }
    }
}
