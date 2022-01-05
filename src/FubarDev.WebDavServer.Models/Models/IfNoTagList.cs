// <copyright file="IfNoTagList.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.Models;

/// <summary>
/// The <c>No-tag-list</c> of the<c>If</c> header.
/// </summary>
/// <param name="List">The conditions.</param>
public record IfNoTagList(
        IReadOnlyList<IfCondition> List)
{
    /// <inheritdoc />
    public override string ToString()
    {
        return $"({string.Join(" ", List)})";
    }
}
