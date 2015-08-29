// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Jellyfish.Commands.Metrics;
using Jellyfish.Commands.Utils;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;

namespace Jellyfish.Commands.CircuitBreaker
{
    public class CircuitBreakerFactory
    {
        private static ConcurrentDictionary<string, ICircuitBreaker> _metrics = new ConcurrentDictionary<string, ICircuitBreaker>();

        public static ICircuitBreaker GetInstance(string name)
        {
            Contract.Assert(!String.IsNullOrEmpty(name));
            ICircuitBreaker value;
            _metrics.TryGetValue(name, out value);
            return value;
        }

        internal static ICircuitBreaker GetOrCreateInstance(string name, CommandProperties properties, CommandMetrics metrics, IClock clock)
        {
            Contract.Assert(!String.IsNullOrEmpty(name));
            Contract.Assert(properties != null);
            Contract.Assert(metrics != null);
            Contract.Assert(clock != null);

            return _metrics.GetOrAdd(name, n => new DefaultCircuitBreaker(properties, metrics, clock));
        }
    }
}