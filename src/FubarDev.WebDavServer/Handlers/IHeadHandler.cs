// <copyright file="IHeadHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IHeadHandler : IClass1Handler
    {
        Task<IWebDavResult> HeadAsync(string path, CancellationToken cancellationToken);
    }
}
