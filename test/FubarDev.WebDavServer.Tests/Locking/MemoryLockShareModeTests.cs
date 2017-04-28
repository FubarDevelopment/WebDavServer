// <copyright file="MemoryLockShareModeTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.Tests.Support.ServiceBuilders;

using Xunit.Abstractions;

namespace FubarDev.WebDavServer.Tests.Locking
{
    public class MemoryLockShareModeTests : LockShareModeTests<MemoryLockServices>
    {
        public MemoryLockShareModeTests(MemoryLockServices services, ITestOutputHelper output)
            : base(services, output)
        {
        }
    }
}
