using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer
{
    public interface IWebDavDispatcher
    {
        IWebDavResult PropFindAsync(string path, Propfind request, Depth depth, CancellationToken cancellationToken);
    }
}
