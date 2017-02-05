// <copyright file="IOptionsHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IOptionsHandler : IClass1Handler
    {
        Task<IWebDavResult> OptionsAsync(string path, CancellationToken cancellationToken);
    }
}
