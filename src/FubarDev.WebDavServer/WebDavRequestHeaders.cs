// <copyright file="WebDavRequestHeaders.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using FubarDev.WebDavServer.Model.Headers;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer
{
    /// <summary>
    /// Implementation of the <see cref="IWebDavRequestHeaders"/> interface
    /// </summary>
    public class WebDavRequestHeaders : IWebDavRequestHeaders
    {
        [NotNull]
        [ItemNotNull]
        private static readonly string[] _empty = new string[0];

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavRequestHeaders"/> class.
        /// </summary>
        /// <param name="headers">The headers to parse</param>
        /// <param name="context">The WebDAV request context</param>
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
            ContentLength = ParseHeader("Content-Length", args => (long?)XmlConvert.ToInt64(args.Single()));
        }

        /// <inheritdoc />
        public long? ContentLength { get; }

        /// <inheritdoc />
        public DepthHeader? Depth { get; }

        /// <inheritdoc />
        public bool? Overwrite { get; }

        /// <inheritdoc />
        public IfHeader If { get; }

        /// <inheritdoc />
        public IfMatchHeader IfMatch { get; }

        /// <inheritdoc />
        public IfNoneMatchHeader IfNoneMatch { get; }

        /// <inheritdoc />
        public IfModifiedSinceHeader IfModifiedSince { get; }

        /// <inheritdoc />
        public IfUnmodifiedSinceHeader IfUnmodifiedSince { get; }

        /// <inheritdoc />
        public RangeHeader Range { get; }

        /// <inheritdoc />
        public TimeoutHeader Timeout { get; }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Headers { get; }

        /// <inheritdoc />
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
