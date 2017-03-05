// <copyright file="EntryProperties.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Live;
using FubarDev.WebDavServer.Props.Store;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props
{
    /// <summary>
    /// The asynchronously enumerable properties for a <see cref="IEntry"/>
    /// </summary>
    public class EntryProperties : IAsyncEnumerable<IUntypedReadableProperty>
    {
        [NotNull]
        private readonly IEntry _entry;

        [NotNull]
        [ItemNotNull]
        private readonly IEnumerable<IUntypedReadableProperty> _predefinedProperties;

        [CanBeNull]
        private readonly IPropertyStore _propertyStore;

        private readonly int? _maxCost;

        private readonly bool _returnInvalidProperties;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntryProperties"/> class.
        /// </summary>
        /// <param name="entry">The entry whose properties are to enumerate</param>
        /// <param name="predefinedProperties">The predefined properties for the entry</param>
        /// <param name="propertyStore">The property store to get the remaining dead properties for</param>
        /// <param name="maxCost">The maximum cost of the properties to return</param>
        /// <param name="returnInvalidProperties">Do we want to get invalid live properties?</param>
        public EntryProperties(
            [NotNull] IEntry entry,
            [NotNull] [ItemNotNull] IEnumerable<IUntypedReadableProperty> predefinedProperties,
            [CanBeNull] IPropertyStore propertyStore,
            int? maxCost,
            bool returnInvalidProperties)
        {
            _entry = entry;
            _predefinedProperties = predefinedProperties;
            _propertyStore = propertyStore;
            _maxCost = maxCost;
            _returnInvalidProperties = returnInvalidProperties;
        }

        /// <inheritdoc />
        public IAsyncEnumerator<IUntypedReadableProperty> GetEnumerator()
        {
            return new PropertiesEnumerator(_entry, _predefinedProperties, _propertyStore, _maxCost, _returnInvalidProperties);
        }

        private class PropertiesEnumerator : IAsyncEnumerator<IUntypedReadableProperty>
        {
            [NotNull]
            private readonly IEntry _entry;

            [CanBeNull]
            private readonly IPropertyStore _propertyStore;

            private readonly int? _maxCost;

            private readonly bool _returnInvalidProperties;

            [NotNull]
            private readonly IEnumerator<IUntypedReadableProperty> _predefinedPropertiesEnumerator;

            private readonly Dictionary<XName, IUntypedReadableProperty> _emittedProperties = new Dictionary<XName, IUntypedReadableProperty>();

            private bool _predefinedPropertiesFinished;

            [CanBeNull]
            private IEnumerator<IDeadProperty> _deadPropertiesEnumerator;

            public PropertiesEnumerator(
                [NotNull] IEntry entry,
                [NotNull] [ItemNotNull] IEnumerable<IUntypedReadableProperty> predefinedProperties,
                [CanBeNull] IPropertyStore propertyStore,
                int? maxCost,
                bool returnInvalidProperties)
            {
                _entry = entry;
                _propertyStore = propertyStore;
                _maxCost = maxCost;
                _returnInvalidProperties = returnInvalidProperties;

                var emittedProperties = new HashSet<XName>();
                var predefinedPropertiesList = new List<IUntypedReadableProperty>();
                foreach (var property in predefinedProperties)
                {
                    if (emittedProperties.Add(property.Name))
                        predefinedPropertiesList.Add(property);
                }

                _predefinedPropertiesEnumerator = predefinedPropertiesList.GetEnumerator();
            }

            public IUntypedReadableProperty Current { get; private set; }

#pragma warning disable IDE1006 // Benennungsstile
            public async Task<bool> MoveNext(CancellationToken cancellationToken)
#pragma warning restore IDE1006 // Benennungsstile
            {
                while (true)
                {
                    var result = await GetNextPropertyAsync(cancellationToken).ConfigureAwait(false);
                    if (result == null)
                    {
                        Current = null;
                        return false;
                    }

                    IUntypedReadableProperty oldProperty;
                    if (_emittedProperties.TryGetValue(result.Name, out oldProperty))
                    {
                        // Property was already emitted - don't return it again.
                        // The predefined dead properties are reading their values from the property store
                        // themself and don't need to be intialized again.
                        continue;
                    }

                    if (!_returnInvalidProperties)
                    {
                        var liveProp = result as ILiveProperty;
                        if (liveProp != null && !await liveProp.IsValidAsync(cancellationToken).ConfigureAwait(false))
                        {
                            // The properties value is not valid
                            continue;
                        }
                    }

                    _emittedProperties.Add(result.Name, result);
                    Current = result;
                    return true;
                }
            }

            public void Dispose()
            {
                _predefinedPropertiesEnumerator.Dispose();
                _deadPropertiesEnumerator?.Dispose();
            }

            private async Task<IUntypedReadableProperty> GetNextPropertyAsync(CancellationToken cancellationToken)
            {
                if (!_predefinedPropertiesFinished)
                {
                    if (_predefinedPropertiesEnumerator.MoveNext())
                    {
                        return _predefinedPropertiesEnumerator.Current;
                    }

                    _predefinedPropertiesFinished = true;

                    if (_propertyStore == null || (_maxCost.HasValue && _propertyStore.Cost > _maxCost))
                        return null;

                    var deadProperties = await _propertyStore.LoadAsync(_entry, cancellationToken).ConfigureAwait(false);
                    _deadPropertiesEnumerator = deadProperties.GetEnumerator();
                }

                if (_propertyStore == null || (_maxCost.HasValue && _propertyStore.Cost > _maxCost))
                    return null;

                Debug.Assert(_deadPropertiesEnumerator != null, "_deadPropertiesEnumerator != null");
                if (_deadPropertiesEnumerator == null)
                    throw new InvalidOperationException("Internal error: The dead properties enumerator was not initialized");
                if (!_deadPropertiesEnumerator.MoveNext())
                    return null;

                return _deadPropertiesEnumerator.Current;
            }
        }
    }
}
