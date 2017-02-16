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
            Depth = ParseHeader("Depth", args => Model.Headers.DepthHeader.Parse(args.Single()));
            Overwrite = ParseHeader("Overwrite", args => Model.Headers.OverwriteHeader.Parse(args.Single()));
            Range = ParseHeader("Range", RangeHeader.Parse);
            If = ParseHeader("If", args => IfHeader.Parse(args.Single()));
            IfMatch = ParseHeader("If-Match", IfMatchHeader.Parse);
            IfNoneMatch = ParseHeader("If-None-Match", IfNoneMatchHeader.Parse);
            IfModifiedSince = ParseHeader("If-Modified-Since", args => IfModifiedSinceHeader.Parse(args.Single()));
            IfUnmodifiedSince = ParseHeader("If-Unmodified-Since", args => IfUnmodifiedSinceHeader.Parse(args.Single()));
            Timeout = ParseHeader("Timeout", TimeoutHeader.Parse);
        }

        public DepthHeader? Depth { get; set; }

        public bool? Overwrite { get; set; }

        public IfHeader If { get; set; }

        public IfMatchHeader IfMatch { get; set; }

        public IfNoneMatchHeader IfNoneMatch { get; set; }

        public IfModifiedSinceHeader IfModifiedSince { get; set; }

        public IfUnmodifiedSinceHeader IfUnmodifiedSince { get; set; }

        public RangeHeader Range { get; set; }

        public TimeoutHeader Timeout { get; set; }

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
