// <copyright file="DefaultMimeTypeDetector.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Utils
{
    /// <summary>
    /// The default implementation of a <see cref="IMimeTypeDetector"/>.
    /// </summary>
    public class DefaultMimeTypeDetector : IMimeTypeDetector
    {
        /// <inheritdoc />
        public bool TryDetect(IEntry entry, out string mimeType)
        {
            var fileExt = Path.GetExtension(entry.Name);
            if (string.IsNullOrEmpty(fileExt))
            {
                mimeType = MimeTypesMap.DefaultMimeType;
                return false;
            }

            return MimeTypesMap.TryGetMimeType(fileExt.Substring(1), out mimeType);
        }
    }
}
