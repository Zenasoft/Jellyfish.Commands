// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Jellyfish.Commands.CircuitBreaker;

namespace Jellyfish.Commands.Metrics.Publishers
{
    public interface IMetricsPublisherCommand
    {
    }

    public interface IMetricsPublisherCommandFactory
    {
        IMetricsPublisherCommand Create(string commandName, CommandMetrics metrics, ICircuitBreaker circuitBreaker, CommandProperties properties);
    }
}