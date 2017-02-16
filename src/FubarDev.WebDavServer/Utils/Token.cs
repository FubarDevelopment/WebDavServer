// <copyright file="Token.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

namespace FubarDev.WebDavServer.Utils
{
    internal static class Token
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

        public static string ReadToken(string text)
        {
            var index = 0;
            while (index < text.Length)
            {
                var ch = text[index];
                if (_separators.Contains(ch) || _ctls.Contains(ch))
                    break;
                ++index;
            }

            return text.Substring(0, index);
        }
    }
}
