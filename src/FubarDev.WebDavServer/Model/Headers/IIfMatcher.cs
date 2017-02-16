// <copyright file="IIfMatcher.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using FubarDev.WebDavServer.FileSystem;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model.Headers
{
    public interface IIfMatcher
    {
        bool IsMatch([NotNull] IEntry entry, EntityTag etag, [NotNull] [ItemNotNull] IReadOnlyCollection<Uri> stateTokens);
    }
}
