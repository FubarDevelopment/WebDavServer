// <copyright file="IWebDavContext.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.WebDavServer.Utils.UAParser;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer
{
    public interface IWebDavContext
    {
        [NotNull]
        string RequestProtocol { get; }

        [NotNull]
        Uri BaseUrl { get; }

        [NotNull]
        Uri RootUrl { get; }

        [NotNull]
        IUAParserOutput DetectedClient { get; }

        [NotNull]
        IWebDavRequestHeaders RequestHeaders { get; }
    }
}
