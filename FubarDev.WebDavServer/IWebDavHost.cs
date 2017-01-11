using System;

namespace FubarDev.WebDavServer
{
    public interface IWebDavHost
    {
        Uri BaseUrl { get; }
    }
}
