// <copyright file="IWebDavResponse.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.IO;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer
{
    public interface IWebDavResponse
    {
        [NotNull]
        IWebDavDispatcher Dispatcher { get; }

        [NotNull]
        IDictionary<string, string[]> Headers { get; }

        [NotNull]
        string ContentType { get; set; }

        [NotNull]
        Stream Body { get; }
    }
}
