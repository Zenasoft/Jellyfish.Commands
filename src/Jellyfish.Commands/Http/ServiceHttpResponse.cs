// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Jellyfish.Commands.Http
{
    public class ServiceHttpResponse
    {
        public string Content { get; private set; }

        public string CorrelationId { get; private set; }

        public string Etag { get; private set; }

        public LinkHeaderValue Link { get; private set; }

        internal ServiceHttpResponse(string content, string etag, LinkHeaderValue link, string correlationId)
        {
            this.Content = content;
            this.Etag = etag;
            this.Link = link;
            this.CorrelationId = correlationId;
        }
    }
}