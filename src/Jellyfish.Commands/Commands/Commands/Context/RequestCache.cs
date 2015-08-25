using System;
using System.Collections.Concurrent;

namespace Jellyfish.Commands
{
    public class RequestCache<T> : IRequestCache
    {
        private static ConcurrentDictionary<string, T> _requestCache = new ConcurrentDictionary<string, T>();

        internal bool TryGetValue(string key, out T result)
        {
            return _requestCache.TryGetValue(key, out result);
        }

        internal bool TryAdd(string key, T result)
        {
            return _requestCache.TryAdd(key, result);
        }
    }
}