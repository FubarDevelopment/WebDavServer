// <copyright file="EntryTarget.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Props;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Live;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines.Local
{
    /// <summary>
    /// The base class of all existing targets
    /// </summary>
    public abstract class EntryTarget : IExistingTarget
    {
        [NotNull]
        private readonly IEntry _entry;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntryTarget"/> class.
        /// </summary>
        /// <param name="targetActions">The target actions implementation to use</param>
        /// <param name="parent">The parent collection</param>
        /// <param name="destinationUrl">The destination URL for this entry</param>
        /// <param name="entry">The underlying entry</param>
        protected EntryTarget(
            ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> targetActions,
            [CanBeNull] CollectionTarget parent,
            [NotNull] Uri destinationUrl,
            [NotNull] IEntry entry)
        {
            TargetActions = targetActions;
            _entry = entry;
            Name = entry.Name;
            Parent = parent;
            DestinationUrl = destinationUrl;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <summary>
        /// Gets the parent collection target
        /// </summary>
        [CanBeNull]
        public CollectionTarget Parent { get; }

        /// <inheritdoc />
        public Uri DestinationUrl { get; }

        /// <summary>
        /// Gets the target actions implementation to use
        /// </summary>
        [NotNull]
        protected ITargetActions<CollectionTarget, DocumentTarget, MissingTarget> TargetActions { get; }

        /// <inheritdoc />
        [ItemNotNull]
        public async Task<IReadOnlyCollection<XName>> SetPropertiesAsync([NotNull][ItemNotNull] IEnumerable<IUntypedWriteableProperty> properties, CancellationToken cancellationToken)
        {
            var liveProperties = new List<ILiveProperty>();
            var deadProperties = new List<IDeadProperty>();
            foreach (var property in properties)
            {
                var liveProp = property as ILiveProperty;
                if (liveProp != null)
                {
                    liveProperties.Add(liveProp);
                }
                else
                {
                    var deadProp = (IDeadProperty)property;
                    deadProperties.Add(deadProp);
                }
            }

            var livePropertiesResult = await SetPropertiesAsync(liveProperties, cancellationToken).ConfigureAwait(false);

            if (deadProperties.Count != 0)
                await SetPropertiesAsync(deadProperties, cancellationToken).ConfigureAwait(false);

            return livePropertiesResult;
        }

        [NotNull]
        private async Task SetPropertiesAsync([NotNull][ItemNotNull] IEnumerable<IDeadProperty> properties, CancellationToken cancellationToken)
        {
            var propertyStore = _entry.FileSystem.PropertyStore;
            if (propertyStore == null)
                return;

            var elements = new List<XElement>();
            foreach (var property in properties)
            {
                elements.Add(await property.GetXmlValueAsync(cancellationToken).ConfigureAwait(false));
            }

            await propertyStore.SetAsync(_entry, elements, cancellationToken).ConfigureAwait(false);
        }

        [NotNull]
        [ItemNotNull]
        private async Task<IReadOnlyCollection<XName>> SetPropertiesAsync([NotNull][ItemNotNull] IEnumerable<ILiveProperty> properties, CancellationToken cancellationToken)
        {
            var isPropUsed = new Dictionary<XName, bool>();
            var propNameToValue = new Dictionary<XName, XElement>();
            foreach (var property in properties)
            {
                propNameToValue[property.Name] = await property.GetXmlValueAsync(cancellationToken).ConfigureAwait(false);
                isPropUsed[property.Name] = false;
            }

            if (propNameToValue.Count == 0)
            {
                return new XName[0];
            }

            using (var propEnum = _entry.GetProperties(TargetActions.Dispatcher).GetEnumerator())
            {
                while (await propEnum.MoveNext(cancellationToken).ConfigureAwait(false))
                {
                    isPropUsed[propEnum.Current.Name] = true;
                    var prop = propEnum.Current as IUntypedWriteableProperty;
                    XElement propValue;
                    if (prop != null && propNameToValue.TryGetValue(prop.Name, out propValue))
                    {
                        await prop.SetXmlValueAsync(propValue, cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            var hasUnsetLiveProperties = isPropUsed.Any(x => !x.Value);
            if (hasUnsetLiveProperties)
            {
                var unsetPropNames = isPropUsed.Where(x => !x.Value).Select(x => x.Key).ToList();
                return unsetPropNames;
            }

            return new XName[0];
        }
    }
}
