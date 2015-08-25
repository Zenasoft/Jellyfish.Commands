using Jellyfish.Commands.Metrics;
using System;
using System.Collections.Generic;

namespace Jellyfish.Commands
{
    public interface ServiceCommandInfo
    {
         string CommandGroup { get;}

         string CommandName { get;}

         string ThreadPoolKey { get;}

         CommandMetrics Metrics { get;}

         CommandProperties Properties { get;}

         bool IsCircuitBreakerOpen {get;}

         bool IsExecutionComplete {get;}

         bool IsExecutedInThread {get;}

         bool IsSuccessfulExecution {get;}

         bool IsFailedExecution {get;}

         Exception FailedExecutionException { get;}

         bool IsResponseFromFallback {get;}

         bool IsResponseTimedOut {get;}

         bool IsResponseShortCircuited {get;}

         bool IsResponseFromCache {get;}

         bool IsResponseRejected {get;}

         List<EventType> ExecutionEvents { get;}

         int ExecutionTimeInMilliseconds { get;}

         long CommandRunStartTimeInMs { get;}

    }
}
