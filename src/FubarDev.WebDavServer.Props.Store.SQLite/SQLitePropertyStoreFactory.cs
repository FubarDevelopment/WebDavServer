// <copyright file="SQLitePropertyStoreFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.IO;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Utils;

using Microsoft.Extensions.DependencyInjection;

using sqlitenet = SQLite;

namespace FubarDev.WebDavServer.Props.Store.SQLite
{
    /// <summary>
    /// The factory for the <see cref="SQLitePropertyStore"/>.
    /// </summary>
    public class SQLitePropertyStoreFactory : IPropertyStoreFactory
    {
        private readonly IWebDavContext _webDavContext;

        private readonly IDeadPropertyFactory _deadPropertyFactory;

        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLitePropertyStoreFactory"/> class.
        /// </summary>
        /// <param name="webDavContext">The WebDAV request context.</param>
        /// <param name="deadPropertyFactory">The factory for dead properties.</param>
        /// <param name="serviceProvider">The current service provider.</param>
        public SQLitePropertyStoreFactory(
            IWebDavContext webDavContext,
            IDeadPropertyFactory deadPropertyFactory,
            IServiceProvider serviceProvider)
        {
            _webDavContext = webDavContext;
            _deadPropertyFactory = deadPropertyFactory;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Ensures that a database with the given file name exists.
        /// </summary>
        /// <param name="dbFileName">The file name of the database.</param>
        public static void EnsureDatabaseExists(string dbFileName)
        {
            if (File.Exists(dbFileName))
            {
                using var conn = new sqlitenet.SQLiteConnection(dbFileName);
                CreateDatabaseTables(conn);

                return;
            }

            CreateDatabase(dbFileName);
        }

        /// <summary>
        /// Creates a new database.
        /// </summary>
        /// <param name="dbFileName">The file name of the database.</param>
        public static void CreateDatabase(string dbFileName)
        {
            if (File.Exists(dbFileName))
            {
                File.Delete(dbFileName);
            }

            var dbFileFolder = Path.GetDirectoryName(dbFileName);
            Debug.Assert(dbFileFolder != null, "dbFileFolder != null");
            Directory.CreateDirectory(dbFileFolder);
            using var conn = new sqlitenet.SQLiteConnection(dbFileName);
            CreateDatabaseTables(conn);
        }

        /// <inheritdoc />
        public IPropertyStore Create(IFileSystem fileSystem)
        {
            string dbPath;
            if (fileSystem is ILocalFileSystem fs)
            {
                string dbFileName;
                if (fs.HasSubfolders)
                {
                    dbFileName = SQLitePropertyStore.PropDbFileName;
                }
                else
                {
                    dbFileName = _webDavContext.User.Identity.IsAnonymous() ? "anonymous.db" : $"{_webDavContext.User.Identity.Name}.db";
                }

                dbPath = Path.Combine(fs.RootDirectoryPath, dbFileName);
            }
            else
            {
                var userHomePath = SystemInfo.GetUserHomePath(_webDavContext.User);
                dbPath = Path.Combine(userHomePath, ".webdav", "properties.db");
            }

            EnsureDatabaseExists(dbPath);

            return ActivatorUtilities.CreateInstance<SQLitePropertyStore>(
                _serviceProvider,
                _deadPropertyFactory,
                dbPath);
        }

        /// <summary>
        /// Creates the database tables.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        private static void CreateDatabaseTables(sqlitenet.SQLiteConnection connection)
        {
            connection.CreateTable<PropertyEntry>(sqlitenet.CreateFlags.AllImplicit);
        }
    }
}
