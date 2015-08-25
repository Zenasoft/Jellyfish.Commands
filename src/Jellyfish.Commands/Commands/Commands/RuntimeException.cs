// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Jellyfish.Commands
{
    public enum FailureType
    {
        BAD_REQUEST_EXCEPTION, COMMAND_EXCEPTION, TIMEOUT, SHORTCIRCUIT, REJECTED_THREAD_EXECUTION, REJECTED_SEMAPHORE_EXECUTION, REJECTED_SEMAPHORE_FALLBACK
    }

    internal class CommandRuntimeException : SystemException
    {
        public Exception FallbackException { get; private set; }
        public FailureType FailureCause { get; private set; }
        public string CommandName { get; private set; }

        public CommandRuntimeException(FailureType failure, string message, string commandName, Exception ex, Exception fallbackException) : base(message, ex)
        {
            this.FailureCause = failure;
            this.FallbackException = fallbackException;
            this.CommandName = commandName;
        }
    }
}