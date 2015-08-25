// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Jellyfish.Configuration;
using System.Threading;

namespace Jellyfish.Commands
{
    /// <summary>
    /// Semaphore that only supports tryAcquire and never blocks and that supports a dynamic permit count.
    /// </summary>
    internal class TryableSemaphoreActual : ITryableSemaphore
    {
        internal IDynamicProperty<int> NumberOfPermits { get; private set; }
        private long count;

        public TryableSemaphoreActual(IDynamicProperty<int> numberOfPermits)
        {
            this.NumberOfPermits = numberOfPermits;
        }

        public bool TryAcquire()
        {
            int currentCount = (int)Interlocked.Increment(ref count);
            if (currentCount > NumberOfPermits.Get())
            {
                Interlocked.Decrement(ref count);
                return false;
            }
            else
            {
                return true;
            }
        }

        public void Release()
        {
            Interlocked.Decrement(ref count);
        }

        public int NumberOfPermitsUsed
        {
            get
            {
                return (int)Interlocked.Read(ref count);
            }
        }

        public override string ToString()
        {
            return "Max=" + NumberOfPermits.ToString();
        }
    }
}