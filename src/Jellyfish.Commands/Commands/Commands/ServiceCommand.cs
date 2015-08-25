// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfish.Commands.CircuitBreaker;
using Jellyfish.Commands.Metrics;
using Jellyfish.Commands.Utils;
using System.Diagnostics.Contracts;

namespace Jellyfish.Commands
{
    public enum ExecutionIsolationStrategy
    {
        Thread,
        Semaphore,
    }

    public abstract class ServiceCommand<T> : ServiceCommandInfo
    {
        [Flags]
        protected enum ServiceCommandOptions
        {
            ThreadExecutionStrategy = 0,
            SemaphoreExecutionStrategy = 1,
            HasFallBack = 2,
            HasCacheKey = 4
        }

        private RequestCache<T> _requestCache;
        private RequestLog _currentRequestLog;
        private ServiceCommandOptions _flags;
        private ICircuitBreaker _circuitBreaker;
        private static ConcurrentDictionary<string, ServiceCommandOptions> _states = new ConcurrentDictionary<string, ServiceCommandOptions>();
        private static ConcurrentDictionary<string, ITryableSemaphore> _fallbackSemaphorePerCircuit = new ConcurrentDictionary<string, ITryableSemaphore>();
        private static ConcurrentDictionary<string, ITryableSemaphore> _executionSemaphorePerCircuit = new ConcurrentDictionary<string, ITryableSemaphore>();
        private ITryableSemaphore fallbackSemaphoreOverride;
        private ITryableSemaphore executionSemaphoreOverride;
        private IClock _clock;
        private Func<T> _execHandler;
        private TaskScheduler _taskscheduler;
        private string _threadPoolKey;
        private ExecutionResult _executionResult;
        private int _started;
        private bool _isExecutedInThread;
        private bool _isExecutionComplete;

        public CommandMetrics Metrics { get; private set; }

        public CommandProperties Properties { get; private set; }

        public string CommandName { get; private set; }

        public string CommandGroup { get; private set; }

        /// <summary>
        /// Get the TryableSemaphore this Command should use for execution if not running in a separate thread.
        /// </summary>
        public ITryableSemaphore ExecutionSemaphore
        {
            get
            {
                if (executionSemaphoreOverride == null)
                {
                    return _executionSemaphorePerCircuit.GetOrAdd(CommandName, new TryableSemaphoreActual(Properties.ExecutionIsolationSemaphoreMaxConcurrentRequests));
                }
                else
                {
                    return executionSemaphoreOverride;
                }
            }
            internal set
            {
                executionSemaphoreOverride = value;
            }
        }


        /// <summary>
        /// Get the TryableSemaphore this Command should use if a fallback occurs.
        /// </summary>
        public ITryableSemaphore FallBackSemaphore
        {
            get
            {
                if (fallbackSemaphoreOverride == null)
                {
                    return _fallbackSemaphorePerCircuit.GetOrAdd(CommandName, new TryableSemaphoreActual(Properties.FallbackIsolationSemaphoreMaxConcurrentRequests));
                }
                else
                {
                    return fallbackSemaphoreOverride;
                }
            }
        }

        public TaskScheduler TaskScheduler
        {
            get
            {
                if (_taskscheduler == null)
                    _taskscheduler = TaskSchedulerFactory.CreateOrRetrieve(Properties.ExecutionIsolationSemaphoreMaxConcurrentRequests.Get(), _threadPoolKey);
                return _taskscheduler;
            }
            set
            {
                _taskscheduler = value;
            }
        }


        protected ServiceCommand(IJellyfishContext context, string commandGroup, string threadPoolKey = null, string commandName = null, ExecutionIsolationStrategy executionPolicy = ExecutionIsolationStrategy.Thread, CommandPropertiesBuilder properties = null)
            : this( context, commandGroup, commandName, threadPoolKey, executionPolicy, properties, null)
        {
        }

