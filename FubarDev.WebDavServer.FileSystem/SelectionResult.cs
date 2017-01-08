using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem
{
    public class SelectionResult
    {
        private readonly IDocument _document;
        private readonly IReadOnlyCollection<string> _pathEntries;

        internal SelectionResult(SelectionResultType resultType, [NotNull] ICollection collection, IDocument document, IReadOnlyCollection<string> pathEntries)
        {
            ResultType = resultType;
            Collection = collection;
            _document = document;
            _pathEntries = pathEntries;
        }

        public SelectionResultType ResultType { get; }

        [NotNull]
        public ICollection Collection { get; }

        [NotNull]
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
        public IReadOnlyCollection<string> PathEntries
        {
            get
            {
                if (ResultType != SelectionResultType.MissingCollection && ResultType != SelectionResultType.MissingDocumentOrCollection)
                    throw new InvalidOperationException();
                return _pathEntries;
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
