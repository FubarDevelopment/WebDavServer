// <copyright file="SQLiteFileSystemException.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using sqlite3 = SQLite;

namespace FubarDev.WebDavServer.FileSystem.SQLite
{
    public class SQLiteFileSystemException : Exception
    {
        public SQLiteFileSystemException()
        {
        }

        public SQLiteFileSystemException(SQLitePCL.sqlite3 db)
            : this(sqlite3.SQLite3.GetErrmsg(db))
        {
        }

        public SQLiteFileSystemException(string message)
            : base(message)
        {
        }

        public SQLiteFileSystemException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
