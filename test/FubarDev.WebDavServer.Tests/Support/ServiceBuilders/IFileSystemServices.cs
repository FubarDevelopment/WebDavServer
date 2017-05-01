// <copyright file="IFileSystemServices.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.WebDavServer.Tests.Support.ServiceBuilders
{
    public interface IFileSystemServices
    {
        IServiceProvider ServiceProvider { get; }
    }
}
