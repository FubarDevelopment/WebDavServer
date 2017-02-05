// <copyright file="IWebDavResponse.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.IO;

namespace FubarDev.WebDavServer
{
    public interface IWebDavResponse
    {
        IWebDavDispatcher Dispatcher { get; }

        IDictionary<string, string[]> Headers { get; }

        string ContentType { get; set; }

        Stream Body { get; }
    }
}
