using System;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Properties
{
    public static class EntryExtensions
    {
        public static IProperty GetResourceTypeProperty(this IEntry entry)
        {
            var coll = entry as ICollection;

            if (coll != null)
                return ResourceTypeProperty.Collection;

            var doc = entry as IDocument;
            if (doc != null)
                return ResourceTypeProperty.Document;

            throw new NotSupportedException();
        }
    }
}
