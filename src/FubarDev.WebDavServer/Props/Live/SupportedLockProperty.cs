// <copyright file="SupportedLockProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Props.Converters;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props.Live
{
    /// <summary>
    /// The <c>supportedlock</c> property
    /// </summary>
    public class SupportedLockProperty : ITypedReadableProperty<supportedlock>, ILiveProperty
    {
        /// <summary>
        /// The XML name of the property
        /// </summary>
        [NotNull]
        public static readonly XName PropertyName = WebDavXml.Dav + "supportedlock";

        [NotNull]
        private static readonly XmlConverter<supportedlock> _converter = new XmlConverter<supportedlock>();

        [CanBeNull]
        private readonly ILockManager _lockManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportedLockProperty"/> class.
        /// </summary>
        /// <param name="entry">The file system entry this property is for</param>
        public SupportedLockProperty([NotNull] IEntry entry)
        {
            _lockManager = entry.FileSystem.LockManager;
            Cost = 0;
            Name = PropertyName;
        }

        /// <inheritdoc />
        public XName Name { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<XName> AlternativeNames { get; } = new[] { WebDavXml.Dav + "contentlength" };

        /// <inheritdoc />
        public int Cost { get; }

        /// <inheritdoc />
        public Task<bool> IsValidAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public async Task<XElement> GetXmlValueAsync(CancellationToken ct)
        {
            return _converter.ToElement(Name, await GetValueAsync(ct).ConfigureAwait(false));
        }

        /// <inheritdoc />
        public Task<supportedlock> GetValueAsync(CancellationToken ct)
        {
            var result = new supportedlock();

            if (_lockManager != null)
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
