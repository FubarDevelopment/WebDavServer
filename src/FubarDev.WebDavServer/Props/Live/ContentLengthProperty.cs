// <copyright file="ContentLengthProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Props.Converters;

namespace FubarDev.WebDavServer.Props.Live
{
    public class ContentLengthProperty : ITypedReadableProperty<long>, ILiveProperty
    {
        public static readonly XName PropertyName = WebDavXml.Dav + "getcontentlength";

        private static readonly LongConverter _converter = new LongConverter();

        private readonly long _propValue;

        public ContentLengthProperty(long propValue)
        {
            _propValue = propValue;
            Cost = 0;
            Name = PropertyName;
        }

        public XName Name { get; }

        public IReadOnlyCollection<XName> AlternativeNames { get; } = new[] { WebDavXml.Dav + "contentlength" };

        public int Cost { get; }

        public async Task<XElement> GetXmlValueAsync(CancellationToken ct)
        {
            return _converter.ToElement(Name, await GetValueAsync(ct).ConfigureAwait(false));
        }

        public Task<long> GetValueAsync(CancellationToken ct)
        {
            return Task.FromResult(_propValue);
        }
    }
}
