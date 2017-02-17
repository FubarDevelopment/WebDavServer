// <copyright file="CopyCrossFileSystemsTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.Props.Dead;

namespace FubarDev.WebDavServer.Tests.Handlers
{
    public class CopyCrossFileSystemsTests : CopyTestsBase
    {
        public CopyCrossFileSystemsTests()
            : base(RecursiveProcessingMode.PreferCrossFileSystem, GetETagProperty.PropertyName)
        {
        }
    }
}
