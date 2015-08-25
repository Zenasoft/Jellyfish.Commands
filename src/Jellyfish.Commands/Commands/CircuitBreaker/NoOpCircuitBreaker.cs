// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Jellyfish.Commands.CircuitBreaker
{
    internal class NoOpCircuitBreaker : ICircuitBreaker
    {
        public bool AllowRequest
        {
            get
            {
                return true;
            }
        }

        public bool IsOpen()
        {
            return false;
        }

        public void MarkSuccess()
        {
        }
    }
}