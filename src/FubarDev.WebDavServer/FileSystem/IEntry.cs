// <copyright file="IEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Props;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem
{
    public interface IEntry
    {
        [NotNull]
        string Name { get; }

        [NotNull]
        IFileSystem RootFileSystem { get; }

        [NotNull]
        IFileSystem FileSystem { get; }

        [CanBeNull]
        ICollection Parent { get; }

        [NotNull]
        Uri Path { get; }

        DateTime LastWriteTimeUtc { get; }

        DateTime CreationTimeUtc { get; }

        [NotNull]
        IAsyncEnumerable<IUntypedReadableProperty> GetProperties(int? maxCost = null);

        [NotNull]
        [ItemNotNull]
        Task<DeleteResult> DeleteAsync(CancellationToken cancellationToken);
    }
}
