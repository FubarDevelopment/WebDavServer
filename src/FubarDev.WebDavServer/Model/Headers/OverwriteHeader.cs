// <copyright file="OverwriteHeader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.WebDavServer.Model.Headers
{
    /// <summary>
    /// The <c>Overwrite</c> header specific parsing functions.
    /// </summary>
    public static class OverwriteHeader
    {
        /// <summary>
        /// Parses the header string to get the value of the <c>Overwrite</c> header.
        /// </summary>
        /// <param name="s">The header string to parse.</param>
        /// <returns>The value of the <c>Overwrite</c> header.</returns>
        public static bool? Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return null;
            }

            s = s.Trim();
            if (s == "T")
            {
                return true;
            }

            if (s == "F")
            {
                return false;
            }

            throw new NotSupportedException($"Overwrite value '{s}' isn't supported");
        }
    }
}
