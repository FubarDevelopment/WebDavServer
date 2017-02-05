// <copyright file="IMkColHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IMkColHandler : IClass1Handler
    {
        Task<IWebDavResult> MkColAsync(string path, CancellationToken cancellationToken);
    }
}