        internal ServiceCommand(IJellyfishContext context, string commandGroup, string commandName, string threadPoolKey = null, ExecutionIsolationStrategy executionPolicy = ExecutionIsolationStrategy.Thread, CommandPropertiesBuilder properties = null, IClock clock = null, ICircuitBreaker circuitBreaker = null, CommandMetrics metrics = null)
        {
            Contract.Requires(context != null);
            if (String.IsNullOrEmpty(commandGroup))
                throw new ArgumentException("commandGroup can not be null or empty.");

            _clock = clock ?? Clock.GetInstance(); // for test
            CommandGroup = commandGroup;
            CommandName = commandName ?? this.GetType().FullName;
            _threadPoolKey = threadPoolKey ?? CommandGroup;
            _executionResult = new ExecutionResult();

            this._flags = _states.GetOrAdd(CommandName, (n) =>
            {
                ServiceCommandOptions flags = ServiceCommandOptions.ThreadExecutionStrategy;
                if (this.IsMethodImplemented("GetFallback"))
                    flags |= ServiceCommandOptions.HasFallBack;
                if (executionPolicy == ExecutionIsolationStrategy.Semaphore)
                    flags |= ServiceCommandOptions.SemaphoreExecutionStrategy;
                if (this.IsMethodImplemented("GetCacheKey"))
                    flags |= ServiceCommandOptions.HasCacheKey;
                return flags;
            });

            Properties = properties != null ? properties.Build(CommandName) : new CommandProperties(CommandName);

            Metrics = metrics ?? CommandMetrics.GetInstance(CommandName, Properties, _clock);

            _circuitBreaker = circuitBreaker ?? (Properties.CircuitBreakerEnabled.Get() ? CircuitBreakerFactory.GetOrCreateInstance(CommandName, Properties, Metrics, _clock) : new NoOpCircuitBreaker());

            Jellyfish.Commands.Metrics.Publishers.MetricsPublisherFactory.CreateOrRetrievePublisherForCommand(CommandGroup, Metrics, _circuitBreaker, Properties);

            if (Properties.RequestLogEnabled.Get())
            {
                _currentRequestLog = context.GetRequestLog();
            }

            if ((_flags & ServiceCommandOptions.HasCacheKey) == ServiceCommandOptions.HasFallBack && Properties.RequestCacheEnabled.Get())
            {
                _requestCache = context.GetCache<T>(CommandName);
            }
        }

        #region ServiceCommandInfo

        public string ThreadPoolKey
        {
            get
            {
                return _threadPoolKey;
            }
        }

        /**
 * Whether the 'circuit-breaker' is open meaning that <code>execute()</code> will immediately return
 * the <code>getFallback()</code> response and not attempt a Command execution.
 *
 * 4 columns are ForcedOpen | ForcedClosed | CircuitBreaker open due to health ||| Expected Result
 *
 * T | T | T ||| OPEN (true)
 * T | T | F ||| OPEN (true)
 * T | F | T ||| OPEN (true)
 * T | F | F ||| OPEN (true)
 * F | T | T ||| CLOSED (false)
 * F | T | F ||| CLOSED (false)
 * F | F | T ||| OPEN (true)
 * F | F | F ||| CLOSED (false)
 *
 */
        public bool IsCircuitBreakerOpen
        {
            get
            {
                return Properties.CircuitBreakerForceOpen.Get() || (!Properties.CircuitBreakerForceClosed.Get() && _circuitBreaker.IsOpen());
            }
        }

        /**
         * If this command has completed execution either successfully, via fallback or failure.
         * 
         * @return bool
         */
        public bool IsExecutionComplete
        {
            get
            {
                return _isExecutionComplete;
            }
        }

        /**
         * Whether the execution occurred in a separate thread.
         * <p>
         * This should be called only once execute()/queue()/fireOrForget() are called otherwise it will always return false.
         * <p>
         * This specifies if a thread execution actually occurred, not just if it is configured to be executed in a thread.
         * 
         * @return bool
         */
        public bool IsExecutedInThread
        {
            get
            {
                return _isExecutedInThread;
            }
        }

        /**
         * Whether the response was returned successfully either by executing <code>run()</code> or from cache.
         * 
         * @return bool
         */
        public bool IsSuccessfulExecution
        {
            get
            {
                return _executionResult.EventExists(EventType.SUCCESS);
            }
        }

        /**
         * Whether the <code>run()</code> resulted in a failure (exception).
         * 
         * @return bool
         */
        public bool IsFailedExecution
        {
            get
            {
                return _executionResult.EventExists(EventType.FAILURE);
            }
        }

        /**
         * Get the Throwable/Exception thrown that caused the failure.
         * <p>
         * If <code>IsFailedExecution { get == true</code> then this would represent the Exception thrown by the <code>run()</code> method.
         * <p>
         * If <code>IsFailedExecution { get == false</code> then this would return null.
         * 
         * @return Throwable or null
         */
        public Exception FailedExecutionException
        {
            get
            {
                return _executionResult.Exception;
            }
        }

