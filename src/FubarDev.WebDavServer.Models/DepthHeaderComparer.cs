// <copyright file="DepthHeaderComparer.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.Models;

namespace FubarDev.WebDavServer;

/// <summary>
/// Comparer for the <see cref="DepthHeader"/>.
/// </summary>
public class DepthHeaderComparer : IComparer<DepthHeader>, IEqualityComparer<DepthHeader>
{
    /// <summary>
    /// Gets the default depth header comparer.
    /// </summary>
    public static DepthHeaderComparer Default { get; } = new DepthHeaderComparer();

    /// <inheritdoc />
    public int Compare(DepthHeader? x, DepthHeader? y)
    {
        if (x is null && y is null)
        {
            return 0;
        }

        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        return x.OrderValue.CompareTo(y.OrderValue);
    }

    /// <inheritdoc />
    public bool Equals(DepthHeader? x, DepthHeader? y)
    {
        return Compare(x, y) == 0;
    }

    /// <inheritdoc />
    public int GetHashCode(DepthHeader obj)
    {
        return obj.OrderValue.GetHashCode();
    }
}
