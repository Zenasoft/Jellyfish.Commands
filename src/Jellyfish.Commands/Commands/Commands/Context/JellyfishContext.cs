using System;
using System.Collections.Concurrent;
using Jellyfish.Commands.Metrics.Publishers;

namespace Jellyfish.Commands
{
    public class JellyfishContext : IJellyfishContext
    {
        private RequestLog _requestLog;
        private ConcurrentDictionary<string, IRequestCache> _cache = new ConcurrentDictionary<string, IRequestCache>();
        public IServiceProvider ServiceProvider { get; private set; }
        private ICommandExecutionHook _commandExecutionHook;
        private MetricsPublisherFactory _metricsPublisherFactory;

        public ICommandExecutionHook CommandExecutionHook
        {
            get{return _commandExecutionHook;}
        }

        public MetricsPublisherFactory MetricsPublisher
        {
            get{ return _metricsPublisherFactory;}
        }

        public JellyfishContext(IServiceProvider serviceProvider=null)
        {
            ServiceProvider = serviceProvider;
            _requestLog = new RequestLog();
            _metricsPublisherFactory = new MetricsPublisherFactory(this);
            _commandExecutionHook = GetService<ICommandExecutionHook>() ??  new CommandExecutionHookDefault();
        }

        public RequestLog GetRequestLog()
        {
            return _requestLog;
        }

        public RequestCache<T> GetCache<T>(string commandName)
        {
            return (RequestCache<T>)_cache.GetOrAdd(commandName, new RequestCache<T>());
        }

        public T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        public object GetService(Type serviceType)
        {
            if (ServiceProvider == null) return null;
            return ServiceProvider.GetService(serviceType);
        }
    }
}
