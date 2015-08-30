using Jellyfish.Commands.CircuitBreaker;
using Jellyfish.Commands.Metrics;
using Jellyfish.Commands.Metrics.Publishers;
using System;

namespace Jellyfish.Commands
{

    public interface IJellyfishContext : IServiceProvider
    {
        ICommandExecutionHook CommandExecutionHook { get; }
        MetricsPublisherFactory MetricsPublisher { get; }
        void Reset();
        RequestCache<T> GetCache<T>(string commandName);
        RequestLog GetRequestLog();
        T GetService<T>();
    }
}