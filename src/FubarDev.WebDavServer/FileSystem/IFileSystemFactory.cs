// <copyright file="IFileSystemFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Security.Principal;

namespace FubarDev.WebDavServer.FileSystem
{
    public interface IFileSystemFactory
    {
        IFileSystem CreateFileSystem(IIdentity identity);
    }
}
