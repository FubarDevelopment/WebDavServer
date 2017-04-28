// <copyright file="MemoryLockCleanupTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.Tests.Support.ServiceBuilders;

namespace FubarDev.WebDavServer.Tests.Locking
{
    public class MemoryLockCleanupTests : LockCleanupTests<MemoryLockServices>
    {
        public MemoryLockCleanupTests(MemoryLockServices services)
            : base(services)
        {
        }
    }
}
