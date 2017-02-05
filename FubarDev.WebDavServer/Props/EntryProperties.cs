// <copyright file="EntryProperties.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

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

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props
{
    public class EntryProperties : IAsyncEnumerable<IUntypedReadableProperty>
    {
        [NotNull]
        private readonly IEntry _entry;

        [NotNull]
        [ItemNotNull]
        private readonly IEnumerable<ILiveProperty> _liveProperties;

        [NotNull]
        [ItemNotNull]
        private readonly IEnumerable<IDeadProperty> _predefinedDeadProperties;

        [CanBeNull]
        private readonly IPropertyStore _propertyStore;

        public EntryProperties(
            [NotNull] IEntry entry,
            [NotNull] [ItemNotNull] IEnumerable<ILiveProperty> liveProperties,
            [NotNull] [ItemNotNull] IEnumerable<IDeadProperty> predefinedDeadProperties,
            [CanBeNull] IPropertyStore propertyStore)
        {
            _entry = entry;
            _liveProperties = liveProperties;
            _predefinedDeadProperties = predefinedDeadProperties;
            _propertyStore = propertyStore;
        }

        public IAsyncEnumerator<IUntypedReadableProperty> GetEnumerator()
        {
            return new PropertiesEnumerator(_entry, _liveProperties, _predefinedDeadProperties, _propertyStore);
        }

        private class PropertiesEnumerator : IAsyncEnumerator<IUntypedReadableProperty>
        {
            [NotNull]
            private readonly IEntry _entry;

            [CanBeNull]
            private readonly IPropertyStore _propertyStore;

            [NotNull]
            private readonly IEnumerator<IUntypedReadableProperty> _predefinedPropertiesEnumerator;

            private readonly Dictionary<XName, IUntypedReadableProperty> _emittedProperties = new Dictionary<XName, IUntypedReadableProperty>();

            private bool _predefinedPropertiesFinished;

            [CanBeNull]
            private IEnumerator<IDeadProperty> _deadPropertiesEnumerator;

            public PropertiesEnumerator(
                [NotNull] IEntry entry,
                [NotNull] [ItemNotNull] IEnumerable<ILiveProperty> liveProperties,
                [NotNull] [ItemNotNull] IEnumerable<IDeadProperty> predefinedDeadProperties,
                [CanBeNull] IPropertyStore propertyStore)
            {
                _entry = entry;
                _propertyStore = propertyStore;
                _predefinedPropertiesEnumerator = liveProperties.Cast<IUntypedReadableProperty>().Concat(predefinedDeadProperties).GetEnumerator();
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
                        var deadProp = oldProperty as IDeadProperty;
                        deadProp?.Init(await result.GetXmlValueAsync(cancellationToken).ConfigureAwait(false));
                        continue;
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

                    if (_propertyStore == null)
                        return null;

                    var deadProperties = await _propertyStore.LoadAsync(_entry, cancellationToken).ConfigureAwait(false);
                    _deadPropertiesEnumerator = deadProperties.GetEnumerator();
                }

                if (_propertyStore == null)
                    return null;

                Debug.Assert(_deadPropertiesEnumerator != null, "_deadPropertiesEnumerator != null");
                if (!_deadPropertiesEnumerator.MoveNext())
                    return null;

                return _deadPropertiesEnumerator.Current;
            }
        }
    }
}
