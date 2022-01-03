// <copyright file="SQLitePropTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.Tests.Support.ServiceBuilders;

namespace FubarDev.WebDavServer.Tests.PropertyStore
{
    public class SQLitePropTests : SimplePropTests<SQLiteFileSystemAndPropertyServices>
    {
        public SQLitePropTests(SQLiteFileSystemAndPropertyServices fsServices)
            : base(fsServices)
        {
        }
    }
}
