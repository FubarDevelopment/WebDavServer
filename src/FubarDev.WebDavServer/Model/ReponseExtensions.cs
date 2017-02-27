// <copyright file="ReponseExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model
{
    /// <summary>
    /// Extensions for a <see cref="response"/>
    /// </summary>
    public static class ReponseExtensions
    {
        /// <summary>
        /// Gets all <c>href</c> entries from a <see cref="response"/>
        /// </summary>
        /// <param name="response">The response to get the <c>href</c> values from</param>
        /// <returns>The list of found <c>href</c> values</returns>
        [NotNull]
        [ItemNotNull]
        public static IEnumerable<string> GetHrefs([NotNull] this response response)
        {
            if (!string.IsNullOrEmpty(response.href))
                yield return response.href;
            if (response.ItemsElementName != null)
            {
                var index = 0;
                foreach (var choiceType in response.ItemsElementName)
                {
                    if (choiceType == ItemsChoiceType2.href)
                        yield return (string)response.Items[index];
                    index += 1;
                }
            }
        }
    }
}
