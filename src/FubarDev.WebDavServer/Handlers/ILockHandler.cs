// <copyright file="ILockHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Handlers
{
    public interface ILockHandler : IClass1Handler
    {
        Task<IWebDavResult> LockAsync(string path, lockinfo info, CancellationToken cancellationToken);
    }
}
