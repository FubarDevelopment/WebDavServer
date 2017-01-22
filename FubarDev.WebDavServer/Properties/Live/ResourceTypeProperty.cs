using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Properties.Live
{
    public abstract class ResourceTypeProperty : ILiveProperty
    {
        public static readonly XName PropertyName = WebDavXml.Dav + "resourcetype";

        private readonly XElement _element;

        protected ResourceTypeProperty(XElement element)
        {
            _element = element;
        }

        public static ResourceTypeProperty Document { get; } = new DocumentResourceType();

        public static ResourceTypeProperty Collection { get; } = new CollectionResourceType();

        public XName Name { get; } = PropertyName;

        public int Cost { get; } = 0;

        public Task<XElement> GetXmlValueAsync(CancellationToken ct)
        {
            var result = new XElement(Name);
            if (_element != null)
                result.Add(_element);
            return Task.FromResult(result);
        }

        private class DocumentResourceType : ResourceTypeProperty
        {
            public DocumentResourceType()
                : base(null)
            {
            }
        }

        private class CollectionResourceType : ResourceTypeProperty
        {
            public CollectionResourceType()
                : base(new XElement(WebDavXml.Dav + "collection"))
            {
            }
        }
    }
}
