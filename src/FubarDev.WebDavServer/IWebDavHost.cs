// <copyright file="IWebDavHost.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.WebDavServer
{
    public interface IWebDavHost
    {
        string RequestProtocol { get; }

        Uri BaseUrl { get; }

        DetectedClient DetectedClient { get; }
    }
}
