// <copyright file="MoveRemoteTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.Props.Dead;

namespace FubarDev.WebDavServer.Tests.Handlers
{
    public class MoveRemoteTests : MoveTestsBase
    {
        public MoveRemoteTests()
            : base(RecursiveProcessingMode.PreferCrossServer, GetETagProperty.PropertyName)
        {
        }
    }
}
