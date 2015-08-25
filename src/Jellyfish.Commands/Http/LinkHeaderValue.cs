// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;

namespace Jellyfish.Commands.Http
{
    public class LinkHeaderValue
    {
        static Regex pattern = new Regex(@"^(?<uri>.*?);\s*rel\s*=\s*(?<rel>\w+)\s*$");
        public Uri Uri { get; private set; }
        public string Relation { get; private set; }

        public LinkHeaderValue(string uri, string rel)
        {
            Uri value;
            Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out value);
            this.Uri = value;
            this.Relation = rel;
        }

        public static LinkHeaderValue Parse(string value)
        {
            string uri = null, rel = null;

            if (!String.IsNullOrEmpty(value))
            {
                Match result = pattern.Match(value ?? String.Empty);
                uri = result.Groups["uri"].Value;
                rel = result.Groups["rel"].Value;
            }

            return new LinkHeaderValue(uri, rel);
        }
    }
}