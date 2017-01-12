using System.Net;

namespace FubarDev.WebDavServer.Model
{
    public enum WebDavStatusCodes
    {
        OK = HttpStatusCode.OK,
        BadRequest = HttpStatusCode.BadRequest,
        MultiStatus = 207,
        Forbidden = HttpStatusCode.Forbidden,
        NotImplemented = HttpStatusCode.NotImplemented,
        NotFound = HttpStatusCode.NotFound,
    }
}
