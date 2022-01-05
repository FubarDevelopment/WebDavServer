// <copyright file="WebDavStatusCodeExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace FubarDev.WebDavServer.Model
{
    /// <summary>
    /// Extension methods for the <see cref="WebDavStatusCode"/>.
    /// </summary>
    public static class WebDavStatusCodeExtensions
    {
        private static readonly ConcurrentDictionary<WebDavStatusCode, string> _reasonPhrases = new ConcurrentDictionary<WebDavStatusCode, string>()
        {
            [WebDavStatusCode.MultiStatus] = "Multi-Status",
            [WebDavStatusCode.OK] = "OK",
        };

        /// <summary>
        /// Builds the reason phrase for a status code and an additional message.
        /// </summary>
        /// <param name="statusCode">The status code to build the reason phrase for.</param>
        /// <param name="additionalMessage">The additional message for the reason phrase.</param>
        /// <returns>The built reason phrase.</returns>
        public static string GetReasonPhrase(this WebDavStatusCode statusCode, string? additionalMessage = null)
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
            {
                return reasonPhrase;
            }

            return $"{reasonPhrase} ({additionalMessage})";
        }

        private static IEnumerable<string> GetParts(string name)
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
            {
                yield return name.Substring(startIndex);
            }
        }
    }
}
