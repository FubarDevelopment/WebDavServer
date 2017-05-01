// <copyright file="SQLiteSimpleFsTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.Tests.Support.ServiceBuilders;

namespace FubarDev.WebDavServer.Tests.FileSystem
{
    public class SQLiteSimpleFsTests : SimpleFsTests<SQLiteFileSystemServices>
    {
        public SQLiteSimpleFsTests(SQLiteFileSystemServices fsServices)
            : base(fsServices)
        {
        }
    }
}
