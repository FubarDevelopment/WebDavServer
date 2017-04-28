// <copyright file="SQLiteLockCleanupTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.Tests.Support.ServiceBuilders;

namespace FubarDev.WebDavServer.Tests.Locking
{
    public class SQLiteLockCleanupTests : LockCleanupTests<SQLiteLockServices>
    {
        public SQLiteLockCleanupTests(SQLiteLockServices services)
            : base(services)
        {
        }
    }
}
