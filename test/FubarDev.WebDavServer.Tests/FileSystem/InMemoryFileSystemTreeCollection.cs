// <copyright file="InMemoryFileSystemTreeCollection.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.Tests.Support.ServiceBuilders;

namespace FubarDev.WebDavServer.Tests.FileSystem
{
    public class InMemoryFileSystemTreeCollection : FileSystemTreeCollection<InMemoryFileSystemServices>
    {
        public InMemoryFileSystemTreeCollection(InMemoryFileSystemServices fsServices)
            : base(fsServices)
        {
        }
    }
}
