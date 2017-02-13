// <copyright file="IWebDavContext.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer
{
    public interface IWebDavContext
    {
        [NotNull]
        string RequestProtocol { get; }

        [NotNull]
        Uri BaseUrl { get; }

        DetectedClient DetectedClient { get; }

        [NotNull]
        IWebDavRequestHeaders RequestHeaders { get; }
    }
}
