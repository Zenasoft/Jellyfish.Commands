using System;
using Microsoft.Extensions.Logging;

namespace Jellyfish.Commands
{
    internal class NullDisposable : IDisposable
    {
        public static IDisposable Instance = new NullDisposable();

        private NullDisposable()
        {
        }

        public void Dispose()
        {
        }
    }

    internal class EmptyLogger : ILogger
    {
        public static ILogger Instance = new EmptyLogger();

        private EmptyLogger()
        {
        }

        public IDisposable BeginScopeImpl(object state)
        {
            return NullDisposable.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return false;
        }

        public void Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
        }
    }
}