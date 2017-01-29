using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model
{
    public static class WebDavStatusCodesExtensions
    {
        private static readonly ConcurrentDictionary<WebDavStatusCodes, string> _reasonPhrases = new ConcurrentDictionary<WebDavStatusCodes, string>()
        {
            [WebDavStatusCodes.MultiStatus] = "Multi-Status",
            [WebDavStatusCodes.OK] = "OK",
        };

        [NotNull]
        public static string GetReasonPhrase(this WebDavStatusCodes statusCode, [CanBeNull] string additionalMessage = null)
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
                yield return name.Substring(startIndex);
        }
    }
}
