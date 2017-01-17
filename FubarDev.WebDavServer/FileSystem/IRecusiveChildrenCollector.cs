using System.Collections.Generic;

namespace FubarDev.WebDavServer.FileSystem
{
    public interface IRecusiveChildrenCollector
    {
        IAsyncEnumerable<IEntry> GetEntries(int maxDepth);
    }
}
