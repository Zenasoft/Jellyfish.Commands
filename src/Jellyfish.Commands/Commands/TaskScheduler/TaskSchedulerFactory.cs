// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Jellyfish.Commands
{
    internal class TaskSchedulerFactory
    {
        private static TaskSchedulerFactory _instance = new TaskSchedulerFactory();
        private ConcurrentDictionary<string, TaskScheduler> _schedulers = new ConcurrentDictionary<string, TaskScheduler>();

        private TaskSchedulerFactory()
        {
        }

        internal static TaskScheduler CreateOrRetrieve(int threadCount, string name)
        {
            return _instance._schedulers.GetOrAdd(name, (n) => new BulkheadTaskScheduler(threadCount, name));
        }
    }
}