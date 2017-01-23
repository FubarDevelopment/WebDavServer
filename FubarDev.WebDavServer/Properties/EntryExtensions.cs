using System;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Properties.Live;

namespace FubarDev.WebDavServer.Properties
{
    public static class EntryExtensions
    {
        public static ILiveProperty GetResourceTypeProperty(this IEntry entry)
        {
            var coll = entry as ICollection;

            if (coll != null)
                return ResourceTypeProperty.GetCollectionResourceType();

            var doc = entry as IDocument;
            if (doc != null)
                return ResourceTypeProperty.GetDocumentResourceType();

            throw new NotSupportedException();
        }
    }
}
