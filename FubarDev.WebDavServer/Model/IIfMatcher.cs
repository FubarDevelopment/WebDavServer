// <copyright file="IEntryMatcher.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model
{
    public interface IIfMatcher
    {
        bool IsMatch(EntityTag etag, [NotNull] [ItemNotNull] IReadOnlyCollection<Uri> stateTokens);
    }
}
