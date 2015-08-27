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
        /// Invoked before <see cref="ServiceCommandInfo"/> begins executing.
        /// </summary>
        /// <param name="commandInstance">The executing ServiceCommandInfo instance.</param>
        public void OnStart<T>(ServiceCommandInfo commandInstance)
        {
            //do nothing by default
        }

        /// <summary>
        /// Invoked when <see cref="ServiceCommandInfo"/> emits a value.
        /// </summary>
        /// <param name="commandInstance">The executing ServiceCommandInfo instance.</param>
        /// <param name="value">value emitted</param>
        public T OnEmit<T>(ServiceCommandInfo commandInstance, T value)
        {
            return value; //by default, just pass through
        }

        /// <summary>
        /// Invoked when <see cref="ServiceCommandInfo"/> fails with an Exception.
        /// </summary>
        /// <param name="commandInstance">The executing ServiceCommandInfo instance.</param>
        /// <param name="failureType"><see cref="FailureType"/> enum representing which type of error</param>
        /// <param name="e">exception object</param>
        public Exception OnError<T>(ServiceCommandInfo commandInstance, FailureType failureType, Exception e)
        {
            return e; //by default, just pass through
        }

        /// <summary>
        /// Invoked when <see cref="ServiceCommandInfo"/> finishes a successful execution.
        /// </summary>
        /// <param name="commandInstance">The executing ServiceCommandInfo instance.</param>
        public void OnSuccess<T>(ServiceCommandInfo commandInstance)
        {
            //do nothing by default
        }

        /// <summary>
        /// Invoked at start of thread execution when <see cref="HystrixCommand"/> is executed using {@link ExecutionIsolationStrategy#THREAD}.
        /// </summary>
        /// <param name="commandInstance">The executing HystrixCommand instance.</param>
        public void OnThreadStart<T>(ServiceCommandInfo commandInstance)
        {
            //do nothing by default
        }

        /// <summary>
        /// Invoked at completion of thread execution when <see cref="HystrixCommand"/> is executed using {@link ExecutionIsolationStrategy#THREAD}.
        /// This will get invoked if the Hystrix thread successfully executes, regardless of whether the calling thread
        /// encountered a timeout.
        /// </summary>
        /// <param name="commandInstance">The executing HystrixCommand instance.</param>
        public void OnThreadComplete<T>(ServiceCommandInfo commandInstance)
        {
            // do nothing by default
        }

        /// <summary>
        /// Invoked when the user-defined execution method in <see cref="ServiceCommandInfo"/> starts.
        /// </summary>
        /// <param name="commandInstance">The executing ServiceCommandInfo instance.</param>
        public void OnExecutionStart<T>(ServiceCommandInfo commandInstance)
        {
            //do nothing by default
        }

        /// <summary>
        /// Invoked when the user-defined execution method in <see cref="ServiceCommandInfo"/> emits a value.
        /// </summary>
        /// <param name="commandInstance">The executing ServiceCommandInfo instance.</param>
        /// <param name="value">value emitted</param>
        public T OnExecutionEmit<T>(ServiceCommandInfo commandInstance, T value)
        {
            return value; //by default, just pass through
        }

        /// <summary>
        /// Invoked when the user-defined execution method in <see cref="ServiceCommandInfo"/> fails with an Exception.
        /// </summary>
        /// <param name="commandInstance">The executing ServiceCommandInfo instance.</param>
        /// <param name="e">exception object</param>
        public Exception OnExecutionError<T>(ServiceCommandInfo commandInstance, Exception e)
        {
            return e; //by default, just pass through
        }

        /// <summary>
        /// Invoked when the user-defined execution method in <see cref="ServiceCommandInfo"/> completes successfully.
        /// </summary>
        /// <param name="commandInstance">The executing ServiceCommandInfo instance.</param>
        public void OnExecutionSuccess<T>(ServiceCommandInfo commandInstance)
        {
            //do nothing by default
        }

        /// <summary>
        /// Invoked when the fallback method in <see cref="ServiceCommandInfo"/> starts.
        /// </summary>
        /// <param name="commandInstance">The executing ServiceCommandInfo instance.</param>
        public void OnFallbackStart<T>(ServiceCommandInfo commandInstance)
        {
            //do nothing by default
        }

        /// <summary>
        /// Invoked when the fallback method in <see cref="ServiceCommandInfo"/> emits a value.
        /// </summary>
        /// <param name="commandInstance">The executing ServiceCommandInfo instance.</param>
        /// <param name="value">value emitted</param>
        public T OnFallbackEmit<T>(ServiceCommandInfo commandInstance, T value)
        {
            return value; //by default, just pass through
        }

        /// <summary>
        /// Invoked when the fallback method in <see cref="ServiceCommandInfo"/> fails with an Exception.
        /// </summary>
        /// <param name="commandInstance">The executing ServiceCommandInfo instance.</param>
        /// <param name="e">exception object</param>
        public Exception OnFallbackError<T>(ServiceCommandInfo commandInstance, Exception e)
        {
            //by default, just pass through
            return e;
        }

        /// <summary>
        /// Invoked when the user-defined execution method in <see cref="ServiceCommandInfo"/> completes successfully.
        /// </summary>
        /// <param name="commandInstance">The executing ServiceCommandInfo instance.</param>
        public void OnFallbackSuccess<T>(ServiceCommandInfo commandInstance)
        {
            //do nothing by default
        }

        /// <summary>
        /// Invoked when the command response is found in the {@link com.netflix.hystrix.HystrixRequestCache}.
        /// </summary>
        /// <param name="commandInstance">The executing HystrixCommand</param>
        public void OnCacheHit<T>(ServiceCommandInfo commandInstance)
        {
            //do nothing by default
        }
    }
}
