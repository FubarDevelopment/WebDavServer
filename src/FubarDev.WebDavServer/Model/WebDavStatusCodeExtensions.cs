// <copyright file="WebDavStatusCodeExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model
{
    public static class WebDavStatusCodeExtensions
    {
        [NotNull]
        private static readonly ConcurrentDictionary<WebDavStatusCode, string> _reasonPhrases = new ConcurrentDictionary<WebDavStatusCode, string>()
        {
            [WebDavStatusCode.MultiStatus] = "Multi-Status",
            [WebDavStatusCode.OK] = "OK",
        };

        [NotNull]
        public static string GetReasonPhrase(this WebDavStatusCode statusCode, [CanBeNull] string additionalMessage = null)
        {
            var reasonPhrase = _reasonPhrases.GetOrAdd(
                statusCode,
                key =>
                {
                    var name = new StringBuilder();
                    var isFirst = true;
                    foreach (var part in GetParts(key.ToString("G")))
                    {
                        if (isFirst)
                        {
                            isFirst = false;
                        }
                        else
                        {
                            name.Append(' ');
                        }

                        name.Append(part);
                    }
                    return name.ToString();
                });

            if (string.IsNullOrEmpty(additionalMessage))
                return reasonPhrase;

            return $"{reasonPhrase} ({additionalMessage})";
        }

        [NotNull]
        [ItemNotNull]
        private static IEnumerable<string> GetParts([NotNull] string name)
        {
            var startIndex = 0;
            var currentIndex = 1;
            while (currentIndex < name.Length)
            {
                if (char.IsUpper(name, currentIndex))
                {
                    yield return name.Substring(startIndex, currentIndex - startIndex);
                    startIndex = currentIndex;
                }

                currentIndex += 1;
            }

            if (startIndex < currentIndex)
                yield return name.Substring(startIndex);
        }
    }
}
