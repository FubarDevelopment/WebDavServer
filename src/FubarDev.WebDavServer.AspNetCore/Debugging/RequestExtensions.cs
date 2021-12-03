// <copyright file="RequestExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Xml;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace FubarDev.WebDavServer.AspNetCore.Debugging
{
    public static class RequestExtensions
    {
        private static readonly Regex _litmusHeader = new(
            @"^\s*(?<group>[^:]+):\s*(?<index>\d+)\s*\((?<name>[^)]+)\)\s*$",
            RegexOptions.Compiled);

        public static bool IsLitmusTest(this HttpRequest request, string group, int index)
        {
            if (!request.TryGetLitmusHeader(out var header))
            {
                return false;
            }

            return header.Group == group && header.Index == index;
        }

        public static bool TryParseHeader(StringValues values, [NotNullWhen(true)] out LitmusHeader? header)
        {
            if (values.Count != 1)
            {
                header = null;
                return false;
            }

            var value = values[0];
            var match = _litmusHeader.Match(value);
            if (!match.Success)
            {
                header = null;
                return false;
            }

            var group = match.Groups["group"].Value;
            var index = XmlConvert.ToInt32(match.Groups["index"].Value);
            var name = match.Groups["name"].Value;
            header = new LitmusHeader(group, index, name);
            return true;
        }

        private static bool TryGetLitmusHeader(this HttpRequest request, [NotNullWhen(true)] out LitmusHeader? header)
        {
            if (!request.Headers.TryGetValue("X-Litmus", out var values))
            {
                if (!request.Headers.TryGetValue("X-Litmus-Second", out values))
                {
                    header = null;
                    return false;
                }
            }

            return TryParseHeader(values, out header);
        }

        public record LitmusHeader(string Group, int Index, string Name);
    }
}
