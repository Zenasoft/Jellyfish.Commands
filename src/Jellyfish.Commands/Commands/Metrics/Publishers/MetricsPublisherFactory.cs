// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Jellyfish.Commands.CircuitBreaker;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Jellyfish.Commands.Metrics.Publishers
{
    public class MetricsPublisherFactory
    {
        public static MetricsPublisherFactory Instance { get; } = new MetricsPublisherFactory();

        private ConcurrentDictionary<string, IMetricsPublisherCommand> _commandPublishers = new ConcurrentDictionary<string, IMetricsPublisherCommand>();
        private Dictionary<string, IMetricsPublisherCommandFactory> _commandPublisherFactories = new Dictionary<string, IMetricsPublisherCommandFactory>();

        private MetricsPublisherFactory()
        {
        }

        public void AddFactory(string name, IMetricsPublisherCommandFactory factory)
        {
            Contract.Assert(!String.IsNullOrEmpty(name));
            Contract.Assert(factory != null);
            _commandPublisherFactories.Add(name, factory);
        }

        public static IMetricsPublisherCommand CreateOrRetrievePublisherForCommand(string commandName, CommandMetrics metrics, ICircuitBreaker circuitBreaker, CommandProperties properties)
        {
            return MetricsPublisherFactory.Instance.GetOrCreatePublisherForCommand(commandName, metrics, circuitBreaker, properties);
        }

        public IMetricsPublisherCommand GetPublisherForCommand(string commandName)
        {
            IMetricsPublisherCommand publisher;
            _commandPublishers.TryGetValue(commandName, out publisher);
            return publisher;
        }

        private IMetricsPublisherCommand GetOrCreatePublisherForCommand(string commandName, CommandMetrics metrics, ICircuitBreaker circuitBreaker, CommandProperties properties)
        {
            // attempt to retrieve from cache first
            IMetricsPublisherCommand publisher;
            if (_commandPublishers.TryGetValue(commandName, out publisher))
            {
                return publisher;
            }

            // it doesn't exist so we need to create it
            IMetricsPublisherCommandFactory factory;
            if (!_commandPublisherFactories.TryGetValue(commandName, out factory))
                return null;

            // attempt to store it (race other threads)
            return _commandPublishers.GetOrAdd(commandName, factory.Create(commandName, metrics, circuitBreaker, properties));
        }
    }
}
