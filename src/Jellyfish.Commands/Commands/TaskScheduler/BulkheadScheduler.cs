// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfish.Commands
{
    //-------------------------------------------------------------------------- 
    //  
    //  Copyright (c) Microsoft Corporation.  All rights reserved.  
    //  
    //  File: QueuedTaskScheduler.cs 
    // 
    //-------------------------------------------------------------------------- 

    /// <summary> 
    /// Provides a TaskScheduler that provides control over priorities, fairness, and the underlying threads utilized. 
    /// </summary> 
    [DebuggerTypeProxy(typeof(QueuedTaskSchedulerDebugView))]
   // [DebuggerDisplay("Id={Id}, Queues={DebugQueueCount}, ScheduledTasks = {DebugTaskCount}")]
    public sealed class BulkheadTaskScheduler : TaskScheduler, IDisposable
    {
        /// <summary>Debug view for the QueuedTaskScheduler.</summary> 
        private class QueuedTaskSchedulerDebugView
        {
            /// <summary>The scheduler.</summary> 
            private BulkheadTaskScheduler _scheduler;

            /// <summary>Initializes the debug view.</summary> 
            /// <param name="scheduler">The scheduler.</param> 
            public QueuedTaskSchedulerDebugView(BulkheadTaskScheduler scheduler)
            {
                if (scheduler == null) throw new ArgumentNullException("scheduler");
                _scheduler = scheduler;
            }

            /// <summary>Gets all of the Tasks queued to the scheduler directly.</summary> 
            public IEnumerable<Task> ScheduledTasks
            {
                get
                {
                    var tasks = (IEnumerable<Task>)_scheduler._blockingTaskQueue;
                    return tasks.Where(t => t != null).ToList();
                }
            }
        }

        /// <summary>Cancellation token used for disposal.</summary> 
        private readonly CancellationTokenSource _disposeCancellation = new CancellationTokenSource();
        /// <summary> 
        /// The maximum allowed concurrency level of this scheduler.  If custom threads are 
        /// used, this represents the number of created threads. 
        /// </summary> 
        private readonly int _concurrencyLevel;
        /// <summary>Whether we're processing tasks on the current thread.</summary> 
        private static ThreadLocal<bool> _taskProcessingThread = new ThreadLocal<bool>();

        /// <summary>The threads used by the scheduler to process work.</summary> 
        private readonly System.Threading.Thread[] _threads;
        /// <summary>The collection of tasks to be executed on our custom threads.</summary> 
        private readonly BlockingCollection<Task> _blockingTaskQueue;
        private int _pendingTasks;

        /// <summary>Initializes the scheduler.</summary> 
        /// <param name="threadCount">The number of threads to create and use for processing work items.</param> 
        /// <param name="threadName">The name to use for each of the created threads.</param> 
        public BulkheadTaskScheduler(
            int threadCount,
            string threadName)
        {
            // Validates arguments (some validation is left up to the Thread type itself). 
            // If the thread count is 0, default to the number of logical processors. 
            if (threadCount < 0) throw new ArgumentOutOfRangeException("concurrencyLevel");
            else if (threadCount == 0) _concurrencyLevel = Environment.ProcessorCount;
            else _concurrencyLevel = threadCount;

            // Initialize the queue used for storing tasks 
            _blockingTaskQueue = new BlockingCollection<Task>(threadCount);

            // Create all of the threads 
            _threads = new Thread[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                _threads[i] = new Thread(() => ThreadBasedDispatchLoop());
                if (threadName != null) _threads[i].Name = "jellyfish-" + threadName + " (" + i + ")";
            }

            // Start all of the threads 
            foreach (var thread in _threads) thread.Start();
        }

        /// <summary>The dispatch loop run by all threads in this scheduler.</summary>
        private void ThreadBasedDispatchLoop()
        {
            _taskProcessingThread.Value = true;

            try
            {
                // If the scheduler is disposed, the cancellation token will be set and 
                // we'll receive an OperationCanceledException.  That OCE should not crash the process. 
                try
                {
                    // If a thread abort occurs, we'll try to reset it and continue running. 
                    while (true)
                    {
                        try
                        {
                            // For each task queued to the scheduler, try to execute it. 
                            foreach (var task in _blockingTaskQueue.GetConsumingEnumerable(_disposeCancellation.Token))
                            {
                                Interlocked.Increment(ref _pendingTasks);
                                task.ContinueWith(t => { Interlocked.Decrement(ref _pendingTasks); });
                                try { 
                                    TryExecuteTask(task);
                                }
                                catch { Interlocked.Decrement(ref _pendingTasks); }
                            }
                        }
                        catch (ThreadAbortException)
                        {
                            // If we received a thread abort, and that thread abort was due to shutting down 
                            // or unloading, let it pass through.  Otherwise, reset the abort so we can 
                            // continue processing work items. 
                            if (!Environment.HasShutdownStarted && !AppDomain.CurrentDomain.IsFinalizingForUnload())
                            {
                                Thread.ResetAbort();
                            }
                        }
                    }
                }
                catch (OperationCanceledException) { }
            }
            finally
            {
                // Run a cleanup routine if there was one 
                _taskProcessingThread.Value = false;
            }
        }

        /// <summary>Queues a task to the scheduler.</summary> 
        /// <param name="task">The task to be queued.</param> 
        protected override void QueueTask(Task task)
        {
            var pendings =  _pendingTasks + _blockingTaskQueue.Count;

            // If we've been disposed, no one should be queueing 
            if (_disposeCancellation.IsCancellationRequested || pendings  >= _concurrencyLevel) throw new RejectedExecutionException();
            _blockingTaskQueue.Add(task);
        }

        /// <summary>Tries to execute a task synchronously on the current thread.</summary> 
        /// <param name="task">The task to execute.</param> 
        /// <param name="taskWasPreviouslyQueued">Whether the task was previously queued.</param> 
        /// <returns>true if the task was executed; otherwise, false.</returns> 
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            // If we're already running tasks on this threads, enable inlining 
            return _taskProcessingThread.Value && TryExecuteTask(task);
        }

        /// <summary>Gets the tasks scheduled to this scheduler.</summary> 
        /// <returns>An enumerable of all tasks queued to this scheduler.</returns> 
        /// <remarks>This does not include the tasks on sub-schedulers.  Those will be retrieved by the debugger separately.</remarks> 
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            // Get all of the tasks, filtering out nulls, which are just placeholders 
            // for tasks in other sub-schedulers 
            return _blockingTaskQueue.Where(t => t != null).ToList();
        }

        /// <summary>Gets the maximum concurrency level to use when processing tasks.</summary> 
        public override int MaximumConcurrencyLevel { get { return _concurrencyLevel; } }

        /// <summary>Initiates shutdown of the scheduler.</summary> 
        public void Dispose()
        {
            _disposeCancellation.Cancel();
        }
    }
}
