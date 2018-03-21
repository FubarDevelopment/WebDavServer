using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Props.Store;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props
{
    /// <summary>
    /// Initzialize a newly created document or collection with properties
    /// </summary>
    public interface IEntryPropertyInitializer
    {
        /// <summary>
        /// Initialize a new document with properties
        /// </summary>
        /// <param name="document">The document to create the properties for</param>
        /// <param name="propertyStore">The property store</param>
        /// <param name="context">The PUT request context</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>the task</returns>
        [NotNull]
        Task CreatePropertiesAsync(
            [NotNull] IDocument document,
            [NotNull] IPropertyStore propertyStore,
            [NotNull] IWebDavContext context,
            CancellationToken cancellationToken);

        /// <summary>
        /// Initialize a new collection with properties
        /// </summary>
        /// <param name="collection">The collection to create the properties for</param>
        /// <param name="propertyStore">The property store</param>
        /// <param name="context">The MKCOL request context</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>the task</returns>
        [NotNull]
        Task CreatePropertiesAsync(
            [NotNull] ICollection collection,
            [NotNull] IPropertyStore propertyStore,
            [NotNull] IWebDavContext context,
            CancellationToken cancellationToken);
    }
}
