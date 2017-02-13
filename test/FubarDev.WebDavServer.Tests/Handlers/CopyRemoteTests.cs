// <copyright file="CopyRemoteTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.Props.Dead;

namespace FubarDev.WebDavServer.Tests.Handlers
{
    public class CopyRemoteTests : CopyTestsBase
    {
        public CopyRemoteTests()
            : base(RecursiveProcessingMode.PreferCrossServer, GetETagProperty.PropertyName)
        {
        }
    }
}
