// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Jellyfish.Commands
{
    internal class BadRequestException :ApplicationException
    {
        public BadRequestException(string message) : base(message)
        {
        }
    }
}