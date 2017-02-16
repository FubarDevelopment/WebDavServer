// <copyright file="IWebDavRequestHeaders.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using FubarDev.WebDavServer.Model.Headers;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer
{
    public interface IWebDavRequestHeaders
    {
        Depth? Depth { get; }

        bool? Overwrite { get; }

        [CanBeNull]
        If If { get; }

        [CanBeNull]
        IfMatch IfMatch { get; }

        [CanBeNull]
        IfNoneMatch IfNoneMatch { get; }

        [CanBeNull]
        IfModifiedSince IfModifiedSince { get; }

        [CanBeNull]
        IfUnmodifiedSince IfUnmodifiedSince { get; }

        [CanBeNull]
        Range Range { get; }

        [CanBeNull]
        Timeout Timeout { get; set; }

        [NotNull]
        IDictionary<string, List<string>> Headers { get; }

        [NotNull]
        [ItemNotNull]
        IReadOnlyCollection<string> this[string name] { get; }
    }
}
