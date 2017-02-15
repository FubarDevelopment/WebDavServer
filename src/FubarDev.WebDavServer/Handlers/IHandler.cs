// <copyright file="IHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Handlers
{
    public interface IHandler
    {
        [NotNull]
        [ItemNotNull]
        IEnumerable<string> HttpMethods { get; }
    }
}
