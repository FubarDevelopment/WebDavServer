using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Handlers;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Properties;
using FubarDev.WebDavServer.Properties.Dead;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.DefaultHandlers
{
    public class PropPatchHandler : IPropPatchHandler
    {
        private readonly IFileSystem _fileSystem;

        private readonly IWebDavHost _host;

        public PropPatchHandler(IFileSystem fileSystem, IWebDavHost host)
        {
            _fileSystem = fileSystem;
            _host = host;
        }

        public IEnumerable<string> HttpMethods { get; } = new[] { "PROPPATCH" };

        public async Task<IWebDavResult> PropPatchAsync(string path, Propertyupdate request, CancellationToken cancellationToken)
        {
            var selectionResult = await _fileSystem.SelectAsync(path, cancellationToken).ConfigureAwait(false);
            if (selectionResult.IsMissing)
                throw new WebDavException(WebDavStatusCodes.NotFound);

            var entry = (IEntry)selectionResult.Document ?? selectionResult.Collection;
            var propertiesList = new List<IUntypedReadableProperty>();
            Debug.Assert(entry != null, "entry != null");
            using (var propEnum = entry.GetProperties().GetEnumerator())
            {
                while (await propEnum.MoveNext(cancellationToken).ConfigureAwait(false))
                {
                    propertiesList.Add(propEnum.Current);
                }
            }

            var properties = propertiesList.ToDictionary(x => x.Name);
            var changes = await ApplyChangesAsync(entry, properties, request, cancellationToken).ConfigureAwait(false);
            if (changes.Any(x => !x.IsSuccess))
            {
                changes = await RevertChangesAsync(entry, changes, properties, cancellationToken).ConfigureAwait(false);
            }

            var failedItem = changes.FirstOrDefault(x => x.IsFailure);
            var isReadOnly = failedItem != null && failedItem.Status == ChangeStatus.ReadOnlyProperty;
            if (isReadOnly)
            {
                // tried to update a read-only property
                return new WebDavResult<Error1>(WebDavStatusCodes.Forbidden, new Error1()
                {
                    ItemsElementName = new[] { ItemsChoiceType2.CannotModifyProtectedProperty, },
                    Items = new[] { new object(), }
                });
            }

            var propStats = new List<Propstat>();
            var changesByStatusCodes = changes.GroupBy(x => x.StatusCode);
            foreach (var changesByStatusCode in changesByStatusCodes)
            {
                var elements = new List<XElement>();
                foreach (var changeItem in changesByStatusCode)
                {
                    elements.Add(new XElement(changeItem.Name));
                }

                var propStat = new Propstat()
                {
                    Prop = new Prop()
                    {
                        Any = elements.ToArray(),
                    },
                    Status = $"{_host.RequestProtocol} {(int)changesByStatusCode.Key} {changesByStatusCode.Key.GetReasonPhrase()}"
                };

                propStats.Add(propStat);
            }

            var status = new Multistatus()
            {
                Response = new[]
                {
                    new Response()
                    {
                        Href = _host.BaseUrl.Append(path).OriginalString,
                        ItemsElementName = propStats.Select(x => ItemsChoiceType1.Propstat).ToArray(),
                        Items = propStats.Cast<object>().ToArray()
                    }
                }
            };

            return new WebDavResult<Multistatus>(WebDavStatusCodes.MultiStatus, status);
        }

        private async Task<IReadOnlyCollection<ChangeItem>> RevertChangesAsync(IEntry entry, IReadOnlyCollection<ChangeItem> changes, Dictionary<XName, IUntypedReadableProperty> properties, CancellationToken cancellationToken)
        {
            var newChangeItems = new List<ChangeItem>();

            foreach (var changeItem in changes.Reverse())
            {
                ChangeItem newChangeItem;
                switch (changeItem.Status)
                {
                    case ChangeStatus.Added:
                        Debug.Assert(entry.FileSystem.PropertyStore != null);
                        await entry.FileSystem.PropertyStore.RemoveAsync(entry, changeItem.Name, cancellationToken).ConfigureAwait(false);
                        newChangeItem = ChangeItem.FailedDependency(changeItem.Name);
                        properties.Remove(changeItem.Name);
                        break;
                    case ChangeStatus.Modified:
                        Debug.Assert(entry.FileSystem.PropertyStore != null);
                        await entry.FileSystem.PropertyStore.SetAsync(entry, changeItem.OldValue, cancellationToken).ConfigureAwait(false);
                        newChangeItem = ChangeItem.FailedDependency(changeItem.Name);
                        break;
                    case ChangeStatus.Removed:
                        if (changeItem.Property != null)
                        {
                            properties.Add(changeItem.Name, changeItem.Property);
                            Debug.Assert(_fileSystem.PropertyStore != null);
                            await _fileSystem.PropertyStore.SetAsync(entry, changeItem.OldValue, cancellationToken).ConfigureAwait(false);
                        }
                        newChangeItem = ChangeItem.FailedDependency(changeItem.Name);
                        break;
                    case ChangeStatus.Conflict:
                    case ChangeStatus.Failed:
                    case ChangeStatus.InsufficientStorage:
                    case ChangeStatus.ReadOnlyProperty:
                    case ChangeStatus.FailedDependency:
                        newChangeItem = changeItem;
                        break;
                    default:
                        throw new NotSupportedException();
                }

                newChangeItems.Add(newChangeItem);
            }

            newChangeItems.Reverse();
            return newChangeItems;
        }

        private async Task<IReadOnlyCollection<ChangeItem>> ApplyChangesAsync(IEntry entry, Dictionary<XName, IUntypedReadableProperty> properties, Propertyupdate request, CancellationToken cancellationToken)
        {
            var result = new List<ChangeItem>();

            var failed = false;
            foreach (var item in request.Items)
            {
                IReadOnlyCollection<ChangeItem> changeItems;
                var set = item as Propset;
                if (set != null)
                {
                    changeItems = await ApplySetAsync(entry, properties, set, failed, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    var remove = (Propremove)item;
                    changeItems = await ApplyRemoveAsync(entry, properties, remove, failed, cancellationToken).ConfigureAwait(false);
                }

                result.AddRange(changeItems);

                failed = failed || changeItems.Any(x => !x.IsSuccess);
            }

            return result;
        }

        private async Task<IReadOnlyCollection<ChangeItem>> ApplyRemoveAsync(IEntry entry, Dictionary<XName, IUntypedReadableProperty> properties, Propremove remove, bool previouslyFailed, CancellationToken cancellationToken)
        {
            var result = new List<ChangeItem>();

            var failed = previouslyFailed;
            foreach (var element in remove.Prop.Any)
            {
                if (failed)
                {
                    result.Add(ChangeItem.FailedDependency(element.Name));
                    continue;
                }

                IUntypedReadableProperty property;
                if (properties.TryGetValue(element.Name, out property))
                {
                    if (entry.FileSystem.PropertyStore == null)
                    {
                        result.Add(ChangeItem.ReadOnly(property, element, "No property store"));
                    }
                    else if (!(property is IUntypedWriteableProperty))
                    {
                        result.Add(ChangeItem.ReadOnly(property, element, "Cannot remove protected property"));
                    }
                    else
                    {
                        try
                        {
                            var oldValue = await property.GetXmlValueAsync(cancellationToken).ConfigureAwait(false);
                            var success = await entry.FileSystem.PropertyStore.RemoveAsync(entry, element.Name, cancellationToken).ConfigureAwait(false);
                            if (!success)
                            {
                                result.Add(ChangeItem.Failed(property, "Cannot remove live property"));
                            }
                            else
                            {
                                result.Add(ChangeItem.Removed(property, oldValue));
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Add(ChangeItem.Failed(property, ex.Message));
                            failed = true;
                        }
                    }
                }
                else
                {
                    result.Add(ChangeItem.Removed(element.Name));
                }
            }

            return result;
        }

        private async Task<IReadOnlyCollection<ChangeItem>> ApplySetAsync(IEntry entry, Dictionary<XName, IUntypedReadableProperty> properties, Propset set, bool previouslyFailed, CancellationToken cancellationToken)
        {
            var result = new List<ChangeItem>();

            var failed = previouslyFailed;
            foreach (var element in set.Prop.Any)
            {
                if (failed)
                {
                    result.Add(ChangeItem.FailedDependency(element.Name));
                    continue;
                }

                IUntypedReadableProperty property;
                if (properties.TryGetValue(element.Name, out property))
                {
                    ChangeItem changeItem;
                    try
                    {
                        var writeableProperty = property as IUntypedWriteableProperty;
                        if (writeableProperty != null)
                        {
                            var oldValue = await writeableProperty.GetXmlValueAsync(cancellationToken).ConfigureAwait(false);
                            await writeableProperty.SetXmlValueAsync(element, cancellationToken).ConfigureAwait(false);
                            changeItem = ChangeItem.Modified(property, element, oldValue);
                        }
                        else
                        {
                            changeItem = ChangeItem.ReadOnly(property, element, "Cannot modify protected property");
                        }
                    }
                    catch (Exception ex)
                    {
                        changeItem = ChangeItem.Failed(property, ex.Message);
                    }

                    failed = !changeItem.IsSuccess;
                    result.Add(changeItem);
                }
                else
                {
                    if (entry.FileSystem.PropertyStore == null)
                    {
                        result.Add(ChangeItem.InsufficientStorage(element, "No property store"));
                        failed = true;
                    }
                    else
                    {
                        var newProperty = new DeadProperty(entry.FileSystem.PropertyStore, entry, element);
                        properties.Add(newProperty.Name, newProperty);
                        result.Add(ChangeItem.Added(newProperty, element));
                    }
                }
            }

            return result;
        }

        public class ChangeItem
        {
            private ChangeItem(ChangeStatus status, IUntypedReadableProperty property, XElement newValue, XElement oldValue, [NotNull] XName name, string description)
            {
                Status = status;
                Property = property;
                NewValue = newValue;
                OldValue = oldValue;
                Name = name;
                Description = description;
            }

            public ChangeStatus Status { get; }

            [CanBeNull]
            public IUntypedReadableProperty Property { get; }

            [CanBeNull]
            public XElement NewValue { get; }

            [CanBeNull]
            public XElement OldValue { get; }

            [NotNull]
            public XName Name { get; }

            public string Description { get; }

            public bool IsSuccess => Status == ChangeStatus.Added || Status == ChangeStatus.Modified || Status == ChangeStatus.Removed;

            public bool IsFailure => Status == ChangeStatus.Conflict || Status == ChangeStatus.Failed || Status == ChangeStatus.InsufficientStorage || Status == ChangeStatus.ReadOnlyProperty;

            public WebDavStatusCodes StatusCode
            {
                get
                {
                    switch (Status)
                    {
                        case ChangeStatus.Added:
                        case ChangeStatus.Modified:
                        case ChangeStatus.Removed:
                            return WebDavStatusCodes.OK;
                        case ChangeStatus.Conflict:
                            return WebDavStatusCodes.Conflict;
                        case ChangeStatus.FailedDependency:
                            return WebDavStatusCodes.FailedDependency;
                        case ChangeStatus.InsufficientStorage:
                            return WebDavStatusCodes.InsufficientStorage;
                        case ChangeStatus.Failed:
                        case ChangeStatus.ReadOnlyProperty:
                            return WebDavStatusCodes.Forbidden;
                    }

                    throw new NotSupportedException();
                }
            }

            public static ChangeItem Added(IUntypedReadableProperty property, XElement newValue)
            {
                return new ChangeItem(ChangeStatus.Added, property, newValue, null, newValue.Name, null);
            }

            public static ChangeItem Modified(IUntypedReadableProperty property, XElement newValue, XElement oldValue)
            {
                return new ChangeItem(ChangeStatus.Modified, property, newValue, oldValue, newValue.Name, null);
            }

            public static ChangeItem Removed([NotNull] IUntypedReadableProperty property, XElement oldValue)
            {
                return new ChangeItem(ChangeStatus.Removed, property, null, oldValue, property.Name, null);
            }

            public static ChangeItem Removed([NotNull] XName name)
            {
                return new ChangeItem(ChangeStatus.Removed, null, null, null, name, null);
            }

            public static ChangeItem Failed([NotNull] IUntypedReadableProperty property, string description)
            {
                return new ChangeItem(ChangeStatus.Failed, property, null, null, property.Name, description);
            }

            public static ChangeItem Conflict([NotNull] IUntypedReadableProperty property, [NotNull] XElement oldValue, string description)
            {
                return new ChangeItem(ChangeStatus.Conflict, property, null, oldValue, property.Name, description);
            }

            public static ChangeItem FailedDependency(XName name, string description = "Failed dependency")
            {
                return new ChangeItem(ChangeStatus.FailedDependency, null, null, null, name, description);
            }

            public static ChangeItem InsufficientStorage(XElement newValue, string description)
            {
                return new ChangeItem(ChangeStatus.InsufficientStorage, null, newValue, null, newValue.Name, description);
            }

            public static ChangeItem ReadOnly(IUntypedReadableProperty property, XElement newValue, string description)
            {
                return new ChangeItem(ChangeStatus.ReadOnlyProperty, property, newValue, null, property.Name, description);
            }
        }

        public enum ChangeStatus
        {
            Added,
            Modified,
            Removed,
            Failed,
            Conflict,
            FailedDependency,
            InsufficientStorage,
            ReadOnlyProperty
        }
    }
}
