using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Properties.Dead;
using FubarDev.WebDavServer.Properties.Live;
using FubarDev.WebDavServer.Properties.Store;

namespace FubarDev.WebDavServer.Properties
{
    public class EntryProperties : IAsyncEnumerable<IUntypedReadableProperty>
    {
        private readonly IEntry _entry;

        private readonly IEnumerable<ILiveProperty> _liveProperties;
        private readonly IEnumerable<IDeadProperty> _predefinedDeadProperties;

        private readonly IPropertyStore _propertyStore;

        public EntryProperties(IEntry entry, IEnumerable<ILiveProperty> liveProperties, IEnumerable<IDeadProperty> predefinedDeadProperties, IPropertyStore propertyStore)
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
            private readonly IEntry _entry;

            private readonly IPropertyStore _propertyStore;

            private readonly IEnumerator<IUntypedReadableProperty> _predefinedPropertiesEnumerator;

            private readonly Dictionary<XName, IUntypedReadableProperty> _emittedProperties = new Dictionary<XName, IUntypedReadableProperty>();

            private bool _predefinedPropertiesFinished;

            private IEnumerator<IDeadProperty> _deadPropertiesEnumerator;

            public PropertiesEnumerator(IEntry entry, IEnumerable<ILiveProperty> liveProperties, IEnumerable<IDeadProperty> predefinedDeadProperties, IPropertyStore propertyStore)
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
                for (;;)
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
                _predefinedPropertiesEnumerator?.Dispose();
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

                if (!_deadPropertiesEnumerator.MoveNext())
                    return null;

                return _deadPropertiesEnumerator.Current;
            }
        }
    }
}