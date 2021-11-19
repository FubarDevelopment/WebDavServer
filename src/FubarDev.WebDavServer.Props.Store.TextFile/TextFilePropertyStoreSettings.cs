// <copyright file="TextFilePropertyStoreSettings.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.Props.Store.TextFile
{
    /// <summary>
    /// Special settings for the <see cref="TextFilePropertyStore"/>.
    /// </summary>
    public class TextFilePropertyStoreSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextFilePropertyStoreSettings"/> class.
        /// </summary>
        /// <param name="rootFolder">The root folder where the properties will be stored.</param>
        /// <param name="storeInRootOnly">Store all properties in the same JSON text file.</param>
        /// <param name="storeEntryName">The name of the JSON text file.</param>
        public TextFilePropertyStoreSettings(string rootFolder, bool storeInRootOnly, string storeEntryName)
        {
            RootFolder = rootFolder;
            StoreInRootOnly = storeInRootOnly;
            StoreEntryName = storeEntryName;
        }

        /// <summary>
        /// Gets the root folder where the properties will be stored.
        /// </summary>
        public string RootFolder { get; }

        /// <summary>
        /// Gets a value indicating whether all properties should be stored in the same JSON text file.
        /// </summary>
        public bool StoreInRootOnly { get; }

        /// <summary>
        /// Gets the name of the JSON text file.
        /// </summary>
        public string StoreEntryName { get; }
    }
}
