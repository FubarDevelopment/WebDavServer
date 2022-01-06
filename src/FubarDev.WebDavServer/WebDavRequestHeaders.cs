// <copyright file="WebDavRequestHeaders.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml;

using FubarDev.WebDavServer.Models;
using FubarDev.WebDavServer.Parsing;

using Microsoft.AspNetCore.Http;

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
    public WebDavRequestHeaders(IHeaderDictionary headers)
    {
        Headers = headers.ToDictionary(
            x => x.Key,
            x => (IReadOnlyList<string>)x.Value.ToList(),
            StringComparer.OrdinalIgnoreCase);
        Depth = ParseHeader("Depth", args => DepthHeader.Parse(args.Single()));
        Overwrite = ParseValueHeader("Overwrite", args => OverwriteHeader.Parse(args.Single()));
        Range = ParseHeader("Range", RangeHeader.Parse);
        If = ParseHeaders("If", ParseIfHeader);
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

    private static IfHeader ParseIfHeader(string s)
    {
        var lexer = new Lexer(s);
        var parser = new Parser(lexer);
        var parseResult = parser.ParseIfHeader();
        if (parseResult.IsError || (!lexer.IsEnd && lexer.Next().Kind != TokenType.End))
        {
            throw new WebDavException(WebDavStatusCode.BadRequest, "Invalid If header");
        }

        return parseResult.Ok.Value;
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
