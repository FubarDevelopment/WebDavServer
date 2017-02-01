using System;

namespace FubarDev.WebDavServer.Tests.Support
{
    public class TestHost : IWebDavHost
    {
        public string RequestProtocol { get; } = "http";
        public Uri BaseUrl { get; } = new Uri("http://localhost/");
        public DetectedClient DetectedClient { get; } = DetectedClient.Any;
    }
}
