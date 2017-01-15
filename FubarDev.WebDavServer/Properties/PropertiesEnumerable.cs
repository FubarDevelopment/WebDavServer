using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Properties.Store;

namespace FubarDev.WebDavServer.Properties
{
    public class PropertiesEnumerable : IAsyncEnumerable<IProperty>
    {
        private readonly IEntry _entry;

        private readonly IReadOnlyCollection<IProperty> _liveProperties;

        private readonly IPropertyStore _propertyStore;

        public PropertiesEnumerable(IEntry entry, IReadOnlyCollection<IProperty> liveProperties, IPropertyStore propertyStore)
        {
            _entry = entry;
            _liveProperties = liveProperties;
            _propertyStore = propertyStore;
        }

        public IAsyncEnumerator<IProperty> GetEnumerator()
        {
            return new PropertiesEnumerator(_entry, _liveProperties, _propertyStore);
        }

        private class PropertiesEnumerator : IAsyncEnumerator<IProperty>
        {
            private readonly IEntry _entry;

            private readonly IPropertyStore _propertyStore;

            private readonly IEnumerator<IProperty> _livePropertiesEnumerator;

            private bool _livePropertiesFinished;

            private IEnumerator<IProperty> _deadPropertiesEnumerator;

            public PropertiesEnumerator(IEntry entry, IReadOnlyCollection<IProperty> liveProperties, IPropertyStore propertyStore)
            {
                _entry = entry;
                _propertyStore = propertyStore;
                _livePropertiesEnumerator = liveProperties.GetEnumerator();
            }

            public IProperty Current { get; private set; }

            public async Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                if (!_livePropertiesFinished)
                {
                    if (_livePropertiesEnumerator.MoveNext())
                    {
                        Current = _livePropertiesEnumerator.Current;
                        return true;
                    }

                    _livePropertiesFinished = true;

                    if (_propertyStore == null)
                        return false;

                    var deadProperties = await _propertyStore.LoadAndCreateAsync(_entry, cancellationToken).ConfigureAwait(false);
                    _deadPropertiesEnumerator = deadProperties.GetEnumerator();
                }

                if (_propertyStore == null)
                    return false;

                if (!_deadPropertiesEnumerator.MoveNext())
                    return false;

                Current = _deadPropertiesEnumerator.Current;

                return true;
            }

            public void Dispose()
            {
                _livePropertiesEnumerator?.Dispose();
                _deadPropertiesEnumerator?.Dispose();
            }
        }
    }
}