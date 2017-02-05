// <copyright file="IPutHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IPutHandler : IClass1Handler
    {
        Task<IWebDavResult> PutAsync(string path, Stream data, CancellationToken cancellationToken);
    }
}
