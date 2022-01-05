// <copyright file="IfCondition.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Text;

namespace FubarDev.WebDavServer.Models;

/// <summary>
/// A condition for the <c>If</c> header.
/// </summary>
/// <param name="Not">Invert the condition.</param>
/// <param name="StateToken">The state token.</param>
/// <param name="EntityTag">The entity tag.</param>
/// <remarks>
/// There is always one of the values set, either a <paramref name="StateToken"/>
/// or an <paramref name="EntityTag"/>.
/// </remarks>
public sealed record IfCondition(bool Not, Uri? StateToken, EntityTag? EntityTag)
{
    /// <inheritdoc />
    public override string ToString()
    {
        var result = new StringBuilder();
        if (Not)
        {
            result.Append("Not ");
        }

        if (StateToken != null)
        {
            result.AppendFormat("<{0}>", StateToken.OriginalString);
        }
        else
        {
            result.AppendFormat("[{0}]", EntityTag);
        }

        return result.ToString();
    }
}
