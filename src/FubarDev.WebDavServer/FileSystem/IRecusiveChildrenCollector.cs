// <copyright file="IRecusiveChildrenCollector.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem
{
    public interface IRecusiveChildrenCollector
    {
        [NotNull]
        IAsyncEnumerable<IEntry> GetEntries(int maxDepth);
    }
}
