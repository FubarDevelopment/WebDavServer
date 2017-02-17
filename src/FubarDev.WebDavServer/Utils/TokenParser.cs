// <copyright file="TokenParser.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FubarDev.WebDavServer.Utils
{
    internal static class TokenParser
    {
        private static readonly ISet<char> _separators = new HashSet<char>(
            new[]
            {
                '(', ')', '<', '>', '@',
                ',', ';', ':', '\\', '"',
                '/', '[', ']', '?', '=',
                '{', '}', ' ', '\t',
            });

        private static readonly ISet<char> _ctls = new HashSet<char>(Enumerable.Range(0, 32).Select(x => (char)x).Concat(new[] { '\u007F' }));

        public static string ReadToken(StringSource source)
        {
            var result = new StringBuilder();
            source.SkipWhiteSpace();
            while (!source.Empty)
            {
                var ch = source.Get();
                if (_separators.Contains(ch) || _ctls.Contains(ch))
                {
                    source.Back();
                    break;
                }

                result.Append(ch);
            }

            return result.ToString();
        }

        public static string ReadToken(string text)
        {
            return ReadToken(new StringSource(text));
        }
    }
}
