using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Properties;
using FubarDev.WebDavServer.Properties.Dead;
using FubarDev.WebDavServer.Properties.Live;

namespace FubarDev.WebDavServer.Engines.FileSystemTargets
{
    public abstract class EntryTarget : IExistingTarget
    {
        private readonly IEntry _entry;

        protected EntryTarget(Uri destinationUrl, IEntry entry)
        {
            _entry = entry;
            DestinationUrl = destinationUrl;
        }

        public string Name => _entry.Name;
        public Uri DestinationUrl { get; }

        public async Task<ExecutionResult> SetPropertiesAsync(IEnumerable<IUntypedWriteableProperty> properties, CancellationToken cancellationToken)
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

        private async Task SetPropertiesAsync(IEnumerable<IDeadProperty> properties, CancellationToken cancellationToken)
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

        private async Task<ExecutionResult> SetPropertiesAsync(IEnumerable<ILiveProperty> properties, CancellationToken cancellationToken)
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
                return new ExecutionResult()
                {
                    Target = this,
                    Href = DestinationUrl,
                    StatusCode = WebDavStatusCodes.NoContent
                };
            }

            using (var propEnum = _entry.GetProperties().GetEnumerator())
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
                var unsetPropNames = isPropUsed.Where(x => !x.Value).Select(x => x.Key.ToString());
                var unsetProperties = $"The following properties couldn't be set: {string.Join(", ", unsetPropNames)}";
                return new ExecutionResult()
                {
                    Target = this,
                    Href = DestinationUrl,
                    Error = new Error()
                    {
                        ItemsElementName = new[] { ItemsChoiceType.PreservedLiveProperties },
                        Items = new[] { new object() },
                    },
                    Reason = unsetProperties,
                    StatusCode = WebDavStatusCodes.Conflict
                };
            }

            return new ExecutionResult()
            {
                Target = this,
                Href = DestinationUrl,
                StatusCode = WebDavStatusCodes.OK
            };
        }
    }
}
