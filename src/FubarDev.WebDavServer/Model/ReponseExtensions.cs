// <copyright file="ReponseExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model
{
    public static class ReponseExtensions
    {
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
