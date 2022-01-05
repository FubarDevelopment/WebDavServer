// <copyright file="IfTaggedList.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.Models;

/// <summary>
/// The <c>No-tag-list</c> of the<c>If</c> header.
/// </summary>
/// <param name="ResourceTag">The resource tag.</param>
/// <param name="Lists">The condition lists.</param>
public sealed record IfTaggedList(
    Uri ResourceTag,
    IReadOnlyList<IfList> Lists)
{
    /// <inheritdoc />
    public override string ToString()
    {
        var listEntries = Lists
            .Select(list => $"({list})");
        return $"<{ResourceTag.OriginalString}> ({string.Join(") (", listEntries)})";
    }
}
