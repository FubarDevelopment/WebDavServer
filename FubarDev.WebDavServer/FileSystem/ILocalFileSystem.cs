// <copyright file="ILocalFileSystem.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.FileSystem
{
    public interface ILocalFileSystem : IFileSystem
    {
        string RootDirectoryPath { get; }
    }
}
