using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem
{
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

        public SelectionResultType ResultType { get; }

        public bool IsMissing =>
            ResultType == SelectionResultType.MissingCollection ||
            ResultType == SelectionResultType.MissingDocumentOrCollection;

        [CanBeNull]
        public ICollection Collection { get; }

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
                result.Append(string.Join("/", MissingNames.Select(Uri.EscapeDataString)));
                if (ResultType == SelectionResultType.MissingCollection)
                    result.Append("/");
                return new Uri(result.ToString(), UriKind.Relative);
            }
        }

        [CanBeNull]
        public IEntry TargetEntry
        {
            get
            {
                if (IsMissing)
                    throw new InvalidOperationException();
                if (ResultType == SelectionResultType.FoundDocument)
                    return Document;
                return Collection;
            }
        }

        [NotNull]
        public static SelectionResult Create([NotNull] ICollection collection, [NotNull] IDocument document)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            return new SelectionResult(SelectionResultType.FoundDocument, collection, document, null);
        }

        [NotNull]
        public static SelectionResult Create([NotNull] ICollection collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            return new SelectionResult(SelectionResultType.FoundCollection, collection, null, null);
        }

        [NotNull]
        public static SelectionResult CreateMissingDocumentOrCollection([NotNull] ICollection collection, [NotNull][ItemNotNull] IReadOnlyCollection<string> pathEntries)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (pathEntries == null)
                throw new ArgumentNullException(nameof(pathEntries));
            return new SelectionResult(SelectionResultType.MissingDocumentOrCollection, collection, null, pathEntries);
        }

        [NotNull]
        public static SelectionResult CreateMissingCollection([NotNull] ICollection collection, [NotNull][ItemNotNull] IReadOnlyCollection<string> pathEntries)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (pathEntries == null)
                throw new ArgumentNullException(nameof(pathEntries));
            return new SelectionResult(SelectionResultType.MissingCollection, collection, null, pathEntries);
        }
    }
}
