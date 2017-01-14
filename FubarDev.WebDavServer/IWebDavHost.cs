using System;

namespace FubarDev.WebDavServer
{
    public interface IWebDavHost
    {
        string RequestProtocol { get; }

        Uri BaseUrl { get; }
    }
}
