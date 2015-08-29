using System;

namespace Jellyfish.Commands
{
    public interface ICommandExecutionHook
    {
        void OnCacheHit<T>(ServiceCommand<T> commandInstance);
        Exception OnError<T>(ServiceCommand<T> commandInstance, FailureType failureType, Exception e);
        Exception OnExecutionError<T>(ServiceCommand<T> commandInstance, Exception e);
        void OnExecutionStart<T>(ServiceCommand<T> commandInstance);
        void OnExecutionSuccess<T>(ServiceCommand<T> commandInstance);
        Exception OnFallbackError<T>(ServiceCommand<T> commandInstance, Exception e);
        void OnFallbackStart<T>(ServiceCommand<T> commandInstance);
        void OnFallbackSuccess<T>(ServiceCommand<T> commandInstance);
        void OnStart<T>(ServiceCommand<T> commandInstance);
        void OnSuccess<T>(ServiceCommand<T> commandInstance);
        void OnThreadComplete<T>(ServiceCommand<T> commandInstance);
        void OnThreadStart<T>(ServiceCommand<T> commandInstance);
    }
}