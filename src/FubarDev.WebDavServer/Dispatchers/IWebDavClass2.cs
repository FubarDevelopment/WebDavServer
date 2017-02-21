// <copyright file="IWebDavClass2.cs" company="Fubar Development Junker">
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
    public interface IWebDavClass2 : IWebDavClass
    {
        [NotNull]
        [ItemNotNull]
        Task<IWebDavResult> LockAsync([NotNull] string path, [NotNull] lockinfo info, CancellationToken cancellationToken);

        [NotNull]
        [ItemNotNull]
        Task<IWebDavResult> RefreshLockAsync([NotNull] string path, [NotNull] IfHeader ifHeader, [CanBeNull] TimeoutHeader timeoutHeader, CancellationToken cancellationToken);

        [NotNull]
        [ItemNotNull]
        Task<IWebDavResult> UnlockAsync([NotNull] string path, [NotNull] LockTokenHeader stateToken, CancellationToken cancellationToken);
    }
}
