// <copyright file="WebDavRequestHeaders.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml;

using FubarDev.WebDavServer.Models;

using Microsoft.AspNetCore.Http;

using IfHeader = FubarDev.WebDavServer.Model.Headers.IfHeader;

namespace FubarDev.WebDavServer;

/// <summary>
/// Implementation of the <see cref="IWebDavRequestHeaders"/> interface.
/// </summary>
public class WebDavRequestHeaders : IWebDavRequestHeaders
{
    private static readonly string[] _empty = Array.Empty<string>();

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDavRequestHeaders"/> class.
    /// </summary>
    /// <param name="headers">The headers to parse.</param>
    /// <param name="context">The WebDAV request context.</param>
    public WebDavRequestHeaders(IHeaderDictionary headers, IWebDavContext context)
    {
        Headers = headers.ToDictionary(
            x => x.Key,
            x => (IReadOnlyList<string>)x.Value.ToList(),
            StringComparer.OrdinalIgnoreCase);
        Depth = ParseHeader("Depth", args => DepthHeader.Parse(args.Single()));
        Overwrite = ParseValueHeader("Overwrite", args => OverwriteHeader.Parse(args.Single()));
        Range = ParseHeader("Range", RangeHeader.Parse);
        If = ParseHeaders("If", arg => IfHeader.Parse(arg, EntityTagComparer.Strong, context));
        IfMatch = ParseHeader("If-Match", IfMatchHeader.Parse);
        IfNoneMatch = ParseHeader("If-None-Match", IfNoneMatchHeader.Parse);
        IfModifiedSince = ParseHeader("If-Modified-Since", args => IfModifiedSinceHeader.Parse(args.Single()));
        IfUnmodifiedSince = ParseHeader("If-Unmodified-Since", args => IfUnmodifiedSinceHeader.Parse(args.Single()));
        Timeout = ParseHeader("Timeout", TimeoutHeader.Parse);
        ContentLength = ParseValueHeader("Content-Length", args => (long?)XmlConvert.ToInt64(args.Single()));
    }

    /// <inheritdoc />
    public long? ContentLength { get; }

    /// <inheritdoc />
    public DepthHeader? Depth { get; }

    /// <inheritdoc />
    public bool? Overwrite { get; }

    /// <inheritdoc />
    public IReadOnlyList<IfHeader>? If { get; }

    /// <inheritdoc />
    public IfMatchHeader? IfMatch { get; }

    /// <inheritdoc />
    public IfNoneMatchHeader? IfNoneMatch { get; }

    /// <inheritdoc />
    public IfModifiedSinceHeader? IfModifiedSince { get; }

    /// <inheritdoc />
    public IfUnmodifiedSinceHeader? IfUnmodifiedSince { get; }

    /// <inheritdoc />
    public RangeHeader? Range { get; }

    /// <inheritdoc />
    public TimeoutHeader? Timeout { get; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Headers { get; }

    /// <inheritdoc />
    public IReadOnlyCollection<string> this[string name]
    {
        get
        {
            if (Headers.TryGetValue(name, out var v))
            {
                return v;
            }

            return _empty;
        }
    }

    private T? ParseValueHeader<T>(string name, Func<IReadOnlyCollection<string>, T?> createFunc, T? defaultValue = default)
        where T : struct
    {
        if (Headers.TryGetValue(name, out var v))
        {
            if (v.Count != 0)
            {
                return createFunc(v);
            }
        }

        return defaultValue;
    }

    private T? ParseHeader<T>(string name, Func<IReadOnlyCollection<string>, T> createFunc, T? defaultValue = default)
        where T : class
    {
        if (Headers.TryGetValue(name, out var v))
        {
            if (v.Count != 0)
            {
                return createFunc(v);
            }
        }

        return defaultValue;
    }

    private IReadOnlyList<T>? ParseHeaders<T>(
        string name,
        Func<string, T> createFunc)
    {
        if (Headers.TryGetValue(name, out var v))
        {
            return v.Select(createFunc).ToImmutableList();
        }

        return null;
    }
}
