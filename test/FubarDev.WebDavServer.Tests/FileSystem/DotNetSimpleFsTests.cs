// <copyright file="DotNetSimpleFsTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.Tests.Support.ServiceBuilders;

namespace FubarDev.WebDavServer.Tests.FileSystem
{
    public class DotNetSimpleFsTests : SimpleFsTests<DotNetFileSystemServices>
    {
        public DotNetSimpleFsTests(DotNetFileSystemServices fsServices)
            : base(fsServices)
        {
        }
    }
}
