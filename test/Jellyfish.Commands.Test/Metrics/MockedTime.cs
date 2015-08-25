// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Jellyfish.Commands.Utils;
using System.Threading;

namespace Jellyfish.Commands.Tests
{
    internal class MockedClock : IClock
    {
        private long time=1;

        public MockedClock()
        {
        }

        public void Increment(long timeInMs)
        {
            Interlocked.Add(ref time, timeInMs);
        }

        public long EllapsedTimeInMs
        {
            get
            {
                return time;
            }
        }
    }
}