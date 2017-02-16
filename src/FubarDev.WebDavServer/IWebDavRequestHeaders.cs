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
        DepthHeader? Depth { get; }

        bool? Overwrite { get; }

        [CanBeNull]
        IfHeader If { get; }

        [CanBeNull]
        IfMatchHeader IfMatch { get; }

        [CanBeNull]
        IfNoneMatchHeader IfNoneMatch { get; }

        [CanBeNull]
        IfModifiedSinceHeader IfModifiedSince { get; }

        [CanBeNull]
        IfUnmodifiedSinceHeader IfUnmodifiedSince { get; }

        [CanBeNull]
        RangeHeader Range { get; }

        [CanBeNull]
        TimeoutHeader Timeout { get; set; }

        [NotNull]
        IDictionary<string, List<string>> Headers { get; }

        [NotNull]
        [ItemNotNull]
        IReadOnlyCollection<string> this[string name] { get; }
    }
}
