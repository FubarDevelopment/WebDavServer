// <copyright file="IHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IHandler
    {
        IEnumerable<string> HttpMethods { get; }
    }
}
