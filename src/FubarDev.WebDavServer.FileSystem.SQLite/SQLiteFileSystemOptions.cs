// <copyright file="SQLiteFileSystemOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.Utils;

namespace FubarDev.WebDavServer.FileSystem.SQLite
{
    /// <summary>
    /// The options for the <see cref="SQLiteFileSystemFactory"/> and <see cref="SQLiteFileSystem"/>.
    /// </summary>
    public class SQLiteFileSystemOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteFileSystemOptions"/> class.
        /// </summary>
        public SQLiteFileSystemOptions()
        {
            var info = SystemInfo.GetHomePath();
            RootPath = info.RootPath;
        }

        /// <summary>
        /// Gets or sets the home path for all users.
        /// </summary>
        public string RootPath { get; set; }
    }
}
