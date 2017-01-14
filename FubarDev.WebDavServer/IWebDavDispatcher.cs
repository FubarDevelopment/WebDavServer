using System;
using System.Collections.Generic;

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
        IWebDavClass1 Class1 { get; }
    }
}
