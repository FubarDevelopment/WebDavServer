// <copyright file="MoveCrossFileSystemsTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.Props.Dead;

namespace FubarDev.WebDavServer.Tests.Handlers
{
    public class MoveCrossFileSystemsTests : MoveTestsBase
    {
        public MoveCrossFileSystemsTests()
            : base(RecursiveProcessingMode.PreferCrossFileSystem, GetETagProperty.PropertyName)
        {
        }
    }
}
