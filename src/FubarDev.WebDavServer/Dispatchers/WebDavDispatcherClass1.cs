// <copyright file="WebDavDispatcherClass1.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Handlers;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Model.Headers;
using FubarDev.WebDavServer.Props;
using FubarDev.WebDavServer.Props.Converters;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Live;
using FubarDev.WebDavServer.Props.Store;

using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.Dispatchers
{
    /// <summary>
    /// The default WebDAV class 1 implementation.
    /// </summary>
    public class WebDavDispatcherClass1 : IWebDavClass1
    {
        private readonly Lazy<IReadOnlyDictionary<XName, CreateDeadPropertyDelegate>> _defaultCreationMap;
        private readonly IDeadPropertyFactory _deadPropertyFactory;
        private readonly IMimeTypeDetector _mimeTypeDetector;
        private readonly IPropFindHandler? _propFindHandler;
        private readonly IPropPatchHandler? _propPatchHandler;
        private readonly IMkColHandler? _mkColHandler;
        private readonly IGetHandler? _getHandler;
        private readonly IHeadHandler? _headHandler;
        private readonly IPutHandler? _putHandler;
        private readonly IDeleteHandler? _deleteHandler;
        private readonly IOptionsHandler? _optionsHandler;
        private readonly ICopyHandler? _copyHandler;
        private readonly IMoveHandler? _moveHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavDispatcherClass1"/> class.
        /// </summary>
        /// <param name="class1Handlers">The WebDAV class 1 handlers.</param>
        /// <param name="context">The WebDAV context.</param>
        /// <param name="deadPropertyFactory">The factory to create dead properties.</param>
        /// <param name="mimeTypeDetector">The mime type detector for the <c>getmimetype</c> property.</param>
        /// <param name="options">The options for the WebDAV class 1 implementation.</param>
        public WebDavDispatcherClass1(
            IEnumerable<IClass1Handler> class1Handlers,
            IWebDavContext context,
            IDeadPropertyFactory deadPropertyFactory,
            IMimeTypeDetector mimeTypeDetector,
            IOptions<WebDavDispatcherClass1Options>? options)
        {
            _deadPropertyFactory = deadPropertyFactory;
            _mimeTypeDetector = mimeTypeDetector;
            var httpMethods = new HashSet<string>();

            foreach (var class1Handler in class1Handlers)
            {
                var handlerFound = false;

                if (class1Handler is IOptionsHandler optionsHandler)
                {
                    _optionsHandler = optionsHandler;
                    handlerFound = true;
                }

                if (class1Handler is IPropFindHandler propFindHandler)
                {
                    _propFindHandler = propFindHandler;
                    handlerFound = true;
                }

                if (class1Handler is IGetHandler getHandler)
                {
                    _getHandler = getHandler;
                    handlerFound = true;
                }

                if (class1Handler is IHeadHandler headHandler)
                {
                    _headHandler = headHandler;
                    handlerFound = true;
                }

                if (class1Handler is IPropPatchHandler propPatchHandler)
                {
                    _propPatchHandler = propPatchHandler;
                    handlerFound = true;
                }

                if (class1Handler is IPutHandler putHandler)
                {
                    _putHandler = putHandler;
                    handlerFound = true;
                }

                if (class1Handler is IMkColHandler mkColHandler)
                {
                    _mkColHandler = mkColHandler;
                    handlerFound = true;
                }

                if (class1Handler is IDeleteHandler deleteHandler)
                {
                    _deleteHandler = deleteHandler;
                    handlerFound = true;
                }

                if (class1Handler is ICopyHandler copyHandler)
                {
                    _copyHandler = copyHandler;
                    handlerFound = true;
                }

                if (class1Handler is IMoveHandler moveHandler)
                {
                    _moveHandler = moveHandler;
                    handlerFound = true;
                }

                if (!handlerFound)
                {
                    throw new NotSupportedException();
                }

                foreach (var httpMethod in class1Handler.HttpMethods)
                {
                    httpMethods.Add(httpMethod);
                }
            }

            HttpMethods = httpMethods.ToList();
            WebDavContext = context;

            OptionsResponseHeaders = new Dictionary<string, IEnumerable<string>>()
            {
                ["Allow"] = HttpMethods,
            };

            DefaultResponseHeaders = new Dictionary<string, IEnumerable<string>>()
            {
                ["DAV"] = new[] { "1" },
            };

            _defaultCreationMap = new Lazy<IReadOnlyDictionary<XName, CreateDeadPropertyDelegate>>(() => CreateDeadPropertiesMap(options?.Value ?? new WebDavDispatcherClass1Options()));
        }

        private delegate IDeadProperty CreateDeadPropertyDelegate(IPropertyStore store, IEntry entry, XName name);

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; }

        /// <inheritdoc />
        public IWebDavContext WebDavContext { get; }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, IEnumerable<string>> OptionsResponseHeaders { get; }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, IEnumerable<string>> DefaultResponseHeaders { get; }

        /// <inheritdoc />
        public Task<IWebDavResult> GetAsync(string path, CancellationToken cancellationToken)
        {
            if (_getHandler == null)
            {
                throw new NotSupportedException();
            }

            return _getHandler.GetAsync(path, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> HeadAsync(string path, CancellationToken cancellationToken)
        {
            if (_headHandler == null)
            {
                throw new NotSupportedException();
            }

            return _headHandler.HeadAsync(path, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> PutAsync(string path, Stream data, CancellationToken cancellationToken)
        {
            if (_putHandler == null)
            {
                throw new NotSupportedException();
            }

            return _putHandler.PutAsync(path, data, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> OptionsAsync(string path, CancellationToken cancellationToken)
        {
            if (_optionsHandler == null)
            {
                throw new NotSupportedException();
            }

            return _optionsHandler.OptionsAsync(path, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> PropFindAsync(string path, propfind? request, CancellationToken cancellationToken)
        {
            if (_propFindHandler == null)
            {
                throw new NotSupportedException();
            }

            return _propFindHandler.PropFindAsync(path, request, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> PropPatchAsync(string path, propertyupdate request, CancellationToken cancellationToken)
        {
            if (_propPatchHandler == null)
            {
                throw new NotSupportedException();
            }

            return _propPatchHandler.PropPatchAsync(path, request, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> DeleteAsync(string path, CancellationToken cancellationToken)
        {
            if (_deleteHandler == null)
            {
                throw new NotSupportedException();
            }

            return _deleteHandler.DeleteAsync(path, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> MkColAsync(string path, CancellationToken cancellationToken)
        {
            if (_mkColHandler == null)
            {
                throw new NotSupportedException();
            }

            return _mkColHandler.MkColAsync(path, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> CopyAsync(string path, Uri destination, CancellationToken cancellationToken)
        {
            if (_copyHandler == null)
            {
                throw new NotSupportedException();
            }

            return _copyHandler.CopyAsync(path, destination, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IWebDavResult> MoveAsync(string path, Uri destination, CancellationToken cancellationToken)
        {
            if (_moveHandler == null)
            {
                throw new NotSupportedException();
            }

            return _moveHandler.MoveAsync(path, destination, cancellationToken);
        }

        /// <inheritdoc />
        public IEnumerable<IUntypedReadableProperty> GetProperties(IEntry entry)
        {
            var propStore = entry.FileSystem.PropertyStore;

            yield return entry.GetResourceTypeProperty();

            foreach (var property in entry.GetLiveProperties())
            {
                yield return property;
            }

            if (propStore != null)
            {
                yield return _deadPropertyFactory.Create(propStore, entry, DisplayNameProperty.PropertyName);
            }

            if (entry is IDocument doc)
            {
                yield return new ContentLengthProperty(doc.Length);
                if (propStore != null)
                {
                    yield return _deadPropertyFactory
                        .Create(propStore, entry, GetContentLanguageProperty.PropertyName);
                    yield return _deadPropertyFactory
                        .Create(propStore, entry, GetContentTypeProperty.PropertyName);
                }
            }
            else
            {
                Debug.Assert(entry is ICollection, "entry is ICollection");
                yield return new ContentLengthProperty(0L);
                if (propStore != null)
                {
                    var contentType = _deadPropertyFactory.Create(propStore, entry, GetContentTypeProperty.PropertyName);
                    contentType.Init(new StringConverter().ToElement(GetContentTypeProperty.PropertyName, Utils.MimeTypesMap.FolderContentType));
                    yield return contentType;
                }
            }
        }

        /// <inheritdoc />
        public bool TryCreateDeadProperty(IPropertyStore store, IEntry entry, XName name, [NotNullWhen(true)] out IDeadProperty? deadProperty)
        {
            if (!_defaultCreationMap.Value.TryGetValue(name, out var createDeadProp))
            {
                deadProperty = null;
                return false;
            }

            deadProperty = createDeadProp(store, entry, name);
            return true;
        }

        private IReadOnlyDictionary<XName, CreateDeadPropertyDelegate> CreateDeadPropertiesMap(WebDavDispatcherClass1Options options)
        {
            var result = new Dictionary<XName, CreateDeadPropertyDelegate>()
            {
                [EntityTag.PropertyName] = (store, entry, name) => new GetETagProperty(store, entry),
                [DisplayNameProperty.PropertyName] = (store, entry, name) => new DisplayNameProperty(entry, store, options.HideExtensionForDisplayName),
                [GetContentLanguageProperty.PropertyName] = (store, entry, name) => new GetContentLanguageProperty(entry, store),
                [GetContentTypeProperty.PropertyName] = (store, entry, name) => new GetContentTypeProperty(entry, store, _mimeTypeDetector),
            };

            return result;
        }
    }
}
