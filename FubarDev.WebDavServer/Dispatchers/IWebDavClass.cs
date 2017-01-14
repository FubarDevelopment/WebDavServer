using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Dispatchers
{
    public interface IWebDavClass
    {
        int Version { get; }

        [NotNull]
        [ItemNotNull]
        IEnumerable<string> HttpMethods { get; }
    }
}
