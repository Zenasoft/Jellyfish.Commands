// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Jellyfish.Commands
{
    public interface ITryableSemaphore
    {
        bool TryAcquire();
        void Release();
    }

    internal class TryableSemaphoreNoOp : ITryableSemaphore
    {
        public bool TryAcquire() { return true; }
        public void Release() { }

        public override string ToString()
        {
            return "No op";
        }
    }
}