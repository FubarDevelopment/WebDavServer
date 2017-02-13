// <copyright file="WebDavRequestHeaders.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

namespace FubarDev.WebDavServer
{
    public class WebDavRequestHeaders : IWebDavRequestHeaders
    {
        private static readonly string[] _empty = new string[0];

        public WebDavRequestHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            Headers = headers.ToDictionary(x => x.Key, x => x.Value.ToList(), StringComparer.OrdinalIgnoreCase);
            Depth = ParseHeader("Depth", args => Model.Depth.Parse(args.Single()));
            Overwrite = ParseHeader("Overwrite", args => Model.Overwrite.Parse(args.Single()));
            Range = ParseHeader("Range", Model.Range.Parse);
            If = ParseHeader("If", args => Model.If.Parse(args.Single()));
            IfMatch = ParseHeader("If-Match", Model.IfMatch.Parse);
            IfNoneMatch = ParseHeader("If-None-Match", Model.IfNoneMatch.Parse);
            IfModifiedSince = ParseHeader("If-Modified-Since", args => Model.IfModifiedSince.Parse(args.Single()));
            IfUnmodifiedSince = ParseHeader("If-Unmodified-Since", args => Model.IfUnmodifiedSince.Parse(args.Single()));
        }

        public Model.Depth? Depth { get; set; }

        public bool? Overwrite { get; set; }

        public Model.If If { get; set; }

        public Model.IfMatch IfMatch { get; set; }

        public Model.IfNoneMatch IfNoneMatch { get; set; }

        public Model.IfModifiedSince IfModifiedSince { get; set; }

        public Model.IfUnmodifiedSince IfUnmodifiedSince { get; set; }

        public Model.Range Range { get; set; }

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

        private T ParseHeader<T>(string name, Func<List<string>, T> createFunc, T defaultValue = default(T))
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
