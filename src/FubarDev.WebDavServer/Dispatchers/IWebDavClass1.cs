// <copyright file="IWebDavClass1.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Model.Headers;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Dispatchers
{
    public interface IWebDavClass1 : IWebDavClass
    {
        [NotNull]
        [ItemNotNull]
        Task<IWebDavResult> OptionsAsync([NotNull] string path, CancellationToken cancellationToken);

        [NotNull]
        [ItemNotNull]
        Task<IWebDavResult> GetAsync([NotNull] string path, CancellationToken cancellationToken);

        [NotNull]
        [ItemNotNull]
        Task<IWebDavResult> HeadAsync([NotNull] string path, CancellationToken cancellationToken);

        [NotNull]
        [ItemNotNull]
        Task<IWebDavResult> PutAsync([NotNull] string path, [NotNull] Stream data, CancellationToken cancellationToken);

        [NotNull]
        [ItemNotNull]
        Task<IWebDavResult> DeleteAsync([NotNull] string path, CancellationToken cancellationToken);

        [NotNull]
        [ItemNotNull]
        Task<IWebDavResult> PropFindAsync([NotNull] string path, [CanBeNull] propfind request, CancellationToken cancellationToken);

        [NotNull]
        [ItemNotNull]
        Task<IWebDavResult> PropPatchAsync([NotNull] string path, [NotNull] propertyupdate request, CancellationToken cancellationToken);

        [NotNull]
        [ItemNotNull]
        Task<IWebDavResult> MkColAsync([NotNull] string path, CancellationToken cancellationToken);

        [NotNull]
        [ItemNotNull]
        Task<IWebDavResult> CopyAsync([NotNull] string path, [NotNull] Uri destination, CancellationToken cancellationToken);

        [NotNull]
        [ItemNotNull]
        Task<IWebDavResult> MoveAsync([NotNull] string path, [NotNull] Uri destination, CancellationToken cancellationToken);
    }
}
