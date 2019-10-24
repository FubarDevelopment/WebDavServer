// <copyright file="EntryProperties.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Live;
using FubarDev.WebDavServer.Props.Store;

namespace FubarDev.WebDavServer.Props
{
    /// <summary>
    /// The asynchronously enumerable properties for a <see cref="IEntry"/>.
    /// </summary>
    public class EntryProperties : IAsyncEnumerable<IUntypedReadableProperty>
    {
        private readonly IEntry _entry;
        private readonly IEnumerable<IUntypedReadableProperty> _predefinedProperties;
        private readonly IPropertyStore? _propertyStore;
        private readonly Predicate<IUntypedReadableProperty>? _predicate;
        private readonly bool _returnInvalidProperties;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntryProperties"/> class.
        /// </summary>
        /// <param name="entry">The entry whose properties are to enumerate.</param>
        /// <param name="predefinedProperties">The predefined properties for the entry.</param>
        /// <param name="propertyStore">The property store to get the remaining dead properties for.</param>
        /// <param name="predicate">A predicate used to filter the returned properties.</param>
        /// <param name="returnInvalidProperties">Indicates whether we want to get invalid live properties.</param>
        public EntryProperties(
            IEntry entry,
            IEnumerable<IUntypedReadableProperty> predefinedProperties,
            IPropertyStore? propertyStore,
            Predicate<IUntypedReadableProperty>? predicate,
            bool returnInvalidProperties)
        {
            _entry = entry;
            _predefinedProperties = predefinedProperties;
            _propertyStore = propertyStore;
            _predicate = predicate;
            _returnInvalidProperties = returnInvalidProperties;
        }

        /// <inheritdoc />
        public IAsyncEnumerator<IUntypedReadableProperty> GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            return new PropertiesEnumerator(
                _entry,
                _predefinedProperties,
                _propertyStore,
                _predicate,
                _returnInvalidProperties,
                cancellationToken);
        }

        private class PropertiesEnumerator : IAsyncEnumerator<IUntypedReadableProperty>
        {
            private readonly IEntry _entry;

            private readonly IPropertyStore? _propertyStore;

            private readonly Predicate<IUntypedReadableProperty>? _predicate;

            private readonly bool _returnInvalidProperties;
            private readonly CancellationToken _cancellationToken;

            private readonly IEnumerator<IUntypedReadableProperty> _predefinedPropertiesEnumerator;

            private readonly Dictionary<XName, IUntypedReadableProperty> _emittedProperties = new Dictionary<XName, IUntypedReadableProperty>();

            private bool _predefinedPropertiesFinished;

            private IEnumerator<IDeadProperty>? _deadPropertiesEnumerator;
            private IUntypedReadableProperty? _current;

            public PropertiesEnumerator(
                IEntry entry,
                IEnumerable<IUntypedReadableProperty> predefinedProperties,
                IPropertyStore? propertyStore,
                Predicate<IUntypedReadableProperty>? predicate,
                bool returnInvalidProperties,
                CancellationToken cancellationToken)
            {
                _entry = entry;
                _propertyStore = propertyStore;
                _predicate = predicate;
                _returnInvalidProperties = returnInvalidProperties;
                _cancellationToken = cancellationToken;

                var emittedProperties = new HashSet<XName>();
                var predefinedPropertiesList = new List<IUntypedReadableProperty>();
                foreach (var property in predefinedProperties)
                {
                    if (emittedProperties.Add(property.Name))
                        predefinedPropertiesList.Add(property);
                }

                _predefinedPropertiesEnumerator = predefinedPropertiesList.GetEnumerator();
            }

            public IUntypedReadableProperty Current =>
                _current ?? throw new InvalidOperationException("Enumeration has not been started yet.");

#pragma warning disable IDE1006 // Benennungsstile
            public async ValueTask<bool> MoveNextAsync()
#pragma warning restore IDE1006 // Benennungsstile
            {
                while (true)
                {
                    var result = await GetNextPropertyAsync(_cancellationToken).ConfigureAwait(false);
                    if (result == null)
                    {
                        _current = null;
                        return false;
                    }

                    if (_emittedProperties.TryGetValue(result.Name, out _))
                    {
                        // Property was already emitted - don't return it again.
                        // The predefined dead properties are reading their values from the property store
                        // themself and don't need to be initialized again.
                        continue;
                    }

                    if (!(_predicate?.Invoke(result) ?? true))
                    {
                        // Caller doesn't want this property.
                        continue;
                    }

                    if (!_returnInvalidProperties)
                    {
                        if (result is ILiveProperty liveProp)
                        {
                            if (!await liveProp.IsValidAsync(_cancellationToken).ConfigureAwait(false))
                            {
                                // The properties value is not valid
                                continue;
                            }
                        }
                    }

                    _emittedProperties.Add(result.Name, result);
                    _current = result;
                    return true;
                }
            }

            public ValueTask DisposeAsync()
            {
                _predefinedPropertiesEnumerator.Dispose();
                _deadPropertiesEnumerator?.Dispose();
                return default;
            }

            private async Task<IUntypedReadableProperty?> GetNextPropertyAsync(CancellationToken cancellationToken)
            {
                if (!_predefinedPropertiesFinished)
                {
                    if (_predefinedPropertiesEnumerator.MoveNext())
                    {
                        return _predefinedPropertiesEnumerator.Current;
                    }

                    _predefinedPropertiesFinished = true;

                    if (_propertyStore == null)
                    {
                        _deadPropertiesEnumerator = Enumerable.Empty<IDeadProperty>().GetEnumerator();
                    }
                    else
                    {
                        var deadProperties =
                            await _propertyStore.LoadAsync(_entry, cancellationToken).ConfigureAwait(false);
                        _deadPropertiesEnumerator = deadProperties.GetEnumerator();
                    }
                }

                Debug.Assert(_deadPropertiesEnumerator != null, "_deadPropertiesEnumerator != null");
                if (_deadPropertiesEnumerator == null)
                {
                    throw new InvalidOperationException("Internal error: The dead properties enumerator was not initialized");
                }

                if (!_deadPropertiesEnumerator.MoveNext())
                {
                    return null;
                }

                return _deadPropertiesEnumerator.Current;
            }
        }
    }
}
