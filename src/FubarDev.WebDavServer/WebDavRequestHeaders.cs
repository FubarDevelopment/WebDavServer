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

        public WebDavRequestHeaders([NotNull] IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers, [NotNull] IWebDavContext context)
        {
            Headers = headers.ToDictionary(x => x.Key, x => (IReadOnlyCollection<string>)x.Value.ToList(), StringComparer.OrdinalIgnoreCase);
            Depth = ParseHeader("Depth", args => DepthHeader.Parse(args.Single()));
            Overwrite = ParseHeader("Overwrite", args => OverwriteHeader.Parse(args.Single()));
            Range = ParseHeader("Range", RangeHeader.Parse);
            If = ParseHeader("If", args => IfHeader.Parse(args.Single(), EntityTagComparer.Strong, context));
            IfMatch = ParseHeader("If-Match", IfMatchHeader.Parse);
            IfNoneMatch = ParseHeader("If-None-Match", IfNoneMatchHeader.Parse);
            IfModifiedSince = ParseHeader("If-Modified-Since", args => IfModifiedSinceHeader.Parse(args.Single()));
            IfUnmodifiedSince = ParseHeader("If-Unmodified-Since", args => IfUnmodifiedSinceHeader.Parse(args.Single()));
            Timeout = ParseHeader("Timeout", TimeoutHeader.Parse);
        }

        public DepthHeader? Depth { get; }

        public bool? Overwrite { get; }

        public IfHeader If { get; }

        public IfMatchHeader IfMatch { get; }

        public IfNoneMatchHeader IfNoneMatch { get; }

        public IfModifiedSinceHeader IfModifiedSince { get; }

        public IfUnmodifiedSinceHeader IfUnmodifiedSince { get; }

        public RangeHeader Range { get; }

        public TimeoutHeader Timeout { get; }

        public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Headers { get; }

        public IReadOnlyCollection<string> this[string name]
        {
            get
            {
                IReadOnlyCollection<string> v;
                if (Headers.TryGetValue(name, out v))
                    return v;
                return _empty;
            }
        }

        private T ParseHeader<T>(string name, [NotNull] Func<IReadOnlyCollection<string>, T> createFunc, T defaultValue = default(T))
        {
            IReadOnlyCollection<string> v;
            if (Headers.TryGetValue(name, out v))
            {
                if (v.Count != 0)
                    return createFunc(v);
            }

            return defaultValue;
        }
    }
}
