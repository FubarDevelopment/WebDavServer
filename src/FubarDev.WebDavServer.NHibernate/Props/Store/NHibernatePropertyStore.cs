// <copyright file="NHibernatePropertyStore.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model.Headers;
using FubarDev.WebDavServer.NHibernate.FileSystem;
using FubarDev.WebDavServer.NHibernate.Models;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Store;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NHibernate;

namespace FubarDev.WebDavServer.NHibernate.Props.Store
{
    /// <summary>
    /// The in-memory implementation of a property store
    /// </summary>
    public class NHibernatePropertyStore : PropertyStoreBase
    {
        [NotNull]
        private readonly ISession _session;

        [NotNull]
        private readonly ILogger<NHibernatePropertyStore> _logger;

        [NotNull]
        private readonly NHibernatePropertyStoreOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="NHibernatePropertyStore"/> class.
        /// </summary>
        /// <param name="deadPropertyFactory">The factory to create dead properties</param>
        /// <param name="session">The NHibernate session</param>
        /// <param name="options">The options for this property store</param>
        /// <param name="logger">The logger</param>
        public NHibernatePropertyStore([NotNull] IDeadPropertyFactory deadPropertyFactory, [NotNull] ISession session, [CanBeNull] IOptions<NHibernatePropertyStoreOptions> options, [NotNull] ILogger<NHibernatePropertyStore> logger)
            : base(deadPropertyFactory)
        {
            _options = options?.Value ?? new NHibernatePropertyStoreOptions();
            _session = session;
            _logger = logger;
        }

        /// <inheritdoc />
        public override int Cost => _options.EstimatedCost;

        /// <inheritdoc />
        public override Task<IReadOnlyCollection<XElement>> GetAsync(IEntry entry, CancellationToken cancellationToken)
        {
            var nhEntry = (NHibernateEntry)entry;
            var info = nhEntry.Info;

            var entries = info.Properties.Values.Select(x => XElement.Parse(x.Value)).ToList();
            return Task.FromResult<IReadOnlyCollection<XElement>>(entries);
        }

        /// <inheritdoc />
        public override Task SetAsync(IEntry entry, IEnumerable<XElement> elements, CancellationToken cancellationToken)
        {
            var elementsToSet = new List<XElement>();
            foreach (var element in elements)
            {
                if (element.Name == GetETagProperty.PropertyName)
                {
                    _logger.LogWarning("The ETag property must not be set using the property store.");
                    continue;
                }

                elementsToSet.Add(element);
            }

            return SetAllAsync(entry, elementsToSet, cancellationToken);
        }

        /// <inheritdoc />
        public override async Task RemoveAsync(IEntry entry, CancellationToken cancellationToken)
        {
            var nhEntry = (NHibernateEntry)entry;
            var info = nhEntry.Info;

            await _session.CreateQuery("delete PropertyEntry pe where pe.Entry.Id=?")
                .SetParameter(0, info.Id)
                .ExecuteUpdateAsync(cancellationToken)
                .ConfigureAwait(false);
            info.Properties.Clear();
        }

        /// <inheritdoc />
        public override async Task<IReadOnlyCollection<bool>> RemoveAsync(IEntry entry, IEnumerable<XName> keys, CancellationToken cancellationToken)
        {
            var nhEntry = (NHibernateEntry)entry;
            var info = nhEntry.Info;

            using (var trans = _session.BeginTransaction())
            {
                var result = new List<bool>();
                foreach (var key in keys)
                {
                    var propKey = key.ToString();
                    if (!info.Properties.TryGetValue(propKey, out var propertyEntry))
                    {
                        _logger.LogWarning("The property {name} was not found.", key);
                        result.Add(false);
                        continue;
                    }

                    if (key == GetETagProperty.PropertyName)
                    {
                        _logger.LogWarning("The ETag property must not be set using the property store.");
                        result.Add(false);
                    }
                    else
                    {
                        propertyEntry.Entry = null;
                        info.Properties.Remove(propKey);
                        await _session.DeleteAsync(propertyEntry, cancellationToken)
                            .ConfigureAwait(false);
                        result.Add(true);
                    }
                }

                await trans.CommitAsync(cancellationToken).ConfigureAwait(false);

                return result;
            }
        }

        /// <inheritdoc />
        protected override Task<EntityTag> GetDeadETagAsync(IEntry entry, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("This environment supports ETags natively");
        }

        /// <inheritdoc />
        protected override Task<EntityTag> UpdateDeadETagAsync(IEntry entry, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("This environment supports ETags natively");
        }

        private async Task SetAllAsync(IEntry entry, IEnumerable<XElement> elements, CancellationToken cancellationToken)
        {
            var nhEntry = (NHibernateEntry)entry;
            var info = nhEntry.Info;

            using (var trans = _session.BeginTransaction())
            {
                foreach (var element in elements)
                {
                    if (element.Name == GetETagProperty.PropertyName)
                    {
                        _logger.LogWarning("The ETag property must not be set using the property store.");
                        continue;
                    }

                    var name = element.Name.ToString();
                    var item = new PropertyEntry()
                    {
                        Id = Guid.NewGuid(),
                        XmlName = name,
                        Language = element.Attribute(XNamespace.Xml + "lang")?.Value,
                        Value = element.ToString(SaveOptions.OmitDuplicateNamespaces),
                        Entry = info,
                    };

                    info.Properties.Add(name, item);

                    await _session.SaveAsync(item, cancellationToken)
                        .ConfigureAwait(false);
                }

                await _session.UpdateAsync(info, cancellationToken)
                    .ConfigureAwait(false);

                await trans.CommitAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}