        /**
         * Whether the response received from was the result of some type of failure
         * and <code>Fallback { get</code> being called.
         * 
         * @return bool
         */
        public bool IsResponseFromFallback
        {
            get
            {
                return _executionResult.EventExists(EventType.FALLBACK_SUCCESS);
            }
        }

        /**
         * Whether the response received was the result of a timeout
         * and <code>Fallback { get</code> being called.
         * 
         * @return bool
         */
        public bool IsResponseTimedOut
        {
            get
            {
                return _executionResult.EventExists(EventType.TIMEOUT);
            }
        }

        /**
         * Whether the response received was a fallback as result of being
         * short-circuited (meaning <code>IsCircuitBreakerOpen { get == true</code>) and <code>Fallback { get</code> being called.
         * 
         * @return bool
         */
        public bool IsResponseShortCircuited
        {
            get
            {
                return _executionResult.EventExists(EventType.SHORT_CIRCUITED);
            }
        }

        /**
         * Whether the response is from cache and <code>run()</code> was not invoked.
         * 
         * @return bool
         */
        public bool IsResponseFromCache
        {
            get
            {
                return _executionResult.EventExists(EventType.RESPONSE_FROM_CACHE);
            }
        }

        /**
         * Whether the response received was a fallback as result of being
         * rejected (from thread-pool or semaphore) and <code>Fallback { get</code> being called.
         * 
         * @return bool
         */
        public bool IsResponseRejected
        {
            get
            {
                return _executionResult.EventExists(EventType.THREAD_POOL_REJECTED) || _executionResult.EventExists(EventType.SEMAPHORE_REJECTED);
            }
        }

        /**
         * List of CommandEventType enums representing events that occurred during execution.
         * <p>
         * Examples of events are SUCCESS, FAILURE, TIMEOUT, and SHORT_CIRCUITED
         * 
         * @return {@code List<EventType>}
         */
        public List<EventType> ExecutionEvents
        {
            get
            {
                return _executionResult.events;
            }
        }

        /**
         * The execution time of this command instance in milliseconds, or -1 if not executed.
         * 
         * @return int
         */
        public int ExecutionTimeInMilliseconds
        {
            get
            {
                return _executionResult.ExecutionTime;
            }
        }

        /**
         * Time in Nanos when this command instance's run method was called, or -1 if not executed 
         * for e.g., command threw an exception
          *
          * @return long
         */
        public long CommandRunStartTimeInMs
        {
            get
            {
                return _executionResult.CommandRunStartTimeInMs;
            }
        }

        private static Exception TimeoutException = new TimeoutException();
        #endregion


        protected void SetFlag(ServiceCommandOptions option, bool set)
        {
            if(set)
                _flags |= option;
            else
                _flags &= ~option;
        }

        private bool IsMethodImplemented(string methodName)
        {
            return (this.GetType()
                        .GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .DeclaringType != typeof(ServiceCommand<T>));
        }

