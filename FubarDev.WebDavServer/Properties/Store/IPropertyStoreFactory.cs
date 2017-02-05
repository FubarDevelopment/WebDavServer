// <copyright file="IPropertyStoreFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Properties.Store
{
    public interface IPropertyStoreFactory
    {
        IPropertyStore Create(IFileSystem fileSystem);
    }
}
