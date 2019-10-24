// <copyright file="SQLitePropertyStoreFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Diagnostics;
using System.IO;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Utils;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using sqlitenet = SQLite;

namespace FubarDev.WebDavServer.Props.Store.SQLite
{
    /// <summary>
    /// The factory for the <see cref="SQLitePropertyStore"/>.
    /// </summary>
    public class SQLitePropertyStoreFactory : IPropertyStoreFactory
    {
        private readonly IWebDavContext _webDavContext;

        private readonly ILogger<SQLitePropertyStore> _logger;

        private readonly IDeadPropertyFactory _deadPropertyFactory;

        private readonly IOptions<SQLitePropertyStoreOptions>? _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLitePropertyStoreFactory"/> class.
        /// </summary>
        /// <param name="webDavContext">The WebDAV request context.</param>
        /// <param name="logger">The logger for the property store factory.</param>
        /// <param name="deadPropertyFactory">The factory for dead properties.</param>
        /// <param name="options">The options for this property store.</param>
        public SQLitePropertyStoreFactory(IWebDavContext webDavContext, ILogger<SQLitePropertyStore> logger, IDeadPropertyFactory deadPropertyFactory, IOptions<SQLitePropertyStoreOptions>? options = null)
        {
            _webDavContext = webDavContext;
            _logger = logger;
            _deadPropertyFactory = deadPropertyFactory;
            _options = options;
        }

        /// <summary>
        /// Ensures that a database with the given file name exists.
        /// </summary>
        /// <param name="dbFileName">The file name of the database.</param>
        public static void EnsureDatabaseExists(string dbFileName)
        {
            if (File.Exists(dbFileName))
            {
                using (var conn = new sqlitenet.SQLiteConnection(dbFileName))
                {
                    CreateDatabaseTables(conn);
                }

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
            using (var conn = new sqlitenet.SQLiteConnection(dbFileName))
            {
                CreateDatabaseTables(conn);
            }
        }

        /// <inheritdoc />
        public IPropertyStore Create(IFileSystem fileSystem)
        {
            string dbPath;
            var fs = fileSystem as ILocalFileSystem;
            if (fs != null)
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

            return new SQLitePropertyStore(_deadPropertyFactory, dbPath, _options, _logger);
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
