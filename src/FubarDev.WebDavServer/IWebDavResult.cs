// <copyright file="IWebDavResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer
{
    public interface IWebDavResult
    {
        WebDavStatusCode StatusCode { get; }

        [NotNull]
        Task ExecuteResultAsync([NotNull] IWebDavResponse response, CancellationToken ct);
    }
}
