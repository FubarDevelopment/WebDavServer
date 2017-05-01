// <copyright file="SQLiteFileSystemTreeCollection.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.Tests.Support.ServiceBuilders;

namespace FubarDev.WebDavServer.Tests.FileSystem
{
    public class SQLiteFileSystemTreeCollection : FileSystemTreeCollection<SQLiteFileSystemServices>
    {
        public SQLiteFileSystemTreeCollection(SQLiteFileSystemServices fsServices)
            : base(fsServices)
        {
        }
    }
}
