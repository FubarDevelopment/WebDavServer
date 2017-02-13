// <copyright file="IFileSystem.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Props.Store;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem
{
    public interface IFileSystem
    {
        [NotNull]
        AsyncLazy<ICollection> Root { get; }

        [CanBeNull]
        IPropertyStore PropertyStore { get; }

        [NotNull]
        [ItemNotNull]
        Task<SelectionResult> SelectAsync([NotNull] string path, CancellationToken ct);
    }
}
