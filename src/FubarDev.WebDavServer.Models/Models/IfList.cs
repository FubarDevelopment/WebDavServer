// <copyright file="IfList.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections;

namespace FubarDev.WebDavServer.Models;

/// <summary>
/// List of conditions.
/// </summary>
public class IfList : IReadOnlyList<IfCondition>
{
    private readonly IReadOnlyList<IfCondition> _conditions;

    public IfList(IReadOnlyList<IfCondition> conditions)
    {
        _conditions = conditions;
    }

    /// <summary>
    /// Gets a value indicating whether this condition list requires the <see cref="EntityTag"/>
    /// of a file system entry for the evaluation.
    /// </summary>
    public bool RequiresEntityTag => _conditions.Any(x => x.EntityTag != null && !x.Not);

    /// <summary>
    /// Gets a value indicating whether this condition list requires the <c>StateToken</c>
    /// of the active lock for the evaluation.
    /// </summary>
    public bool RequiresStateToken => _conditions.Any(x => x.StateToken != null && !x.Not);

    /// <inheritdoc />
    public int Count => _conditions.Count;

    /// <inheritdoc />
    public IfCondition this[int index] => _conditions[index];

    /// <inheritdoc />
    public IEnumerator<IfCondition> GetEnumerator()
    {
        return _conditions.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_conditions).GetEnumerator();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return string.Join(" ", _conditions);
    }
}
