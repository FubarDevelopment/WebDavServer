// <copyright file="InMemorySimpleFsTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.Tests.Support.ServiceBuilders;

namespace FubarDev.WebDavServer.Tests.FileSystem
{
    public class InMemorySimpleFsTests : SimpleFsTests<InMemoryFileSystemServices>
    {
        public InMemorySimpleFsTests(InMemoryFileSystemServices fsServices)
            : base(fsServices)
        {
        }
    }
}