        public async Task<T> ExecuteAsync()
        {
            /* this is a stateful object so can only be used once */
            if (Interlocked.CompareExchange(ref _started, 1, 0) == 1)
            {
                throw new IllegalStateException("This instance can only be executed once. Please instantiate a new instance.");
            }

            T result;
            // get from cache
            if ( _requestCache != null)
            {
                var key = GetCacheKey();
                if (key != null && _requestCache.TryGetValue(key, out result))
                {
                    Metrics.MarkResponseFromCache();
                    _executionResult.AddEvent(EventType.RESPONSE_FROM_CACHE);
                    return result;
                }
            }

            long ms;
            var start = _clock.EllapsedTimeInMs;
            try
            {
                RecordExecutedCommand();
                Metrics.IncrementConcurrentExecutionCount();

                if (_circuitBreaker.AllowRequest) // short circuit closed = OK
                {
                    if (ExecutionSemaphore.TryAcquire()) // semaphore
                    {
                        try
                        {
                            // if bulkhead by thread                            
                            var token = Properties.ExecutionTimeoutEnabled.Get() 
                                ? new CancellationTokenSource(Properties.ExecutionTimeoutInMilliseconds.Get()) 
                                : new CancellationTokenSource();
                            try
                            {
                                if ((_flags & ServiceCommandOptions.SemaphoreExecutionStrategy) != ServiceCommandOptions.SemaphoreExecutionStrategy)
                                {
                                    _isExecutedInThread = true;

                                    // Run with bulkhead and timeout
                                    var t = await Task.Factory.StartNew(
                                        () => Run(token.Token),
                                        token.Token,
                                        TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler,
                                        TaskScheduler
                                    )
                                    .ConfigureAwait(false);
                                    result = await t.ConfigureAwait(false);
                                    if (token.IsCancellationRequested)
                                    {
                                        throw new OperationCanceledException();
                                    }
                                }
                                else
                                {
                                    // Simple semaphore
                                    result = await Run(token.Token).ConfigureAwait(false);
                                }
                            }
                            catch (AggregateException ae)
                            {
                                ms = _clock.EllapsedTimeInMs - start;
                                var e = ((AggregateException)ae).InnerException;
                                return await OnExecutionError(ms, e);
                            }
                            catch (TaskSchedulerException tex)
                            {
                                start = 0; // We don't want register execution time.
                                Metrics.MarkThreadPoolRejection();
                                return await GetFallbackOrThrowException(EventType.THREAD_POOL_REJECTED, FailureType.REJECTED_THREAD_EXECUTION, "Thread pool is full", tex.InnerException);
                            }
                            catch (OperationCanceledException)
                            {
                                // timeout
                                ms = _clock.EllapsedTimeInMs - start;
                                Metrics.MarkTimeout(ms);
                                return await GetFallbackOrThrowException(EventType.TIMEOUT, FailureType.TIMEOUT, "timed-out", ServiceCommand<T>.TimeoutException);
                            }
                            catch (Exception e)
                            { 
                                ms = _clock.EllapsedTimeInMs - start;
                                return await OnExecutionError(ms, e);
                            }

                            ms = _clock.EllapsedTimeInMs - start;
                            Metrics.AddCommandExecutionTime(ms);
                            Metrics.MarkSuccess(ms);
                            _circuitBreaker.MarkSuccess();
                            _executionResult.AddEvent(EventType.SUCCESS);

                            if (_requestCache != null)
                            {
                                var key = GetCacheKey();
                                if (key != null && !_requestCache.TryAdd(key, result))
                                {
                                    _requestCache.TryGetValue(key, out result);
                                }
                            }
                            return result;
                        }
                        finally
                        {
                            ExecutionSemaphore.Release();
                        }
                    }
                    else
                    {
                        Metrics.MarkSemaphoreRejection();
                        return await GetFallbackOrThrowException(EventType.SEMAPHORE_REJECTED, FailureType.REJECTED_SEMAPHORE_EXECUTION,
                                "could not acquire a semaphore for execution", new Exception("could not acquire a semaphore for execution"));
                    }
                }
                else
                {
                    start = 0;
                    Metrics.MarkShortCircuited();
                    return await GetFallbackOrThrowException(EventType.SHORT_CIRCUITED, FailureType.SHORTCIRCUIT,
                                "short-circuited", new Exception(" circuit short-circuited and is OPEN"));
                }

            }
            finally
            {
                if( start > 0)
                    RecordTotalExecutionTime(start);
                Metrics.DecrementConcurrentExecutionCount();
                _isExecutionComplete = true;
            }
        }

        private async Task<T> OnExecutionError(long ms, Exception e)
        {
            /*
            * Treat BadRequestException from ExecutionHook like a plain BadRequestException.
            */
            if (e is BadRequestException)
            {
                Metrics.MarkBadRequest(ms);
                throw e;
            }

            /**
             * All other error handling
             */
            // logger.debug("Error executing Command.run(). Proceeding to fallback logic ...", e);

            // report failure
            Metrics.MarkFailure(ms);
            // record the exception
            _executionResult.Exception = e;
            return await GetFallbackOrThrowException(EventType.FAILURE, FailureType.COMMAND_EXCEPTION, "failed", e);
        }

        /// <summary>
        /// Key to be used for request caching.
        /// <p>
        /// By default this returns null which means "do not cache".
        /// <p>
        /// To enable caching override this method and return a string key uniquely representing the state of a command instance.
        /// <p>
        /// If multiple command instances in the same request scope match keys then only the first will be executed and all others returned from cache.
        /// </summary> 
        /// <returns >key to use for caching</returns>
        protected virtual string GetCacheKey()
        {
            throw new NotImplementedException();
        }

