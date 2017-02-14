// <copyright file="IWebDavRequestHeaders.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer
{
    public interface IWebDavRequestHeaders
    {
        Model.Depth? Depth { get; }

        bool? Overwrite { get; }

        [CanBeNull]
        Model.If If { get; }

        [CanBeNull]
        Model.IfMatch IfMatch { get; }

        [CanBeNull]
        Model.IfNoneMatch IfNoneMatch { get; }

        [CanBeNull]
        Model.IfModifiedSince IfModifiedSince { get; }

        [CanBeNull]
        Model.IfUnmodifiedSince IfUnmodifiedSince { get; }

        [CanBeNull]
        Model.Range Range { get; }

        [CanBeNull]
        Model.Timeout Timeout { get; set; }

        [NotNull]
        IDictionary<string, List<string>> Headers { get; }

        [NotNull]
        [ItemNotNull]
        IReadOnlyCollection<string> this[string name] { get; }
    }
}
