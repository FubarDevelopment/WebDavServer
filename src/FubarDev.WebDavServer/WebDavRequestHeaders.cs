// <copyright file="WebDavRequestHeaders.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using FubarDev.WebDavServer.Model.Headers;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer
{
    public class WebDavRequestHeaders : IWebDavRequestHeaders
    {
        [NotNull]
        [ItemNotNull]
        private static readonly string[] _empty = new string[0];

        public WebDavRequestHeaders([NotNull] IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            Headers = headers.ToDictionary(x => x.Key, x => x.Value.ToList(), StringComparer.OrdinalIgnoreCase);
            Depth = ParseHeader("Depth", args => Model.Headers.Depth.Parse(args.Single()));
            Overwrite = ParseHeader("Overwrite", args => Model.Headers.Overwrite.Parse(args.Single()));
            Range = ParseHeader("Range", Range.Parse);
            If = ParseHeader("If", args => If.Parse(args.Single()));
            IfMatch = ParseHeader("If-Match", IfMatch.Parse);
            IfNoneMatch = ParseHeader("If-None-Match", IfNoneMatch.Parse);
            IfModifiedSince = ParseHeader("If-Modified-Since", args => IfModifiedSince.Parse(args.Single()));
            IfUnmodifiedSince = ParseHeader("If-Unmodified-Since", args => IfUnmodifiedSince.Parse(args.Single()));
            Timeout = ParseHeader("Timeout", Timeout.Parse);
        }

        public Depth? Depth { get; set; }

        public bool? Overwrite { get; set; }

        public If If { get; set; }

        public IfMatch IfMatch { get; set; }

        public IfNoneMatch IfNoneMatch { get; set; }

        public IfModifiedSince IfModifiedSince { get; set; }

        public IfUnmodifiedSince IfUnmodifiedSince { get; set; }

        public Range Range { get; set; }

        public Timeout Timeout { get; set; }

        public IDictionary<string, List<string>> Headers { get; }

        public IReadOnlyCollection<string> this[string name]
        {
            get
            {
                List<string> v;
                if (Headers.TryGetValue(name, out v))
                    return v;
                return _empty;
            }
        }

        private T ParseHeader<T>(string name, [NotNull] Func<List<string>, T> createFunc, T defaultValue = default(T))
        {
            List<string> v;
            if (Headers.TryGetValue(name, out v))
            {
                if (v.Count != 0)
                    return createFunc(v);
            }

            return defaultValue;
        }
    }
}
