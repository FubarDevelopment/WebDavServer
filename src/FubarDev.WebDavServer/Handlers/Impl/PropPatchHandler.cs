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

using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    /// <summary>
    /// Implementation of the <see cref="IPropPatchHandler"/> interface.
    /// </summary>
    public class PropPatchHandler : IPropPatchHandler
    {
        private readonly IFileSystem _fileSystem;

        private readonly IWebDavContextAccessor _contextAccessor;

        private readonly IImplicitLockFactory _implicitLockFactory;
        private readonly IDeadPropertyFactory _deadPropertyFactory;

        private readonly bool _encodeHref;

        private readonly bool _atomicOperations;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropPatchHandler"/> class.
        /// </summary>
        /// <param name="litmusCompatibilityOptions">The compatibility options for the litmus tests.</param>
        /// <param name="fileSystem">The root file system.</param>
        /// <param name="contextAccessor">The WebDAV request context accessor.</param>
        /// <param name="implicitLockFactory">A factory to create implicit locks.</param>
        /// <param name="deadPropertyFactory">Factory for dead properties.</param>
        public PropPatchHandler(
            IOptions<LitmusCompatibilityOptions> litmusCompatibilityOptions,
            IFileSystem fileSystem,
            IWebDavContextAccessor contextAccessor,
            IImplicitLockFactory implicitLockFactory,
            IDeadPropertyFactory deadPropertyFactory)
        {
            _encodeHref = !litmusCompatibilityOptions.Value.DisableUrlEncodingOfResponseHref;
            _atomicOperations = litmusCompatibilityOptions.Value.UseAtomicPropSet;
            _fileSystem = fileSystem;
            _contextAccessor = contextAccessor;
            _implicitLockFactory = implicitLockFactory;
            _deadPropertyFactory = deadPropertyFactory;
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
            ReadOnlyProperty,
        }

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; } = new[] { "PROPPATCH" };

        /// <inheritdoc />
        public async Task<IWebDavResult> PropPatchAsync(
            string path,
            propertyupdate request,
            CancellationToken cancellationToken)
        {
            var context = _contextAccessor.WebDavContext;

            var selectionResult = await _fileSystem.SelectAsync(path, cancellationToken).ConfigureAwait(false);
            if (selectionResult.IsMissing)
            {
                if (context.RequestHeaders.IfNoneMatch != null)
                {
                    throw new WebDavException(WebDavStatusCode.PreconditionFailed);
                }

                throw new WebDavException(WebDavStatusCode.NotFound);
            }

            var targetEntry = selectionResult.TargetEntry;
            Debug.Assert(targetEntry != null, "targetEntry != null");

            await context.RequestHeaders
                .ValidateAsync(selectionResult.TargetEntry, cancellationToken).ConfigureAwait(false);

            var lockRequirements = new Lock(
                new Uri(path, UriKind.Relative),
                context.HrefUrl,
                false,
                context.User.Identity?.GetOwner(),
                context.User.Identity?.GetOwnerHref(),
                LockAccessType.Write,
                LockShareMode.Exclusive,
                TimeoutHeader.Infinite);
            var tempLock = await _implicitLockFactory.CreateAsync(lockRequirements, cancellationToken)
                .ConfigureAwait(false);
            if (!tempLock.IsSuccessful)
            {
                return tempLock.CreateErrorResponse();
            }

            try
            {
                var propertiesList = await targetEntry.GetProperties(_deadPropertyFactory, returnInvalidProperties: true)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                var properties = propertiesList.ToDictionary(x => x.Name);
                var changes =
                    await ApplyChangesAsync(targetEntry, properties, request, cancellationToken).ConfigureAwait(false);

                WebDavStatusCode statusCode;
                bool updateEtags;
                if (_atomicOperations)
                {
                    var hasError = changes.Any(x => !x.IsSuccess);
                    if (hasError)
                    {
                        changes = await RevertChangesAsync(
                                targetEntry,
                                changes,
                                properties,
                                cancellationToken)
                            .ConfigureAwait(false);
                        statusCode = WebDavStatusCode.Forbidden;
                        updateEtags = false;
                    }
                    else
                    {
                        statusCode = WebDavStatusCode.MultiStatus;
                        updateEtags = changes.Any(c => c.IsSuccess);
                    }
                }
                else
                {
                    statusCode = WebDavStatusCode.MultiStatus;
                    updateEtags = changes.Any(c => c.IsSuccess);
                }

                if (updateEtags)
                {
                    var targetPropStore = targetEntry.FileSystem.PropertyStore;
                    if (targetPropStore != null)
                    {
                        await targetPropStore.UpdateETagAsync(targetEntry, cancellationToken).ConfigureAwait(false);
                    }

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

                var propStats = new List<propstat>();

                var readOnlyProperties = changes.Where(x => x.Status == ChangeStatus.ReadOnlyProperty).ToList();
                if (readOnlyProperties.Count != 0)
                {
                    propStats.AddRange(
                        CreatePropStats(
                            context,
                            readOnlyProperties,
                            new error()
                            {
                                ItemsElementName = new[] { ItemsChoiceType.cannotmodifyprotectedproperty, },
                                Items = new[] { new object(), },
                            }));
                    changes = changes.Except(readOnlyProperties).ToList();
                }

                propStats.AddRange(CreatePropStats(context, changes, null));

                var status = new multistatus()
                {
                    response = new[]
                    {
                        new response()
                        {
                            href = context.PublicControllerUrl.Append(path, true).EncodeHref(_encodeHref),
                            ItemsElementName = propStats.Select(_ => ItemsChoiceType2.propstat).ToArray(),
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

        private static IUntypedReadableProperty? FindProperty(
            IReadOnlyDictionary<XName, IUntypedReadableProperty> properties,
            XName propertyKey)
        {
            if (properties.TryGetValue(propertyKey, out var foundProperty))
            {
                return foundProperty;
            }

            foreach (var item in properties.Values.Where(x => x.AlternativeNames.Count != 0))
            {
                if (item.AlternativeNames.Any(x => x == propertyKey))
                {
                    return item;
                }
            }

            return null;
        }

        private IEnumerable<propstat> CreatePropStats(
            IWebDavContext context,
            IEnumerable<ChangeItem> changes,
            error? error)
        {
            var changesByStatusCodes = changes.GroupBy(x => x.StatusCode);
            foreach (var changesByStatusCode in changesByStatusCodes)
            {
                var elements = new List<XElement>();
                foreach (var changeItem in changesByStatusCode)
                {
                    elements.Add(new XElement(changeItem.Key));
                }

                var propStat = new propstat()
                {
                    prop = new prop()
                    {
                        Any = elements.ToArray(),
                    },
                    status = new Status(context.RequestProtocol, changesByStatusCode.Key).ToString(),
                    error = error,
                };

                yield return propStat;
            }
        }

        private async Task<IReadOnlyCollection<ChangeItem>> RevertChangesAsync(
            IEntry entry,
            IReadOnlyCollection<ChangeItem> changes,
            IDictionary<XName, IUntypedReadableProperty> properties,
            CancellationToken cancellationToken)
        {
            if (entry.FileSystem.PropertyStore == null || _fileSystem.PropertyStore == null)
            {
                throw new InvalidOperationException("The property store must be configured");
            }

            var newChangeItems = new List<ChangeItem>();

            foreach (var changeItem in changes.Reverse())
            {
                ChangeItem newChangeItem;
                switch (changeItem.Status)
                {
                    case ChangeStatus.Added:
                        Debug.Assert(entry.FileSystem.PropertyStore != null, "entry.FileSystem.PropertyStore != null");
                        await entry.FileSystem.PropertyStore.RemoveAsync(entry, changeItem.Key, cancellationToken)
                            .ConfigureAwait(false);
                        newChangeItem = ChangeItem.FailedDependency(changeItem.Key);
                        properties.Remove(changeItem.Key);
                        break;
                    case ChangeStatus.Modified:
                        Debug.Assert(entry.FileSystem.PropertyStore != null, "entry.FileSystem.PropertyStore != null");
                        Debug.Assert(changeItem.OldValue != null, "changeItem.OldValue != null");
                        if (changeItem.OldValue == null)
                        {
                            throw new InvalidOperationException("There must be a old value for the item to change");
                        }

                        await entry.FileSystem.PropertyStore.SetAsync(entry, changeItem.OldValue, cancellationToken)
                            .ConfigureAwait(false);
                        newChangeItem = ChangeItem.FailedDependency(changeItem.Key);
                        break;
                    case ChangeStatus.Removed:
                        if (changeItem.Property != null)
                        {
                            properties.Add(changeItem.Key, changeItem.Property);
                            Debug.Assert(_fileSystem.PropertyStore != null, "_fileSystem.PropertyStore != null");
                            Debug.Assert(changeItem.OldValue != null, "changeItem.OldValue != null");
                            if (changeItem.OldValue == null)
                            {
                                throw new InvalidOperationException("There must be a old value for the item to change");
                            }

                            await _fileSystem.PropertyStore.SetAsync(entry, changeItem.OldValue, cancellationToken)
                                .ConfigureAwait(false);
                        }

                        newChangeItem = ChangeItem.FailedDependency(changeItem.Key);
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

        private async Task<IReadOnlyCollection<ChangeItem>> ApplyChangesAsync(
            IEntry entry,
            Dictionary<XName, IUntypedReadableProperty> properties,
            propertyupdate request,
            CancellationToken cancellationToken)
        {
            var result = new List<ChangeItem>();
            if (request.Items == null)
            {
                return result;
            }

            var failed = false;
            foreach (var item in request.Items)
            {
                IReadOnlyCollection<ChangeItem> changeItems;
                if (item is propset set)
                {
                    changeItems = await ApplySetAsync(entry, properties, set, failed, cancellationToken)
                        .ConfigureAwait(false);
                }
                else if (item is propremove remove)
                {
                    changeItems = await ApplyRemoveAsync(entry, properties, remove, failed, cancellationToken)
                        .ConfigureAwait(false);
                }
                else
                {
                    changeItems = Array.Empty<ChangeItem>();
                }

                result.AddRange(changeItems);

                failed = failed || changeItems.Any(x => !x.IsSuccess);
            }

            return result;
        }

        private async Task<IReadOnlyCollection<ChangeItem>> ApplyRemoveAsync(
            IEntry entry,
            IReadOnlyDictionary<XName, IUntypedReadableProperty> properties,
            propremove remove,
            bool previouslyFailed,
            CancellationToken cancellationToken)
        {
            var result = new List<ChangeItem>();

            if (remove.prop?.Any == null)
            {
                return result;
            }

            var language = remove.prop.Language;

            var failed = previouslyFailed;
            foreach (var element in remove.prop.Any)
            {
                // Add a parent elements xml:lang to the element
                var elementLanguage = element.Attribute(XNamespace.Xml + "lang")?.Value;
                if (string.IsNullOrEmpty(elementLanguage) && !string.IsNullOrEmpty(language))
                {
                    element.SetAttributeValue(XNamespace.Xml + "lang", language);
                }

                var propertyKey = element.Name;

                if (failed)
                {
                    result.Add(ChangeItem.FailedDependency(propertyKey));
                    continue;
                }

                var property = FindProperty(properties, propertyKey);
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
                            result.Add(
                                ChangeItem.ReadOnly(property, element, "Cannot remove dead without property store"));
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
                            var success = await entry.FileSystem.PropertyStore
                                .RemoveAsync(entry, propertyKey, cancellationToken).ConfigureAwait(false);

                            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                            if (!success)
                            {
                                result.Add(
                                    ChangeItem.Failed(
                                        property,
                                        "Couldn't remove property from property store (concurrent access?)"));
                            }
                            else
                            {
                                result.Add(ChangeItem.Removed(property, oldValue));
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Add(ChangeItem.Failed(property, ex));
                            failed = true;
                        }
                    }
                }
                else
                {
                    result.Add(ChangeItem.Removed(propertyKey));
                }
            }

            return result;
        }

        private async Task<IReadOnlyCollection<ChangeItem>> ApplySetAsync(
            IEntry entry,
            Dictionary<XName, IUntypedReadableProperty> properties,
            propset set,
            bool previouslyFailed,
            CancellationToken cancellationToken)
        {
            var result = new List<ChangeItem>();

            if (set.prop?.Any == null)
            {
                return result;
            }

            var language = set.prop.Language;

            var failed = previouslyFailed;
            foreach (var element in set.prop.Any)
            {
                // Add a parent elements xml:lang to the element
                var elementLanguage = element.Attribute(XNamespace.Xml + "lang")?.Value;
                if (string.IsNullOrEmpty(elementLanguage) && !string.IsNullOrEmpty(language))
                {
                    element.SetAttributeValue(XNamespace.Xml + "lang", language);
                }

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
                        if (property is IUntypedWriteableProperty writeableProperty)
                        {
                            if (entry.FileSystem.PropertyStore == null && writeableProperty is IDeadProperty)
                            {
                                changeItem = ChangeItem.ReadOnly(
                                    property,
                                    element,
                                    "Cannot modify dead without property store");
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
                        changeItem = ChangeItem.Failed(property, ex);
                    }

                    failed = !changeItem.IsSuccess;
                    result.Add(changeItem);
                }
                else
                {
                    if (entry.FileSystem.PropertyStore == null)
                    {
                        result.Add(
                            ChangeItem.InsufficientStorage(element, "Cannot add dead property without property store"));
                        failed = true;
                    }
                    else
                    {
                        var newProperty = new DeadProperty(entry.FileSystem.PropertyStore, entry, element);

                        ChangeItem changeItem;
                        try
                        {
                            await newProperty.SetXmlValueAsync(element, cancellationToken).ConfigureAwait(false);

                            properties.Add(newProperty.Name, newProperty);
                            changeItem = ChangeItem.Added(newProperty, element);
                        }
                        catch (Exception ex)
                        {
                            changeItem = ChangeItem.Failed(newProperty, ex.Message);
                        }

                        failed = !changeItem.IsSuccess;
                        result.Add(changeItem);
                    }
                }
            }

            return result;
        }

        [SuppressMessage(
            "ReSharper",
            "MemberCanBePrivate.Local",
            Justification = "Reviewed. Might be used when locking is implemented.")]
        [SuppressMessage(
            "ReSharper",
            "UnusedAutoPropertyAccessor.Local",
            Justification = "Reviewed. Might be used when locking is implemented.")]
        [SuppressMessage(
            "ReSharper",
            "UnusedMember.Local",
            Justification = "Reviewed. Might be used when locking is implemented.")]
        private class ChangeItem
        {
            private ChangeItem(
                ChangeStatus status,
                IUntypedReadableProperty? property,
                XElement? newValue,
                XElement? oldValue,
                XName key,
                string? description,
                Exception? exception)
            {
                Status = status;
                Property = property;
                NewValue = newValue;
                OldValue = oldValue;
                Key = key;
                Description = description;
                Exception = exception;
            }

            public ChangeStatus Status { get; }

            public IUntypedReadableProperty? Property { get; }

            public XElement? NewValue { get; }

            public XElement? OldValue { get; }

            public XName Key { get; }

            public string? Description { get; }

            public Exception? Exception { get; }

            public bool IsSuccess => Status == ChangeStatus.Added || Status == ChangeStatus.Modified ||
                                     Status == ChangeStatus.Removed;

            public bool IsFailure => Status == ChangeStatus.Conflict || Status == ChangeStatus.Failed ||
                                     Status == ChangeStatus.InsufficientStorage ||
                                     Status == ChangeStatus.ReadOnlyProperty;

            public WebDavStatusCode StatusCode =>
                Status switch
                {
                    ChangeStatus.Added => WebDavStatusCode.OK,
                    ChangeStatus.Modified => WebDavStatusCode.OK,
                    ChangeStatus.Removed => WebDavStatusCode.OK,
                    ChangeStatus.Conflict => WebDavStatusCode.Conflict,
                    ChangeStatus.FailedDependency => WebDavStatusCode.FailedDependency,
                    ChangeStatus.InsufficientStorage => WebDavStatusCode.InsufficientStorage,

                    // Returning "Conflict" is required, according to tolsen/litmus
                    ChangeStatus.Failed =>
                        Exception is ArgumentException
                            ? WebDavStatusCode.Conflict
                            : WebDavStatusCode.Forbidden,
                    ChangeStatus.ReadOnlyProperty => WebDavStatusCode.Forbidden,
                    _ => throw new NotSupportedException(),
                };

            public static ChangeItem Added(IUntypedReadableProperty property, XElement newValue)
            {
                return new ChangeItem(ChangeStatus.Added, property, newValue, null, property.Name, null, null);
            }

            public static ChangeItem Modified(IUntypedReadableProperty property, XElement newValue, XElement oldValue)
            {
                return new ChangeItem(ChangeStatus.Modified, property, newValue, oldValue, property.Name, null, null);
            }

            public static ChangeItem Removed(IUntypedReadableProperty property, XElement oldValue)
            {
                return new ChangeItem(ChangeStatus.Removed, property, null, oldValue, property.Name, null, null);
            }

            public static ChangeItem Removed(XName key)
            {
                return new ChangeItem(ChangeStatus.Removed, null, null, null, key, null, null);
            }

            public static ChangeItem Failed(IUntypedReadableProperty property, string description)
            {
                return new ChangeItem(ChangeStatus.Failed, property, null, null, property.Name, description, null);
            }

            public static ChangeItem Failed(IUntypedReadableProperty property, Exception exception)
            {
                return new ChangeItem(ChangeStatus.Failed, property, null, null, property.Name, exception.Message, exception);
            }

            public static ChangeItem Conflict(IUntypedReadableProperty property, XElement oldValue, string description)
            {
                return new ChangeItem(ChangeStatus.Conflict, property, null, oldValue, property.Name, description, null);
            }

            public static ChangeItem FailedDependency(XName key, string description = "Failed dependency")
            {
                return new ChangeItem(ChangeStatus.FailedDependency, null, null, null, key, description, null);
            }

            public static ChangeItem InsufficientStorage(XElement newValue, string description)
            {
                return new ChangeItem(
                    ChangeStatus.InsufficientStorage,
                    null,
                    newValue,
                    null,
                    newValue.Name,
                    description,
                    null);
            }

            public static ChangeItem ReadOnly(IUntypedReadableProperty property, XElement newValue, string description)
            {
                return new ChangeItem(
                    ChangeStatus.ReadOnlyProperty,
                    property,
                    newValue,
                    null,
                    property.Name,
                    description,
                    null);
            }
        }
    }
}
