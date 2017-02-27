// <copyright file="SelectionResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem
{
    /// <summary>
    /// The result of a <see cref="IFileSystem.SelectAsync"/> operation
    /// </summary>
    public class SelectionResult
    {
        private static readonly IReadOnlyCollection<string> _emptyCollection = new string[0];
        private readonly IDocument _document;
        private readonly IReadOnlyCollection<string> _pathEntries;

        internal SelectionResult(SelectionResultType resultType, [NotNull] ICollection collection, IDocument document, [CanBeNull] IReadOnlyCollection<string> pathEntries)
        {
            ResultType = resultType;
            Collection = collection;
            _document = document;
            _pathEntries = pathEntries ?? _emptyCollection;
        }

        /// <summary>
        /// Gets the type of the result
        /// </summary>
        public SelectionResultType ResultType { get; }

        /// <summary>
        /// Gets a value indicating whether there was a missing path part?
        /// </summary>
        public bool IsMissing =>
            ResultType == SelectionResultType.MissingCollection ||
            ResultType == SelectionResultType.MissingDocumentOrCollection;

        /// <summary>
        /// Gets the collection of the search result.
        /// </summary>
        /// <remarks>
        /// When <see cref="ResultType"/> is <see cref="SelectionResultType.FoundCollection"/>, this is the found collection.
        /// When <see cref="ResultType"/> is <see cref="SelectionResultType.FoundDocument"/>, this is the parent collection.
        /// Otherwise, this is the last found collection.
        /// </remarks>
        [NotNull]
        public ICollection Collection { get; }

        /// <summary>
        /// Gets the found document
        /// </summary>
        /// <remarks>
        /// This property is only valid when <see cref="ResultType"/> is <see cref="SelectionResultType.FoundDocument"/>.
        /// </remarks>
        [CanBeNull]
        public IDocument Document
        {
            get
            {
                if (ResultType != SelectionResultType.FoundDocument)
                    throw new InvalidOperationException();
                return _document;
            }
        }

        /// <summary>
        /// Gets the collection of missing child elements
        /// </summary>
        /// <remarks>
        /// This is only valid, when <see cref="IsMissing"/> is <see langword="true"/>.
        /// </remarks>
        [NotNull]
        [ItemNotNull]
        public IReadOnlyCollection<string> MissingNames
        {
            get
            {
                if (ResultType != SelectionResultType.MissingCollection && ResultType != SelectionResultType.MissingDocumentOrCollection)
                    throw new InvalidOperationException();
                return _pathEntries;
            }
        }

        /// <summary>
        /// Gets the full root-relative path of the element that was searched
        /// </summary>
        [NotNull]
        public Uri FullPath
        {
            get
            {
                switch (ResultType)
                {
                    case SelectionResultType.FoundCollection:
                        Debug.Assert(Collection != null, "Collection != null");
                        return Collection.Path;
                    case SelectionResultType.FoundDocument:
                        Debug.Assert(Document != null, "Document != null");
                        return Document.Path;
                }

                var result = new StringBuilder();
                Debug.Assert(Collection != null, "Collection != null");
                result.Append(Collection.Path.OriginalString);
                Debug.Assert(MissingNames != null, "MissingNames != null");
                result.Append(string.Join("/", MissingNames.Select(n => n.UriEscape())));
                if (ResultType == SelectionResultType.MissingCollection)
                    result.Append("/");
                return new Uri(result.ToString(), UriKind.Relative);
            }
        }

        /// <summary>
        /// Gets the found target entry
        /// </summary>
        /// <remarks>
        /// This is only valid when <see cref="IsMissing"/> is <see langword="false"/>.
        /// </remarks>
        [NotNull]
        public IEntry TargetEntry
        {
            get
            {
                if (IsMissing)
                    throw new InvalidOperationException();
                if (ResultType == SelectionResultType.FoundDocument)
                {
                    Debug.Assert(Document != null, "Document != null");
                    return Document;
                }

                return Collection;
            }
        }

        /// <summary>
        /// Gets the file system of the found element or the last found collection
        /// </summary>
        [NotNull]
        public IFileSystem TargetFileSystem => ((IEntry)_document ?? Collection).FileSystem;

        /// <summary>
        /// Creates a selection result for a found document
        /// </summary>
        /// <param name="collection">The parent collection</param>
        /// <param name="document">The found document</param>
        /// <returns>The created selection result</returns>
        [NotNull]
        public static SelectionResult Create([NotNull] ICollection collection, [NotNull] IDocument document)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            return new SelectionResult(SelectionResultType.FoundDocument, collection, document, null);
        }

        /// <summary>
        /// Creates a selection result for a found collection
        /// </summary>
        /// <param name="collection">The found collection</param>
        /// <returns>The created selection result</returns>
        [NotNull]
        public static SelectionResult Create([NotNull] ICollection collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            return new SelectionResult(SelectionResultType.FoundCollection, collection, null, null);
        }

        /// <summary>
        /// Creates a selection for a missing document or collection
        /// </summary>
        /// <param name="collection">The last found collection</param>
        /// <param name="pathEntries">The missing path elements</param>
        /// <returns>The created selection result</returns>
        [NotNull]
        public static SelectionResult CreateMissingDocumentOrCollection([NotNull] ICollection collection, [NotNull] [ItemNotNull] IReadOnlyCollection<string> pathEntries)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (pathEntries == null)
                throw new ArgumentNullException(nameof(pathEntries));
            return new SelectionResult(SelectionResultType.MissingDocumentOrCollection, collection, null, pathEntries);
        }

        /// <summary>
        /// Creates a selection for a missing collection
        /// </summary>
        /// <param name="collection">The last found collection</param>
        /// <param name="pathEntries">The missing path elements</param>
        /// <returns>The created selection result</returns>
        [NotNull]
        public static SelectionResult CreateMissingCollection([NotNull] ICollection collection, [NotNull] [ItemNotNull] IReadOnlyCollection<string> pathEntries)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (pathEntries == null)
                throw new ArgumentNullException(nameof(pathEntries));
            return new SelectionResult(SelectionResultType.MissingCollection, collection, null, pathEntries);
        }
    }
}
