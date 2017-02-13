// <copyright file="IWebDavClass1.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Dispatchers
{
    public interface IWebDavClass1 : IWebDavClass
    {
        Task<IWebDavResult> OptionsAsync(string path, CancellationToken cancellationToken);

        Task<IWebDavResult> GetAsync(string path, CancellationToken cancellationToken);

        Task<IWebDavResult> HeadAsync(string path, CancellationToken cancellationToken);

        Task<IWebDavResult> PutAsync(string path, Stream data, CancellationToken cancellationToken);

        Task<IWebDavResult> DeleteAsync(string path, CancellationToken cancellationToken);

        Task<IWebDavResult> PropFindAsync(string path, Propfind request, CancellationToken cancellationToken);

        Task<IWebDavResult> PropPatchAsync(string path, Propertyupdate request, CancellationToken cancellationToken);

        Task<IWebDavResult> MkColAsync(string path, CancellationToken cancellationToken);

        Task<IWebDavResult> CopyAsync(string path, Uri destination, CancellationToken cancellationToken);

        Task<IWebDavResult> MoveAsync(string path, Uri destination, CancellationToken cancellationToken);
    }
}
