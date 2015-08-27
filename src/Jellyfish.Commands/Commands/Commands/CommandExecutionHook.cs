using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jellyfish.Commands
{
    /// <summary>
     /// Abstract ExecutionHook with invocations at different lifecycle points of <see cref="ServiceCommand"/>
     /// execution with default no-op implementations.
     /// <p>
     /// <b>Note on thread-safety and performance</b>
     /// <p>
     /// A single implementation of this class will be used globally so methods on this class will be invoked concurrently from multiple threads so all functionality must be thread-safe.
     /// 
     /// /// </summary>
    public abstract class CommandExecutionHook
    {
        /// <summary>
        /// Invoked before <see cref="ServiceCommand<T>"/> begins executing.
        /// </summary>
        /// <param name="commandInstance">The executing ServiceCommand<T> instance.</param>
        public virtual void OnStart<T>(ServiceCommand<T> commandInstance)
        {
            //do nothing by default
        }

        /// <summary>
        /// Invoked when <see cref="ServiceCommand<T>"/> emits a value.
        /// </summary>
        /// <param name="commandInstance">The executing ServiceCommand<T> instance.</param>
        /// <param name="value">value emitted</param>
        //public T OnEmit<T>(ServiceCommand<T> commandInstance, T value)
        //{
        //    return value; //by default, just pass through
        //}

        /// <summary>
        /// Invoked when <see cref="ServiceCommand<T>"/> fails with an Exception.
        /// </summary>
        /// <param name="commandInstance">The executing ServiceCommand<T> instance.</param>
        /// <param name="failureType"><see cref="FailureType"/> enum representing which type of error</param>
        /// <param name="e">exception object</param>
        public virtual Exception OnError<T>(ServiceCommand<T> commandInstance, FailureType failureType, Exception e)
        {
            return e; //by default, just pass through
        }

        /// <summary>
        /// Invoked when <see cref="ServiceCommand<T>"/> finishes a successful execution.
        /// </summary>
        /// <param name="commandInstance">The executing ServiceCommand<T> instance.</param>
        public virtual void OnSuccess<T>(ServiceCommand<T> commandInstance)
        {
            //do nothing by default
        }

        /// <summary>
        /// Invoked at start of thread execution when <see cref="HystrixCommand"/> is executed using {@link ExecutionIsolationStrategy#THREAD}.
        /// </summary>
        /// <param name="commandInstance">The executing HystrixCommand instance.</param>
        public virtual void OnThreadStart<T>(ServiceCommand<T> commandInstance)
        {
            //do nothing by default
        }

        /// <summary>
        /// Invoked at completion of thread execution when <see cref="HystrixCommand"/> is executed using {@link ExecutionIsolationStrategy#THREAD}.
        /// This will get invoked if the Hystrix thread successfully executes, regardless of whether the calling thread
        /// encountered a timeout.
        /// </summary>
        /// <param name="commandInstance">The executing HystrixCommand instance.</param>
        public virtual void OnThreadComplete<T>(ServiceCommand<T> commandInstance)
        {
            // do nothing by default
        }

        /// <summary>
        /// Invoked when the user-defined execution method in <see cref="ServiceCommand<T>"/> starts.
        /// </summary>
        /// <param name="commandInstance">The executing ServiceCommand<T> instance.</param>
        public virtual void OnExecutionStart<T>(ServiceCommand<T> commandInstance)
        {
            //do nothing by default
        }

        /// <summary>
        /// Invoked when the user-defined execution method in <see cref="ServiceCommand<T>"/> emits a value.
        /// </summary>
        /// <param name="commandInstance">The executing ServiceCommand<T> instance.</param>
        /// <param name="value">value emitted</param>
        //public T OnExecutionEmit<T>(ServiceCommand<T> commandInstance, T value)
        //{
        //    return value; //by default, just pass through
        //}

        /// <summary>
        /// Invoked when the user-defined execution method in <see cref="ServiceCommand<T>"/> fails with an Exception.
        /// </summary>
        /// <param name="commandInstance">The executing ServiceCommand<T> instance.</param>
        /// <param name="e">exception object</param>
        public virtual Exception OnExecutionError<T>(ServiceCommand<T> commandInstance, Exception e)
        {
            return e; //by default, just pass through
        }

        /// <summary>
        /// Invoked when the user-defined execution method in <see cref="ServiceCommand<T>"/> completes successfully.
        /// </summary>
        /// <param name="commandInstance">The executing ServiceCommand<T> instance.</param>
        public virtual void OnExecutionSuccess<T>(ServiceCommand<T> commandInstance)
        {
            //do nothing by default
        }

        /// <summary>
        /// Invoked when the fallback method in <see cref="ServiceCommand<T>"/> starts.
        /// </summary>
        /// <param name="commandInstance">The executing ServiceCommand<T> instance.</param>
        public virtual void OnFallbackStart<T>(ServiceCommand<T> commandInstance)
        {
            //do nothing by default
        }

        /// <summary>
        /// Invoked when the fallback method in <see cref="ServiceCommand<T>"/> emits a value.
        /// </summary>
        /// <param name="commandInstance">The executing ServiceCommand<T> instance.</param>
        /// <param name="value">value emitted</param>
        //public T OnFallbackEmit<T>(ServiceCommand<T> commandInstance, T value)
        //{
        //    return value; //by default, just pass through
        //}

        /// <summary>
        /// Invoked when the fallback method in <see cref="ServiceCommand<T>"/> fails with an Exception.
        /// </summary>
        /// <param name="commandInstance">The executing ServiceCommand<T> instance.</param>
        /// <param name="e">exception object</param>
        public virtual Exception OnFallbackError<T>(ServiceCommand<T> commandInstance, Exception e)
        {
            //by default, just pass through
            return e;
        }

        /// <summary>
        /// Invoked when the user-defined execution method in <see cref="ServiceCommand<T>"/> completes successfully.
        /// </summary>
        /// <param name="commandInstance">The executing ServiceCommand<T> instance.</param>
        public virtual void OnFallbackSuccess<T>(ServiceCommand<T> commandInstance)
        {
            //do nothing by default
        }

        /// <summary>
        /// Invoked when the command response is found in the <see cref="com.netflix.hystrix.HystrixRequestCache"/>.
        /// </summary>
        /// <param name="commandInstance">The executing HystrixCommand</param>
        public virtual void OnCacheHit<T>(ServiceCommand<T> commandInstance)
        {
            //do nothing by default
        }
    }

    internal class CommandExecutionHookDefault : CommandExecutionHook
    {
        public CommandExecutionHookDefault()
        {
        }
    }
}