        private async Task<T> GetFallbackOrThrowException(EventType eventType, FailureType failureType, string message, Exception ex)
        {
            try {
                if (IsUnrecoverable(ex))
                {
                    //  logger.error("Unrecoverable Error for Command so will throw RuntimeException and not apply fallback. ", ex);
                    // record the _executionResult
                    _executionResult.AddEvent(eventType);

                    /* executionHook for all errors */
                    throw new CommandRuntimeException(failureType, CommandName, GetLogMessagePrefix() + " " + message + " and encountered unrecoverable error.", ex, null);
                }

                _executionResult.AddEvent(eventType);

                if ((_flags & ServiceCommandOptions.HasFallBack) != ServiceCommandOptions.HasFallBack || Properties.FallbackEnabled.Get() == false)
                {
                    throw new CommandRuntimeException(failureType, CommandName, GetLogMessagePrefix() + " " + message + " and fallback disabled.", ex, null);
                }

                if (FallBackSemaphore.TryAcquire()) // fallback semaphore
                {
                    try
                    {
                        var fallback = await GetFallback();
                        Metrics.MarkFallbackSuccess();
                        _executionResult.AddEvent(EventType.FALLBACK_SUCCESS);
                        return fallback;
                    }
                    catch (Exception ex2)
                    {
                        var fe = ex2 is AggregateException ? ((AggregateException)ex2).InnerException : ex2;
                        if (fe is NotImplementedException)
                        {
                            // logger.debug("No fallback for Command. ", fe); // debug only since we're throwing the exception and someone higher will do something with it
                            throw new CommandRuntimeException(failureType, CommandName, GetLogMessagePrefix() + " and no fallback available.", ex, fe);
                        }
                        else
                        {
                            Metrics.MarkFallbackFailure();
                            _executionResult.AddEvent(EventType.FALLBACK_FAILURE);
                            throw new CommandRuntimeException(failureType, CommandName, GetLogMessagePrefix() + " and fallback failed.", ex, fe);
                        }
                    }
                    finally
                    {
                        FallBackSemaphore.Release();
                    }
                }
                else
                {
                    Metrics.MarkFallbackRejection();
                    _executionResult.AddEvent(EventType.FALLBACK_REJECTION);
                    //logger.debug("Command Fallback Rejection."); // debug only since we're throwing the exception and someone higher will do something with it
                    // if we couldn't acquire a permit, we "fail fast" by throwing an exception
                    throw new CommandRuntimeException(FailureType.REJECTED_SEMAPHORE_FALLBACK, CommandName, GetLogMessagePrefix() + "fallback execution rejected.", null, null);
                }
            }
            catch
            {
                // count that we are throwing an exception 
                Metrics.MarkExceptionThrown();
                throw;
            }
        }

        private string GetLogMessagePrefix()
        {
            return CommandName;
        }

        private bool IsUnrecoverable(Exception ex)
        {
            return ex is StackOverflowException || ex is OutOfMemoryException;// || ex is ExecutionEngineException;
        }

        protected abstract Task<T> Run(CancellationToken token);

        protected virtual Task<T> GetFallback()
        {
            throw new NotImplementedException();
        }

        /**
 * Record that this command was executed in the HystrixRequestLog.
 * <p>
 * This can be treated as an async operation as it just adds a references to "this" in the log even if the current command is still executing.
 */
        protected void RecordExecutedCommand()
        {
            if (Properties.RequestLogEnabled.Get())
            {
                // log this command execution regardless of what happened
                if (_currentRequestLog != null)
                {
                    _currentRequestLog.AddExecutedCommand(this);
                }
            }
        }

        /**
  * Record the duration of execution as response or exception is being returned to the caller.
  */
        protected void RecordTotalExecutionTime(long startTime)
        {
            long duration = _clock.EllapsedTimeInMs - startTime;
            // the total execution time for the user thread including queuing, thread scheduling, run() execution
            Metrics.AddUserThreadExecutionTime(duration);

            /*
             * We record the executionTime for command execution.
             * 
             * If the command is never executed (rejected, short-circuited, etc) then it will be left unset.
             * 
             * For this metric we include failures and successes as we use it for per-request profiling and debugging
             * whereas 'metrics.addCommandExecutionTime(duration)' is used by stats across many requests.
             */
            _executionResult.ExecutionTime = (int)duration;
        }
    }
}
