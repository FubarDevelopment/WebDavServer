// <copyright file="ContentLengthProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

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

        private readonly GetPropertyValueAsyncDelegate<long> _getPropertyValueAsync;

        public ContentLengthProperty(GetPropertyValueAsyncDelegate<long> getPropertyValueAsync)
        {
            Cost = 0;
            Name = PropertyName;
            _getPropertyValueAsync = getPropertyValueAsync;
        }

        public XName Name { get; }

        public int Cost { get; }

        public async Task<XElement> GetXmlValueAsync(CancellationToken ct)
        {
            return _converter.ToElement(Name, await GetValueAsync(ct).ConfigureAwait(false));
        }

        public Task<long> GetValueAsync(CancellationToken ct)
        {
            return _getPropertyValueAsync(ct);
        }
    }
}
