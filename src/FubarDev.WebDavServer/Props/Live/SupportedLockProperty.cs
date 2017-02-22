// <copyright file="SupportedLockProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Props.Converters;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props.Live
{
    public class SupportedLockProperty : ITypedReadableProperty<supportedlock>, ILiveProperty
    {
        [NotNull]
        public static readonly XName PropertyName = WebDavXml.Dav + "supportedlock";

        [NotNull]
        private static readonly XmlConverter<supportedlock> _converter = new XmlConverter<supportedlock>();

        [NotNull]
        private readonly IEntry _entry;

        public SupportedLockProperty([NotNull] IEntry entry)
        {
            _entry = entry;
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

        public Task<supportedlock> GetValueAsync(CancellationToken ct)
        {
            var result = new supportedlock();

            if (_entry.FileSystem.LockManager != null)
            {
                result.lockentry = new[]
                {
                    new supportedlockLockentry()
                    {
                        locktype = new locktype()
                        {
                            Item = new object(),
                        },
                        lockscope = new lockscope()
                        {
                            ItemElementName = ItemChoiceType.shared,
                            Item = new object(),
                        },
                    },
                    new supportedlockLockentry()
                    {
                        locktype = new locktype()
                        {
                            Item = new object(),
                        },
                        lockscope = new lockscope()
                        {
                            ItemElementName = ItemChoiceType.exclusive,
                            Item = new object(),
                        },
                    },
                };
            }

            return Task.FromResult(result);
        }
    }
}
