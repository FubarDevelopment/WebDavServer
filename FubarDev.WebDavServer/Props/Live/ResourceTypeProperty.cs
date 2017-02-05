// <copyright file="ResourceTypeProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Props.Live
{
    public abstract class ResourceTypeProperty : ILiveProperty
    {
        public static readonly XName PropertyName = WebDavXml.Dav + "resourcetype";

        private readonly XElement _element;

        protected ResourceTypeProperty(XElement element)
        {
            _element = element;
        }

        public XName Name { get; } = PropertyName;

        public int Cost { get; } = 0;

        public static ResourceTypeProperty GetDocumentResourceType()
            => new DocumentResourceType();

        public static ResourceTypeProperty GetCollectionResourceType()
            => new CollectionResourceType();

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
