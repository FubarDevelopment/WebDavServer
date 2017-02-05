// <copyright file="IDeleteHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IDeleteHandler : IClass1Handler
    {
        Task<IWebDavResult> DeleteAsync(string path, CancellationToken cancellationToken);
    }
}
