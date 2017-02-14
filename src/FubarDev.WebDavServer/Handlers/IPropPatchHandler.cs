// <copyright file="IPropPatchHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IPropPatchHandler : IClass1Handler
    {
        Task<IWebDavResult> PropPatchAsync(string path, propertyupdate request, CancellationToken cancellationToken);
    }
}
