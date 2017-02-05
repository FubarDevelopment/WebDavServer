// <copyright file="IWebDavDispatcher.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using FubarDev.WebDavServer.Dispatchers;
using FubarDev.WebDavServer.Formatters;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer
{
    public interface IWebDavDispatcher
    {
        [NotNull]
        IReadOnlyCollection<int> SupportedClasses { get; }

        [NotNull]
        [ItemNotNull]
        IReadOnlyCollection<string> SupportedHttpMethods { get; }

        [NotNull]
        IWebDavOutputFormatter Formatter { get; }

        [NotNull]
        IWebDavClass1 Class1 { get; }
    }
}
