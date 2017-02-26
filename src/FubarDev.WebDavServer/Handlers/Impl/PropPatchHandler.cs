// <copyright file="PropPatchHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Model.Headers;
using FubarDev.WebDavServer.Props;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Live;
using FubarDev.WebDavServer.Utils;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    /// <summary>
    /// Implementation of the <see cref="IPropPatchHandler"/> interface
    /// </summary>
    public class PropPatchHandler : IPropPatchHandler
    {
        [NotNull]
        private readonly IFileSystem _fileSystem;

        [NotNull]
        private readonly IWebDavContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropPatchHandler"/> class.
        /// </summary>
        /// <param name="fileSystem">The root file system</param>
        /// <param name="context">The WebDAV request context</param>
        public PropPatchHandler([NotNull] IFileSystem fileSystem, [NotNull] IWebDavContext context)
        {
            _fileSystem = fileSystem;
            _context = context;
        }

        private enum ChangeStatus
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

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; } = new[] { "PROPPATCH" };

        /// <inheritdoc />
        public async Task<IWebDavResult> PropPatchAsync(
            string path,
            propertyupdate request,
            CancellationToken cancellationToken)
        {
            var selectionResult = await _fileSystem.SelectAsync(path, cancellationToken).ConfigureAwait(false);
            if (selectionResult.IsMissing)
            {
                if (_context.RequestHeaders.IfNoneMatch != null)
                    throw new WebDavException(WebDavStatusCode.PreconditionFailed);

                throw new WebDavException(WebDavStatusCode.NotFound);
            }

            var targetEntry = selectionResult.TargetEntry;
            Debug.Assert(targetEntry != null, "targetEntry != null");

            await _context.RequestHeaders
                .ValidateAsync(selectionResult.TargetEntry, cancellationToken).ConfigureAwait(false);

            var lockRequirements = new Lock(
                new Uri(path, UriKind.Relative),
                _context.RelativeRequestUrl,
                false,
                new XElement(WebDavXml.Dav + "owner", _context.User.Identity.Name),
                LockAccessType.Write,
                LockShareMode.Shared,
                TimeoutHeader.Infinite);
            var lockManager = _fileSystem.LockManager;
            var tempLock = lockManager == null
                ? new ImplicitLock(true)
                : await lockManager.LockImplicitAsync(
                        _fileSystem,
                        _context.RequestHeaders.If?.Lists,
                        lockRequirements,
                        cancellationToken)
                    .ConfigureAwait(false);
            if (!tempLock.IsSuccessful)
                return tempLock.CreateErrorResponse();

            try
            {
                var propertiesList = new List<IUntypedReadableProperty>();
                using (var propEnum = targetEntry.GetProperties(_context.Dispatcher).GetEnumerator())
                {
                    while (await propEnum.MoveNext(cancellationToken).ConfigureAwait(false))
                    {
                        propertiesList.Add(propEnum.Current);
                    }
                }

                var properties = propertiesList.ToDictionary(x => x.Name);
                var changes =
                    await ApplyChangesAsync(targetEntry, properties, request, cancellationToken).ConfigureAwait(false);
                var hasError = changes.Any(x => !x.IsSuccess);
                if (hasError)
                {
                    changes = await RevertChangesAsync(
                            targetEntry,
                            changes,
                            properties,
                            cancellationToken)
                        .ConfigureAwait(false);
                }
                else
                {
                    var targetPropStore = targetEntry.FileSystem.PropertyStore;
                    if (targetPropStore != null)
                        await targetPropStore.UpdateETagAsync(targetEntry, cancellationToken).ConfigureAwait(false);
                    var parent = targetEntry.Parent;
                    while (parent != null)
                    {
                        var parentPropStore = parent.FileSystem.PropertyStore;
                        if (parentPropStore != null)
                        {
                            await parentPropStore.UpdateETagAsync(parent, cancellationToken)
                                .ConfigureAwait(false);
                        }

                        parent = parent.Parent;
                    }
                }

                var statusCode = hasError ? WebDavStatusCode.Forbidden : WebDavStatusCode.MultiStatus;
                var propStats = new List<propstat>();

                var readOnlyProperties = changes.Where(x => x.Status == ChangeStatus.ReadOnlyProperty).ToList();
                if (readOnlyProperties.Count != 0)
                {
                    propStats.AddRange(
                        CreatePropStats(
                            readOnlyProperties,
                            new error()
                            {
                                ItemsElementName = new[] { ItemsChoiceType.cannotmodifyprotectedproperty, },
                                Items = new[] { new object(), },
                            }));
                    changes = changes.Except(readOnlyProperties).ToList();
                }

                propStats.AddRange(CreatePropStats(changes, null));

                var status = new multistatus()
                {
                    response = new[]
                    {
                        new response()
                        {
                            href = _context.BaseUrl.Append(path, true).OriginalString,
                            ItemsElementName = propStats.Select(x => ItemsChoiceType2.propstat).ToArray(),
                            Items = propStats.Cast<object>().ToArray(),
                        },
                    },
                };

                return new WebDavResult<multistatus>(statusCode, status);
            }
            finally
            {
                await tempLock.DisposeAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        [CanBeNull]
        private static IUntypedReadableProperty FindProperty([NotNull] IReadOnlyDictionary<XName, IUntypedReadableProperty> properties, [NotNull] XName name)
        {
            IUntypedReadableProperty property;
            if (properties.TryGetValue(name, out property))
                return property;

            foreach (var readableProperty in properties.Values.Where(x => x.AlternativeNames.Count != 0))
            {
                if (readableProperty.AlternativeNames.Any(x => x == name))
                    return readableProperty;
            }

            return null;
        }

        [NotNull]
        [ItemNotNull]
        private IEnumerable<propstat> CreatePropStats([NotNull][ItemNotNull] IEnumerable<ChangeItem> changes, [CanBeNull] error error)
        {
            var changesByStatusCodes = changes.GroupBy(x => x.StatusCode);
            foreach (var changesByStatusCode in changesByStatusCodes)
            {
                var elements = new List<XElement>();
                foreach (var changeItem in changesByStatusCode)
                {
                    elements.Add(new XElement(changeItem.Name));
                }

                var propStat = new propstat()
                {
                    prop = new prop()
                    {
                        Any = elements.ToArray(),
                    },
                    status = new Status(_context.RequestProtocol, changesByStatusCode.Key).ToString(),
                    error = error,
                };

                yield return propStat;
            }
        }

        [NotNull]
        [ItemNotNull]
        private async Task<IReadOnlyCollection<ChangeItem>> RevertChangesAsync([NotNull] IEntry entry, [NotNull][ItemNotNull] IReadOnlyCollection<ChangeItem> changes, [NotNull] Dictionary<XName, IUntypedReadableProperty> properties, CancellationToken cancellationToken)
        {
            if (entry.FileSystem.PropertyStore == null || _fileSystem.PropertyStore == null)
                throw new InvalidOperationException("The property store must be configured");

            var newChangeItems = new List<ChangeItem>();

            foreach (var changeItem in changes.Reverse())
            {
                ChangeItem newChangeItem;
                switch (changeItem.Status)
                {
                    case ChangeStatus.Added:
                        Debug.Assert(entry.FileSystem.PropertyStore != null, "entry.FileSystem.PropertyStore != null");
                        await entry.FileSystem.PropertyStore.RemoveAsync(entry, changeItem.Name, cancellationToken).ConfigureAwait(false);
                        newChangeItem = ChangeItem.FailedDependency(changeItem.Name);
                        properties.Remove(changeItem.Name);
                        break;
                    case ChangeStatus.Modified:
                        Debug.Assert(entry.FileSystem.PropertyStore != null, "entry.FileSystem.PropertyStore != null");
                        Debug.Assert(changeItem.OldValue != null, "changeItem.OldValue != null");
                        if (changeItem.OldValue == null)
                            throw new InvalidOperationException("There must be a old value for the item to change");
                        await entry.FileSystem.PropertyStore.SetAsync(entry, changeItem.OldValue, cancellationToken).ConfigureAwait(false);
                        newChangeItem = ChangeItem.FailedDependency(changeItem.Name);
                        break;
                    case ChangeStatus.Removed:
                        if (changeItem.Property != null)
                        {
                            properties.Add(changeItem.Name, changeItem.Property);
                            Debug.Assert(_fileSystem.PropertyStore != null, "_fileSystem.PropertyStore != null");
                            Debug.Assert(changeItem.OldValue != null, "changeItem.OldValue != null");
                            if (changeItem.OldValue == null)
                                throw new InvalidOperationException("There must be a old value for the item to change");
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

        [NotNull]
        [ItemNotNull]
        private async Task<IReadOnlyCollection<ChangeItem>> ApplyChangesAsync([NotNull] IEntry entry, [NotNull] Dictionary<XName, IUntypedReadableProperty> properties, [NotNull] propertyupdate request, CancellationToken cancellationToken)
        {
            var result = new List<ChangeItem>();
            if (request.Items == null)
                return result;

            var failed = false;
            foreach (var item in request.Items)
            {
                IReadOnlyCollection<ChangeItem> changeItems;
                var set = item as propset;
                if (set != null)
                {
                    changeItems = await ApplySetAsync(entry, properties, set, failed, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    var remove = (propremove)item;
                    changeItems = await ApplyRemoveAsync(entry, properties, remove, failed, cancellationToken).ConfigureAwait(false);
                }

                result.AddRange(changeItems);

                failed = failed || changeItems.Any(x => !x.IsSuccess);
            }

            return result;
        }

        [NotNull]
        [ItemNotNull]
        private async Task<IReadOnlyCollection<ChangeItem>> ApplyRemoveAsync([NotNull] IEntry entry, [NotNull] IReadOnlyDictionary<XName, IUntypedReadableProperty> properties, [NotNull] propremove remove, bool previouslyFailed, CancellationToken cancellationToken)
        {
            var result = new List<ChangeItem>();

            if (remove.prop.Any == null)
                return result;

            var failed = previouslyFailed;
            foreach (var element in remove.prop.Any)
            {
                if (failed)
                {
                    result.Add(ChangeItem.FailedDependency(element.Name));
                    continue;
                }

                var property = FindProperty(properties, element.Name);
                if (property != null)
                {
                    if (!(property is IUntypedWriteableProperty))
                    {
                        result.Add(ChangeItem.ReadOnly(property, element, "Cannot remove protected property"));
                    }
                    else if (entry.FileSystem.PropertyStore == null)
                    {
                        if (property is IDeadProperty)
                        {
                            result.Add(ChangeItem.ReadOnly(property, element, "Cannot remove dead without property store"));
                        }
                        else
                        {
                            result.Add(ChangeItem.ReadOnly(property, element, "Cannot remove live property"));
                        }
                    }
                    else if (property is ILiveProperty)
                    {
                        result.Add(ChangeItem.Failed(property, "Cannot remove live property"));
                    }
                    else
                    {
                        try
                        {
                            var oldValue = await property.GetXmlValueAsync(cancellationToken).ConfigureAwait(false);
                            var success = await entry.FileSystem.PropertyStore.RemoveAsync(entry, element.Name, cancellationToken).ConfigureAwait(false);
                            if (!success)
                            {
                                result.Add(ChangeItem.Failed(property, "Couldn't remove property from property store (concurrent access?)"));
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

        [NotNull]
        [ItemNotNull]
        private async Task<IReadOnlyCollection<ChangeItem>> ApplySetAsync([NotNull] IEntry entry, [NotNull] Dictionary<XName, IUntypedReadableProperty> properties, [NotNull] propset set, bool previouslyFailed, CancellationToken cancellationToken)
        {
            var result = new List<ChangeItem>();

            if (set.prop.Any == null)
                return result;

            var failed = previouslyFailed;
            foreach (var element in set.prop.Any)
            {
                if (failed)
                {
                    result.Add(ChangeItem.FailedDependency(element.Name));
                    continue;
                }

                var property = FindProperty(properties, element.Name);
                if (property != null)
                {
                    ChangeItem changeItem;
                    try
                    {
                        var writeableProperty = property as IUntypedWriteableProperty;
                        if (writeableProperty != null)
                        {
                            if (entry.FileSystem.PropertyStore == null && writeableProperty is IDeadProperty)
                            {
                                changeItem = ChangeItem.ReadOnly(property, element, "Cannot modify dead without property store");
                            }
                            else
                            {
                                var oldValue = await writeableProperty
                                    .GetXmlValueAsync(cancellationToken)
                                    .ConfigureAwait(false);
                                await writeableProperty
                                    .SetXmlValueAsync(element, cancellationToken)
                                    .ConfigureAwait(false);
                                changeItem = ChangeItem.Modified(property, element, oldValue);
                            }
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
                        result.Add(ChangeItem.InsufficientStorage(element, "Cannot add dead property without property store"));
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

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local", Justification = "Reviewed. Might be used when locking is implemented.")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local", Justification = "Reviewed. Might be used when locking is implemented.")]
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Reviewed. Might be used when locking is implemented.")]
        private class ChangeItem
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

            public WebDavStatusCode StatusCode
            {
                get
                {
                    switch (Status)
                    {
                        case ChangeStatus.Added:
                        case ChangeStatus.Modified:
                        case ChangeStatus.Removed:
                            return WebDavStatusCode.OK;
                        case ChangeStatus.Conflict:
                            return WebDavStatusCode.Conflict;
                        case ChangeStatus.FailedDependency:
                            return WebDavStatusCode.FailedDependency;
                        case ChangeStatus.InsufficientStorage:
                            return WebDavStatusCode.InsufficientStorage;
                        case ChangeStatus.Failed:
                        case ChangeStatus.ReadOnlyProperty:
                            return WebDavStatusCode.Forbidden;
                    }

                    throw new NotSupportedException();
                }
            }

            public static ChangeItem Added(IUntypedReadableProperty property, [NotNull] XElement newValue)
            {
                return new ChangeItem(ChangeStatus.Added, property, newValue, null, newValue.Name, null);
            }

            public static ChangeItem Modified(IUntypedReadableProperty property, [NotNull] XElement newValue, XElement oldValue)
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

            public static ChangeItem InsufficientStorage([NotNull] XElement newValue, string description)
            {
                return new ChangeItem(ChangeStatus.InsufficientStorage, null, newValue, null, newValue.Name, description);
            }

            public static ChangeItem ReadOnly([NotNull] IUntypedReadableProperty property, XElement newValue, string description)
            {
                return new ChangeItem(ChangeStatus.ReadOnlyProperty, property, newValue, null, property.Name, description);
            }
        }
    }
}
