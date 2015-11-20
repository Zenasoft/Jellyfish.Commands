using Jellyfish.Commands.CircuitBreaker;
using Jellyfish.Commands.Metrics;
using Jellyfish.Commands.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jellyfish.Commands
{
    internal class CommandState
    {
        public ServiceCommandOptions Flags;
        public string CommandName;
        public string CommandGroup;
    }

    [Flags]
    internal enum ServiceCommandOptions
    {
        None = 0,
        ThreadExecutionStrategy = 1,
        SemaphoreExecutionStrategy = 2,
        HasFallBack = 4,
        HasCacheKey = 8
    }

    internal class ServiceCommandHelper
    {
        internal static CommandState PrepareInternal(Type commandType, IJellyfishContext context, string commandGroup, string commandName, CommandPropertiesBuilder propertiesBuilder=null, IClock clock = null, CommandMetrics metrics = null, ICircuitBreaker circuitBreaker = null)
        {
            var state = new CommandState();

            state.CommandName = commandName ?? commandType.FullName;
            state.CommandGroup = commandGroup ?? state.CommandName;

            clock = clock ?? Clock.GetInstance();
            var properties = propertiesBuilder?.Build( state.CommandName ) ?? new CommandProperties( state.CommandName );
            metrics = metrics ?? CommandMetricsFactory.GetInstance( state.CommandName, state.CommandGroup, properties, clock );
            circuitBreaker = circuitBreaker ?? ( properties.CircuitBreakerEnabled.Value ? CircuitBreakerFactory.GetOrCreateInstance( state.CommandName, properties, metrics, clock ) : new NoOpCircuitBreaker() );
            context.MetricsPublisher.CreateOrRetrievePublisherForCommand( state.CommandGroup, metrics, circuitBreaker );

            ServiceCommandOptions flags = ServiceCommandOptions.None;
            if(IsMethodImplemented( commandType, "GetFallback" ))
                flags |= ServiceCommandOptions.HasFallBack;
            if(IsMethodImplemented( commandType, "GetCacheKey" ))
                flags |= ServiceCommandOptions.HasCacheKey;
            state.Flags = flags;
            return state;
        }

        private static bool IsMethodImplemented(Type commandType, string methodName)
        {
            var m = ( commandType
                        .GetMethod( methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance )
                        .DeclaringType );
            return !m.IsGenericType || m.GetGenericTypeDefinition() != typeof( ServiceCommand<> );
        }
    }
}
