// <copyright file="ITarget.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines
{
    public interface ITarget
    {
        [NotNull]
        string Name { get; }

        [NotNull]
        Uri DestinationUrl { get; }
    }
}
