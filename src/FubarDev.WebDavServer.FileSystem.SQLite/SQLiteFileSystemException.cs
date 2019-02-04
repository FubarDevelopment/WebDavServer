// <copyright file="SQLiteFileSystemException.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using sqlite3 = SQLite;

namespace FubarDev.WebDavServer.FileSystem.SQLite
{
    /// <summary>
    /// An exception to be thrown, because the internal exception of SQLite.NET isn't accessible.
    /// </summary>
    public class SQLiteFileSystemException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteFileSystemException"/> class.
        /// </summary>
        public SQLiteFileSystemException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteFileSystemException"/> class.
        /// </summary>
        /// <param name="db">The SQLite DB handle.</param>
        public SQLiteFileSystemException(SQLitePCL.sqlite3 db)
            : this(sqlite3.SQLite3.GetErrmsg(db))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteFileSystemException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public SQLiteFileSystemException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteFileSystemException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public SQLiteFileSystemException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
