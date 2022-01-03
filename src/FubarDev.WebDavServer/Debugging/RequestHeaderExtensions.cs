// <copyright file="RequestHeaderExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Xml;

using Microsoft.AspNetCore.Http;

namespace FubarDev.WebDavServer.Debugging
{
    public static class RequestHeaderExtensions
    {
        private static readonly Regex _litmusHeader = new(
            @"^\s*(?<group>[^:]+):\s*(?<index>\d+)\s*\((?<name>[^)]+)\)\s*$",
            RegexOptions.Compiled);

        public static bool IsLitmusTest(this IHeaderDictionary requestHeaders, string group, int index)
        {
            if (!requestHeaders.TryGetLitmusHeader(out var header))
            {
                return false;
            }

            return header.Group == group && header.Index == index;
        }

        public static bool IsLitmusTest(this IWebDavRequestHeaders requestHeaders, string group, int index)
        {
            if (!requestHeaders.TryGetLitmusHeader(out var header))
            {
                return false;
            }

            return header.Group == group && header.Index == index;
        }

        public static bool TryParseHeader(IReadOnlyList<string> values, [NotNullWhen(true)] out LitmusHeader? header)
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

        private static bool TryGetLitmusHeader(
            this IHeaderDictionary requestHeaders,
            [NotNullWhen(true)] out LitmusHeader? header)
        {
            if (!requestHeaders.TryGetValue("X-Litmus", out var values))
            {
                if (!requestHeaders.TryGetValue("X-Litmus-Second", out values))
                {
                    header = null;
                    return false;
                }
            }

            return TryParseHeader(values, out header);
        }

        private static bool TryGetLitmusHeader(
            this IWebDavRequestHeaders requestHeaders,
            [NotNullWhen(true)] out LitmusHeader? header)
        {
            if (!requestHeaders.Headers.TryGetValue("X-Litmus", out var values))
            {
                if (!requestHeaders.Headers.TryGetValue("X-Litmus-Second", out values))
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
