// <copyright file="IfHeaderMatch.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.WebDavServer.Models;

namespace FubarDev.WebDavServer.Utils;

/// <summary>
/// Information about a header match.
/// </summary>
public class IfHeaderMatch
{
    private readonly IfNoTagList? _noTagList;
    private readonly IfTaggedList? _taggedList;

    /// <summary>
    /// Initializes a new instance of the <see cref="IfHeaderMatch"/> class.
    /// </summary>
    /// <param name="header">The matched header.</param>
    /// <param name="noTagList">The matched <c>No-tag-list</c>.</param>
    public IfHeaderMatch(
        IfHeader header,
        IfNoTagList noTagList)
    {
        Header = header;
        _noTagList = noTagList;
        List = noTagList.List;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IfHeaderMatch"/> class.
    /// </summary>
    /// <param name="header">The matched header.</param>
    /// <param name="taggedList">The matched <c>Tagged-list</c>.</param>
    /// <param name="list">The matched condition list.</param>
    public IfHeaderMatch(
        IfHeader header,
        IfTaggedList taggedList,
        IfList list)
    {
        Header = header;
        _taggedList = taggedList;
        List = list;
    }

    /// <summary>
    /// Gets the matched <c>If</c> header.
    /// </summary>
    public IfHeader Header { get; }

    /// <summary>
    /// Gets a value indicating whether this is a <c>No-tag-list</c>.
    /// </summary>
    public bool IsNoTagList => _noTagList != null;

    /// <summary>
    /// Gets a value indicating whether this is a <c>Tagged-list</c>.
    /// </summary>
    public bool IsTaggedList => _taggedList != null;

    /// <summary>
    /// Gets the matched <c>No-tag-list</c>.
    /// </summary>
    public IfNoTagList NoTagList => _noTagList ?? throw new InvalidOperationException();

    /// <summary>
    /// Gets the matched <c>Tagged-list</c>.
    /// </summary>
    public IfTaggedList TaggedList => _taggedList ?? throw new InvalidOperationException();

    /// <summary>
    /// Gets the matched condition list.
    /// </summary>
    public IfList List { get; }
}
