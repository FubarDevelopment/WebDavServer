using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer
{
    public class WebDavResult<T> : IWebDavResult
    {
        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(T));

        public WebDavResult(WebDavStatusCodes statusCode, T data)
        {
            StatusCode = statusCode;
            Data = data;
        }

        public WebDavStatusCodes StatusCode { get; }

        public T Data { get; }

        public virtual Task WriteResponseAsync(Stream stream, CancellationToken ct)
        {
            _serializer.Serialize(stream, Data);
            return Task.FromResult(0);
        }
    }
}
