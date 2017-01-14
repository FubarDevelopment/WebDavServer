using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer
{
    public class WebDavResult<T> : WebDavResult
    {
        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(T));

        public WebDavResult(WebDavStatusCodes statusCode, T data)
            : base(statusCode)
        {
            Data = data;
        }

        public T Data { get; }

        public override Task ExecuteResultAsync(IWebDavResponse response, CancellationToken ct)
        {
            response.ContentType = "application/xml; charset=\"utf-8\"";
            _serializer.Serialize(response.Body, Data);
            return Task.FromResult(0);
        }
    }
}
