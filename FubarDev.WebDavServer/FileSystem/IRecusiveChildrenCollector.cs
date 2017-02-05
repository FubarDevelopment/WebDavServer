// <copyright file="IRecusiveChildrenCollector.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace FubarDev.WebDavServer.FileSystem
{
    public interface IRecusiveChildrenCollector
    {
        IAsyncEnumerable<IEntry> GetEntries(int maxDepth);
    }
}
