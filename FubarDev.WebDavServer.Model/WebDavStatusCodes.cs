using System.Net;

namespace FubarDev.WebDavServer.Model
{
    public enum WebDavStatusCodes
    {
        OK = HttpStatusCode.OK,
        MultiStatus = 207,
        Forbidden = HttpStatusCode.Forbidden,
        NotImplemented = HttpStatusCode.NotImplemented,
    }
}
