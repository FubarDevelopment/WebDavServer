using System;
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
