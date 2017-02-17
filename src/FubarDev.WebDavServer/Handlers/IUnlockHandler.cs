// <copyright file="IUnlockHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model.Headers;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IUnlockHandler : IClass1Handler
    {
        [NotNull]
        [ItemNotNull]
        Task<IWebDavResult> UnlockAsync([NotNull] string path, [NotNull] LockTokenHeader stateToken, CancellationToken cancellationToken);
    }
}
