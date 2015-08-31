// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Jellyfish.Commands.CircuitBreaker;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;


namespace Jellyfish.Commands.Metrics.Publishers
{
    public class MetricsPublisherFactory
    {
        private ConcurrentDictionary<string, IMetricsPublisherCommand> _commandPublishers = new ConcurrentDictionary<string, IMetricsPublisherCommand>();
        private IJellyfishContext _context;

        internal MetricsPublisherFactory(IJellyfishContext context)
        {
            _context = context;
        }

        public IMetricsPublisherCommand CreateOrRetrievePublisherForCommand(string commandName, CommandMetrics metrics, ICircuitBreaker circuitBreaker)
        {
            return GetOrCreatePublisherForCommand(commandName, metrics, circuitBreaker);
        }

        private IMetricsPublisherCommand GetOrCreatePublisherForCommand(string commandName, CommandMetrics metrics, ICircuitBreaker circuitBreaker)
        {
            // attempt to retrieve from cache first
            IMetricsPublisherCommand publisher;
            if (_commandPublishers.TryGetValue(commandName, out publisher))
            {
                return publisher;
            }

            // it doesn't exist so we need to create it
            publisher = _context.GetService<IMetricsPublisherCommand>() ?? new DefaultPublisherCommand();

            // attempt to store it (race other threads)
            return _commandPublishers.GetOrAdd(commandName, publisher);
        }
    }

    internal class DefaultPublisherCommand : IMetricsPublisherCommand
    {
        public void Run(Action<string> handler)
        {
        }
    }
}
