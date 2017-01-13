using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Handlers;
using FubarDev.WebDavServer.Model;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.DefaultHandlers
{
    public class PropFindHandler : IPropFindHandler
    {
        public PropFindHandler(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        public IFileSystem FileSystem { get; }

        public async Task<IWebDavResult> HandleAsync(string path, Propfind request, Depth depth, CancellationToken cancellationToken)
        {
            if (depth == Depth.Infinity)
            {
                // Not supported yet
                return new WebDavResult<Error1>(WebDavStatusCodes.Forbidden, new Error1()
                {
                    ItemsElementName = new[] {ItemsChoiceType2.PropfindFiniteDepth,},
                    Items = new[] {new object(),}
                });
            }

            var selectionResult = await FileSystem.SelectAsync(path, cancellationToken).ConfigureAwait(false);
            if (selectionResult.IsMissing)
            {
                throw new WebDavException(WebDavStatusCodes.NotFound);
            }

            switch (request.ItemsElementName[0])
            {
                case ItemsChoiceType.Allprop:
                    return await HandleAllProp(request, selectionResult, depth).ConfigureAwait(false);
            }

            throw new WebDavException(WebDavStatusCodes.Forbidden);
        }

        private Task<IWebDavResult> HandleAllProp(Propfind request, SelectionResult selectionResult, Depth depth)
        {
            var include = request.ItemsElementName.Select((x, i) => Tuple.Create(x, i)).Where(x => x.Item1 == ItemsChoiceType.Include).Select(x => (Include)request.Items[x.Item2]).FirstOrDefault();
            return HandleAllProp(include, selectionResult, depth);
        }

        private async Task<IWebDavResult> HandleAllProp([CanBeNull] Include include, [NotNull] SelectionResult selectionResult, Depth depth)
        {
            throw new NotImplementedException();
        }
    }
}
