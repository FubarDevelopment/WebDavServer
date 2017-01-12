namespace FubarDev.WebDavServer
{
    public class WebDavServer : IWebDavDispatcher
    {
        public WebDavServer(IWebDavClass1 webDavClass1)
        {
            Class1 = webDavClass1;
        }

        public IWebDavClass1 Class1 { get; }
    }
}
