using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Handlers;
using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.DefaultHandlers
{
    public class PropFindHandler : IPropFindHandler
    {
        public PropFindHandler(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        public IFileSystem FileSystem { get; }

        public Depth Depth { get; set; } = Depth.Infinity;

        public IWebDavResult HandleAsync(string path, Propfind request, CancellationToken cancellationToken)
        {
            if (Depth == Depth.Infinity)
                return new WebDavResult<Error1>(WebDavStatusCodes.Forbidden, new Error1()
                {
                    ItemsElementName = new[] {ItemsChoiceType2.PropfindFiniteDepth,},
                    Items = new[] {new object(),}
                });

            var elements = new List<IEntry>();
            var remainingDepth = Depth.OrderValue;

            throw new NotImplementedException();
        }
    }
}
