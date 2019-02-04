// <copyright file="IfHeader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using FubarDev.WebDavServer.Utils;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model.Headers
{
    /// <summary>
    /// Class that represents the HTTP <c>If</c> header.
    /// </summary>
    public class IfHeader
    {
        private IfHeader([NotNull] [ItemNotNull] IReadOnlyCollection<IfHeaderList> lists)
        {
            Lists = lists;
        }

        /// <summary>
        /// Gets all condition lists.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public IReadOnlyCollection<IfHeaderList> Lists { get; }

        /// <summary>
        /// Parses the text into a <see cref="IfHeader"/>.
        /// </summary>
        /// <param name="s">The text to parse.</param>
        /// <param name="etagComparer">The comparer to use for entity tag comparison.</param>
        /// <param name="context">The WebDAV request context.</param>
        /// <returns>The new <see cref="IfHeader"/>.</returns>
        [NotNull]
        public static IfHeader Parse([NotNull] string s, [NotNull] EntityTagComparer etagComparer, [NotNull] IWebDavContext context)
        {
            var source = new StringSource(s);
            var lists = IfHeaderList.Parse(source, etagComparer, context).ToList();
            if (!source.Empty)
            {
                throw new ArgumentException("Not an accepted list of conditions", nameof(s));
            }

            return new IfHeader(lists);
        }
    }
}
