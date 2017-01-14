using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Properties.Default
{
    public abstract class ResourceType : IUntypedReadableProperty
    {
        private readonly XElement _element;

        protected ResourceType(XElement element)
        {
            _element = element;
        }

        public static ResourceType Document { get; } = new DocumentResourceType();

        public static ResourceType Collection { get; } = new CollectionResourceType();

        public XName Name { get; } = WebDavXml.Dav + "resourcetype";

        public int Cost { get; } = 0;

        public Task<XElement> GetXmlValueAsync(CancellationToken ct)
        {
            var result = new XElement(Name);
            if (_element != null)
                result.Add(_element);
            return Task.FromResult(result);
        }

        private class DocumentResourceType : ResourceType
        {
            public DocumentResourceType()
                : base(null)
            {
            }
        }

        private class CollectionResourceType : ResourceType
        {
            public CollectionResourceType()
                : base(new XElement(WebDavXml.Dav + "collection"))
            {
            }
        }
    }
}
