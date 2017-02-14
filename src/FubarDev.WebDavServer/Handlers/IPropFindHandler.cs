// <copyright file="IPropFindHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IPropFindHandler : IClass1Handler
    {
        Task<IWebDavResult> PropFindAsync(string path, propfind request, CancellationToken cancellationToken);
    }
}
