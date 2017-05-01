// <copyright file="DotNetFileSystemTreeCollection.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.Tests.Support.ServiceBuilders;

namespace FubarDev.WebDavServer.Tests.FileSystem
{
    public class DotNetFileSystemTreeCollection : FileSystemTreeCollection<DotNetFileSystemServices>
    {
        public DotNetFileSystemTreeCollection(DotNetFileSystemServices fsServices)
            : base(fsServices)
        {
        }
    }
}
