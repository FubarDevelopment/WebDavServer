// <copyright file="SQLitePropertyStore.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model.Headers;
using FubarDev.WebDavServer.Props.Dead;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using sqlitenet = SQLite;

namespace FubarDev.WebDavServer.Props.Store.SQLite
{
    /// <summary>
    /// The in-memory implementation of a property store.
    /// </summary>
    public class SQLitePropertyStore : PropertyStoreBase, IFileSystemPropertyStore
    {
        internal const string PropDbFileName = ".properties.db";

        private readonly ILogger<SQLitePropertyStore> _logger;

        private readonly SQLitePropertyStoreOptions _options;

        private readonly sqlitenet.SQLiteConnection _connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLitePropertyStore"/> class.
        /// </summary>
        /// <param name="deadPropertyFactory">The factory to create dead properties.</param>
        /// <param name="dbFileName">The file name of the SQLite database.</param>
        /// <param name="options">The options for this property store.</param>
        /// <param name="logger">The logger.</param>
        public SQLitePropertyStore(IDeadPropertyFactory deadPropertyFactory, string dbFileName, IOptions<SQLitePropertyStoreOptions>? options, ILogger<SQLitePropertyStore> logger)
            : base(deadPropertyFactory)
        {
            _options = options?.Value ?? new SQLitePropertyStoreOptions();
            _logger = logger;
            _connection = new sqlitenet.SQLiteConnection(dbFileName);
        }

        /// <inheritdoc />
        public override int Cost => _options.EstimatedCost;

        /// <inheritdoc />
        public bool IgnoreEntry(IEntry entry)
        {
            return entry.Parent?.Path.OriginalString == string.Empty && entry.Name == PropDbFileName;
        }

        /// <inheritdoc />
        public override Task<IReadOnlyCollection<XElement>> GetAsync(IEntry entry, CancellationToken cancellationToken)
        {
            var entries = GetAll(entry)
                .Where(x => x.Name != GetETagProperty.PropertyName)
                .ToList();
            return Task.FromResult<IReadOnlyCollection<XElement>>(entries);
        }

        /// <inheritdoc />
        public override Task SetAsync(IEntry entry, IEnumerable<XElement> elements, CancellationToken cancellationToken)
        {
            var elementsToSet = new List<XElement>();
            foreach (var element in elements)
            {
                if (element.Name == GetETagProperty.PropertyName)
                {
                    _logger.LogWarning("The ETag property must not be set using the property store.");
                    continue;
                }

                elementsToSet.Add(element);
            }

            SetAll(entry, elementsToSet);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override Task RemoveAsync(IEntry entry, CancellationToken cancellationToken)
        {
            _connection
                .CreateCommand("DELETE FROM props WHERE path=?", entry.Path.ToString())
                .ExecuteNonQuery();
            if (entry is ICollection)
            {
                _connection
                    .CreateCommand("DELETE FROM props WHERE path like '?%'", entry.Path.ToString())
                    .ExecuteNonQuery();
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override Task<IReadOnlyCollection<bool>> RemoveAsync(IEntry entry, IEnumerable<XName> keys, CancellationToken cancellationToken)
        {
            var result = new List<bool>();
            var entries = GetAll(entry)
                .ToDictionary(x => x.Name);
            foreach (var key in keys)
            {
                if (!entries.TryGetValue(key, out _))
                {
                    continue;
                }

                if (key == GetETagProperty.PropertyName)
                {
                    _logger.LogWarning("The ETag property must not be set using the property store.");
                    result.Add(false);
                }
                else
                {
                    var id = CreateId(key, entry);
                    var affected = _connection
                        .CreateCommand("DELETE FROM props WHERE id=?", id)
                        .ExecuteNonQuery();
                    result.Add(affected != 0);
                }
            }

            return Task.FromResult<IReadOnlyCollection<bool>>(result);
        }

        /// <inheritdoc />
        protected override Task<EntityTag> GetDeadETagAsync(IEntry entry, CancellationToken cancellationToken)
        {
            var key = GetETagProperty.PropertyName;
            var prop = _connection
                .CreateCommand("SELECT * FROM props WHERE id=?", key)
                .ExecuteQuery<PropertyEntry>()
                .FirstOrDefault();
            if (prop == null)
            {
                var etag = new EntityTag(false);
                prop = new PropertyEntry()
                {
                    Id = CreateId(key, entry),
                    Language = null,
                    Path = entry.Path.ToString(),
                    XmlName = key.ToString(),
                    Value = etag.ToXml().ToString(SaveOptions.OmitDuplicateNamespaces),
                };

                _connection.Insert(prop);
                return Task.FromResult(etag);
            }

            var result = EntityTag.FromXml(XElement.Parse(prop.Value));
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        protected override Task<EntityTag> UpdateDeadETagAsync(IEntry entry, CancellationToken cancellationToken)
        {
            var etag = new EntityTag(false);
            var key = GetETagProperty.PropertyName;
            var prop = new PropertyEntry()
            {
                Id = CreateId(key, entry),
                Language = null,
                Path = entry.Path.ToString(),
                XmlName = key.ToString(),
                Value = etag.ToXml().ToString(SaveOptions.OmitDuplicateNamespaces),
            };

            _connection.InsertOrReplace(prop);
            return Task.FromResult(etag);
        }

        private static string CreateId(XName key, IEntry entry)
        {
            return $"{key}:{entry.Path}";
        }

        private IReadOnlyCollection<XElement> GetAll(IEntry entry)
        {
            var path = entry.Path.ToString();
            var entries = _connection.Table<PropertyEntry>().Where(x => x.Path == path).ToList();
            return entries.Select(x => XElement.Parse(x.Value)).ToList();
        }

        private void SetAll(IEntry entry, IEnumerable<XElement> elements)
        {
            var isEtagEntry = entry is IEntityTagEntry;
            _connection.RunInTransaction(
                () =>
                {
                    foreach (var element in elements)
                    {
                        if (isEtagEntry && element.Name == GetETagProperty.PropertyName)
                        {
                            _logger.LogWarning("The ETag property must not be set using the property store.");
                            continue;
                        }

                        var key = element.Name;
                        var item = new PropertyEntry()
                        {
                            Id = CreateId(key, entry),
                            Path = entry.Path.ToString(),
                            XmlName = key.ToString(),
                            Language = element.Attribute(XNamespace.Xml + "lang")?.Value,
                            Value = element.ToString(SaveOptions.OmitDuplicateNamespaces),
                        };

                        _connection.InsertOrReplace(item);
                    }
                });
        }
    }
}
