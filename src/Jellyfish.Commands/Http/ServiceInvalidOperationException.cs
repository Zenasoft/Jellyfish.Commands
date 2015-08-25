// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;

namespace Jellyfish.Commands.Http
{
    internal class ServiceInvalidOperationException : SystemException
    {
        private HttpRequestMessage request;
        private HttpResponseMessage response;
        private string v;

        public ServiceInvalidOperationException()
        {
        }

        public ServiceInvalidOperationException(string message) : base(message)
        {
        }

        public ServiceInvalidOperationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ServiceInvalidOperationException(string v, HttpRequestMessage request, HttpResponseMessage response)
        {
            this.v = v;
            this.request = request;
            this.response = response;
        }
    }
}