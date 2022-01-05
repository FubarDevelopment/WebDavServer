// <copyright file="IfHeader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.Models;

/// <summary>
/// The <c>If</c> header.
/// </summary>
public class IfHeader
{
    private readonly IReadOnlyList<IfNoTagList>? _noTagLists;
    private readonly IReadOnlyList<IfTaggedList>? _taggedLists;

    /// <summary>
    /// Initializes a new instance of the <see cref="IfHeader"/> class.
    /// </summary>
    /// <param name="taggedLists">The tagged lists.</param>
    /// <exception cref="ArgumentOutOfRangeException">The list must contain at least one element.</exception>
    public IfHeader(IReadOnlyList<IfTaggedList> taggedLists)
    {
        if (taggedLists.Count == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(taggedLists));
        }

        _taggedLists = taggedLists;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IfHeader"/> class.
    /// </summary>
    /// <param name="noTagLists">The untagged lists.</param>
    /// <exception cref="ArgumentOutOfRangeException">The list must contain at least one element.</exception>
    public IfHeader(IReadOnlyList<IfNoTagList> noTagLists)
    {
        if (noTagLists.Count == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(noTagLists));
        }

        _noTagLists = noTagLists;
    }

    /// <summary>
    /// Gets a value indicating whether this instance has no tag lists.
    /// </summary>
    public bool IsNoTagList => _noTagLists != null;

    /// <summary>
    /// Gets a value indicating whether this instance has tagged lists.
    /// </summary>
    public bool IsTaggedList => _taggedLists != null;

    /// <summary>
    /// Gets the no-tag-list elements.
    /// </summary>
    /// <exception cref="InvalidOperationException">This header doesn't contain no-tag-list elements.</exception>
    public IReadOnlyList<IfNoTagList> NoTagLists =>
        _noTagLists ?? throw new InvalidOperationException();

    /// <summary>
    /// Gets the tagged-list elements.
    /// </summary>
    /// <exception cref="InvalidOperationException">This header doesn't contain tagged-list elements.</exception>
    public IReadOnlyList<IfTaggedList> TaggedLists =>
        _taggedLists ?? throw new InvalidOperationException();

    /// <inheritdoc />
    public override string ToString()
    {
        if (_taggedLists != null)
        {
            return string.Join(" ", _taggedLists);
        }

        return string.Join(" ", _noTagLists!);
    }
}
