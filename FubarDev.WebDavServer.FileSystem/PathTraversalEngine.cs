using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.FileSystem
{
    public class PathTraversalEngine
    {
        private readonly IFileSystem _fileSystem;

        public PathTraversalEngine(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public Task<SelectionResult> TraverseAsync(string path, CancellationToken ct)
        {
            return TraverseAsync(SplitPath(path ?? string.Empty), ct);
        }

        public async Task<SelectionResult> TraverseAsync(IEnumerable<string> pathParts, CancellationToken ct)
        {
            var current = await _fileSystem.Root;
            return await TraverseAsync(current, pathParts, ct).ConfigureAwait(false);
        }

        public Task<SelectionResult> TraverseAsync(ICollection currentCollection, string path, CancellationToken ct)
        {
            return TraverseAsync(currentCollection, SplitPath(path ?? string.Empty), ct);
        }

        public Task<SelectionResult> TraverseAsync(ICollection currentCollection, IEnumerable<string> pathParts, CancellationToken ct)
        {
            return TraverseAsync(currentCollection, ToPathElements(pathParts), ct);
        }
        
        private async Task<SelectionResult> TraverseAsync(ICollection currentCollection, IEnumerable<PathElement> pathParts, CancellationToken ct)
        {
            var currentPathStack = new Stack<ICollection>();
            currentPathStack.Push(currentCollection);
            
            var pathPartsArr = pathParts.ToArray();

            for (var i = 0; i != pathPartsArr.Length; ++i)
            {
                var pathPart = pathPartsArr[i];
                if (pathPart.OriginalName == "./" || pathPart.OriginalName == string.Empty)
                    continue;
                if (pathPart.OriginalName == "../")
                {
                    currentCollection = currentPathStack.Pop();
                    continue;
                }

                var next = await currentCollection.GetChildAsync(pathPart.Name, ct).ConfigureAwait(false);
                if (next == null)
                {
                    // missing
                    var missingPathParts = pathPartsArr.Select(x => x.Name).Skip(i).ToArray();
                    return SelectionResult.CreateMissingDocumentOrCollection(currentCollection, missingPathParts);
                }

                var isDirectory = next is ICollection;
                if (pathPart.IsDirectory && !isDirectory)
                {
                    // file instead of directory
                    var missingPathParts = pathPartsArr.Select(x => x.Name).Skip(i).ToArray();
                    return SelectionResult.CreateMissingCollection(currentCollection, missingPathParts);
                }

                if (!isDirectory)
                {
                    // file found
                    return SelectionResult.Create(currentCollection, (IDocument) next);
                }

                currentCollection = (ICollection) next;
                currentPathStack.Push(currentCollection);
            }

            // directory found
            return SelectionResult.Create(currentCollection);
        }

        private static IEnumerable<string> SplitPath(string path)
        {
            var lastIndex = 0;
            var indexOfSlash = path.IndexOf('/');
            while (indexOfSlash != -1)
            {
                yield return path.Substring(lastIndex, indexOfSlash - lastIndex + 1);
                lastIndex = indexOfSlash + 1;
                indexOfSlash = path.IndexOf('/', lastIndex);
            }
            var remaining = path.Substring(lastIndex);
            if (!string.IsNullOrEmpty(remaining))
                yield return remaining;
        }

        private static IEnumerable<PathElement> ToPathElements(IEnumerable<string> pathParts)
        {
            foreach (var pathPart in pathParts)
            {
                var isDirectory = pathPart.EndsWith("/");
                var name = isDirectory ? pathPart.Substring(0, pathPart.Length - 1) : pathPart;
                yield return new PathElement(pathPart, name, isDirectory);
            }
        }

        private struct PathElement
        {
            public PathElement(string originalName, string name, bool isDirectory)
            {
                OriginalName = originalName;
                Name = name;
                IsDirectory = isDirectory;
            }

            public string OriginalName { get; }
            public string Name { get; }
            public bool IsDirectory { get; }
        }
    }
}