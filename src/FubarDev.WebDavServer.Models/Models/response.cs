// <copyright file="response.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace FubarDev.WebDavServer.Models;

/// <summary>
/// Extensions for a <see cref="response"/>.
/// </summary>
// ReSharper disable InconsistentNaming
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Name created by xsd tool.")]
public partial class response
{
    /// <summary>
    /// Gets all <c>href</c> entries from a <see cref="response"/>.
    /// </summary>
    /// <returns>The list of found <c>href</c> values.</returns>
    public IEnumerable<string> GetHrefs()
    {
        if (!string.IsNullOrEmpty(href))
        {
            yield return href;
        }

        if (ItemsElementName != null)
        {
            var index = 0;
            foreach (var choiceType in ItemsElementName)
            {
                if (choiceType == ItemsChoiceType2.href)
                {
                    yield return (string)Items[index];
                }

                index += 1;
            }
        }
    }
}
