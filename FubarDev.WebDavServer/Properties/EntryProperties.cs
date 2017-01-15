using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Properties
{
    public class EntryProperties : IAsyncEnumerable<IUntypedReadableProperty>
    {
        private readonly IEntry _entry;

        private readonly IEnumerable<IUntypedReadableProperty> _liveProperties;

        private readonly IPropertyStore _propertyStore;

        public EntryProperties(IEntry entry, IEnumerable<IUntypedReadableProperty> liveProperties, IPropertyStore propertyStore)
        {
            _entry = entry;
            _liveProperties = liveProperties;
            _propertyStore = propertyStore;
        }

        public IAsyncEnumerator<IUntypedReadableProperty> GetEnumerator()
        {
            return new PropertiesEnumerator(_entry, _liveProperties, _propertyStore);
        }

        private class PropertiesEnumerator : IAsyncEnumerator<IUntypedReadableProperty>
        {
            private readonly IEntry _entry;

            private readonly IPropertyStore _propertyStore;

            private readonly IEnumerator<IUntypedReadableProperty> _livePropertiesEnumerator;

            private readonly Dictionary<XName, IUntypedReadableProperty> _emittedProperties = new Dictionary<XName, IUntypedReadableProperty>();

            private bool _livePropertiesFinished;

            private IEnumerator<IUntypedReadableProperty> _deadPropertiesEnumerator;

            public PropertiesEnumerator(IEntry entry, IEnumerable<IUntypedReadableProperty> liveProperties, IPropertyStore propertyStore)
            {
                _entry = entry;
                _propertyStore = propertyStore;
                _livePropertiesEnumerator = liveProperties.GetEnumerator();
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
                        var initProp = oldProperty as IInitializableProperty;
                        initProp?.Init(await result.GetXmlValueAsync(cancellationToken).ConfigureAwait(false));
                        continue;
                    }

                    Current = result;
                    return true;
                }
            }

            public void Dispose()
            {
                _livePropertiesEnumerator?.Dispose();
                _deadPropertiesEnumerator?.Dispose();
            }

            private async Task<IUntypedReadableProperty> GetNextPropertyAsync(CancellationToken cancellationToken)
            {
                if (!_livePropertiesFinished)
                {
                    if (_livePropertiesEnumerator.MoveNext())
                    {
                        return _livePropertiesEnumerator.Current;
                    }

                    _livePropertiesFinished = true;

                    if (_propertyStore == null)
                        return null;

                    var deadProperties = await _propertyStore.LoadAndCreateAsync(_entry, cancellationToken).ConfigureAwait(false);
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