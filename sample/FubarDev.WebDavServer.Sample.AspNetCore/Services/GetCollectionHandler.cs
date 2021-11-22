using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Fluid;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Handlers;

namespace FubarDev.WebDavServer.Sample.AspNetCore.Services
{
    public class GetCollectionHandler : IGetCollectionHandler
    {
        private static readonly TemplateOptions _templateOptions = CreateTemplateOptions();

        private readonly IWebDavContextAccessor _contextAccessor;
        private readonly string _assetPath;

        public GetCollectionHandler(
            IWebDavContextAccessor contextAccessor)
        {
            var assembly = typeof(GetCollectionHandler).Assembly;
            _assetPath = Path.Combine(Path.GetDirectoryName(assembly.Location)!, "Assets");
            _contextAccessor = contextAccessor;
        }

        /// <inheritdoc />
        public async Task<Stream> GetCollectionAsync(ICollection collection, CancellationToken cancellationToken)
        {
            var assetFileName = Path.Combine(_assetPath, "collection-contents.liquid");
            var source = await File.ReadAllTextAsync(assetFileName, cancellationToken);
            var parser = new FluidParser();
            if (!parser.TryParse(source, out var template, out var error))
            {
                throw new InvalidOperationException(error);
            }

            var context = _contextAccessor.WebDavContext;

            var children = await collection.GetChildrenAsync(cancellationToken);
            var entries = new List<ChildInfo>();
            foreach (var child in children)
            {
                var href = new Uri(context.PublicControllerUrl, child.Path);
                var name = child is ICollection
                    ? child.Name + "/"
                    : child.Name;

                entries.Add(new ChildInfo(name, href.ToString()));
            }
            
            var parentHref = new Uri(new Uri(context.PublicControllerUrl, collection.Path), "../");
            var path = Uri.UnescapeDataString(collection.Path.ToString()).TrimEnd('/') + "/";
            var collectionInfo = new CollectionInfo(
                collection.Name + "/",
                parentHref.ToString(),
                path);

            var templateContext = new TemplateContext(
                new
                {
                    Collection = collectionInfo,
                    Children = entries,
                },
                _templateOptions);
            
            var result = await template.RenderAsync(templateContext);
            return new MemoryStream(Encoding.UTF8.GetBytes(result));
        }

        private static TemplateOptions CreateTemplateOptions()
        {
            var templateOptions = new TemplateOptions();
            templateOptions.MemberAccessStrategy.Register<CollectionInfo>();
            templateOptions.MemberAccessStrategy.Register<ChildInfo>();
            return templateOptions;
        }
        
        // ReSharper disable NotAccessedPositionalProperty.Local
        private record CollectionInfo(string Name, string HrefParent, string Path);
        private record ChildInfo(string Name, string Href);
        // ReSharper restore NotAccessedPositionalProperty.Local
    }
}
