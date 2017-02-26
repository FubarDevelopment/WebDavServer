// <copyright file="TextFilePropertyStore.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model.Headers;
using FubarDev.WebDavServer.Props.Dead;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using Polly;

namespace FubarDev.WebDavServer.Props.Store.TextFile
{
    public class TextFilePropertyStore : PropertyStoreBase, IFileSystemPropertyStore
    {
        private readonly Policy<string> _fileReadPolicy;

        private readonly Policy _fileWritePolicy;

        private readonly IMemoryCache _cache;

        private readonly ILogger<TextFilePropertyStore> _logger;

        private readonly TextFilePropertyStoreOptions _options;

        private readonly string _storeEntryName = ".properties";

        public TextFilePropertyStore(IOptions<TextFilePropertyStoreOptions> options, IMemoryCache cache, ILogger<TextFilePropertyStore> logger, IDeadPropertyFactory deadPropertyFactory = null)
            : this(options.Value, cache, deadPropertyFactory ?? new DeadPropertyFactory(), logger)
        {
        }

        public TextFilePropertyStore(TextFilePropertyStoreOptions options, IMemoryCache cache, IDeadPropertyFactory deadPropertyFactory, ILogger<TextFilePropertyStore> logger)
            : this(options, cache, deadPropertyFactory, options.RootFolder, logger)
        {
        }

        public TextFilePropertyStore(TextFilePropertyStoreOptions options, IMemoryCache cache, IDeadPropertyFactory deadPropertyFactory, string rootFolder, ILogger<TextFilePropertyStore> logger)
            : base(deadPropertyFactory)
        {
            _cache = cache;
            _logger = logger;
            _options = options;
            RootPath = rootFolder;
            var rnd = new Random();
            _fileReadPolicy = Policy<string>
                .Handle<IOException>()
                .WaitAndRetry(100, n => TimeSpan.FromMilliseconds(100 + rnd.Next(-10, 10)));
            _fileWritePolicy = Policy
                .Handle<IOException>()
                .WaitAndRetry(100, n => TimeSpan.FromMilliseconds(100 + rnd.Next(-10, 10)));
        }

        public override int Cost => _options.EstimatedCost;

        public string RootPath { get; set; }

        public bool IgnoreEntry(IEntry entry)
        {
            return entry is IDocument && entry.Name == _storeEntryName;
        }

        public override Task<IReadOnlyCollection<XElement>> GetAsync(IEntry entry, CancellationToken cancellationToken)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace($"Get properties for {entry.Path}");

            var storeData = Load(entry, false, cancellationToken);
            EntryInfo info;
            IReadOnlyCollection<XElement> result;
            if (storeData.Entries.TryGetValue(GetEntryKey(entry), out info))
            {
                result = info.Attributes
                    .Where(x => x.Key != GetETagProperty.PropertyName)
                    .Select(x => x.Value)
                    .ToList();
            }
            else
            {
                result = new XElement[0];
            }

            return Task.FromResult(result);
        }

        public override Task SetAsync(IEntry entry, IEnumerable<XElement> elements, CancellationToken cancellationToken)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace($"Set properties for {entry.Path}");

            var info = GetInfo(entry, cancellationToken) ?? new EntryInfo();
            foreach (var element in elements)
            {
                if (element.Name == GetETagProperty.PropertyName)
                {
                    _logger.LogWarning("The ETag property must not be set using the property store.");
                    continue;
                }

                info.Attributes[element.Name] = element;
            }

            UpdateInfo(entry, info, cancellationToken);
            return Task.FromResult(0);
        }

        public override Task<IReadOnlyCollection<bool>> RemoveAsync(IEntry entry, IEnumerable<XName> names, CancellationToken cancellationToken)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace($"Remove properties for {entry.Path}");

            var info = GetInfo(entry, cancellationToken) ?? new EntryInfo();
            var result = new List<bool>();
            foreach (var name in names)
            {
                if (name == GetETagProperty.PropertyName)
                {
                    _logger.LogWarning("The ETag property must not be set using the property store.");
                    result.Add(false);
                }
                else
                {
                    result.Add(info.Attributes.Remove(name));
                }
            }

            UpdateInfo(entry, info, cancellationToken);
            return Task.FromResult<IReadOnlyCollection<bool>>(result);
        }

        public override Task RemoveAsync(IEntry entry, CancellationToken cancellationToken)
        {
            var fileName = GetFileNameFor(entry);
            if (!File.Exists(fileName))
                return Task.FromResult(0);

            var storeData = Load(entry, false, cancellationToken);
            var entryKey = GetEntryKey(entry);
            if (storeData.Entries.Remove(entryKey))
            {
                Save(entry, storeData, cancellationToken);
            }

            return Task.FromResult(0);
        }

        protected override Task<EntityTag> GetDeadETagAsync(IEntry entry, CancellationToken cancellationToken)
        {
            var storeData = Load(entry, false, cancellationToken);
            var entryKey = GetEntryKey(entry);
            EntryInfo info;
            if (!storeData.Entries.TryGetValue(entryKey, out info))
            {
                info = new EntryInfo();
                storeData.Entries.Add(entryKey, info);
            }

            XElement etagElement;
            if (!info.Attributes.TryGetValue(GetETagProperty.PropertyName, out etagElement))
            {
                var etag = new EntityTag(false);
                etagElement = etag.ToXml();
                info.Attributes[etagElement.Name] = etagElement;

                Save(entry, storeData, cancellationToken);

                return Task.FromResult(etag);
            }

            return Task.FromResult(EntityTag.FromXml(etagElement));
        }

        protected override Task<EntityTag> UpdateDeadETagAsync(IEntry entry, CancellationToken cancellationToken)
        {
            var storeData = Load(entry, false, cancellationToken);
            var entryKey = GetEntryKey(entry);
            EntryInfo info;
            if (!storeData.Entries.TryGetValue(entryKey, out info))
            {
                info = new EntryInfo();
                storeData.Entries.Add(entryKey, info);
            }

            var etag = new EntityTag(false);
            var etagElement = etag.ToXml();
            info.Attributes[etagElement.Name] = etagElement;

            Save(entry, storeData, cancellationToken);

            return Task.FromResult(etag);
        }

        private static string GetEntryKey(IEntry entry)
        {
            if (entry is ICollection)
                return ".";
            return entry.Name.ToLower();
        }

        private void UpdateInfo(IEntry entry, EntryInfo info, CancellationToken cancellationToken)
        {
            var storeData = Load(entry, true, cancellationToken);
            var entryKey = GetEntryKey(entry);
            storeData.Entries[entryKey] = info;
            Save(entry, storeData, cancellationToken);
        }

        private EntryInfo GetInfo(IEntry entry, CancellationToken cancellationToken)
        {
            var storeData = Load(entry, false, cancellationToken);

            EntryInfo info;
            if (!storeData.Entries.TryGetValue(GetEntryKey(entry), out info))
                info = new EntryInfo();

            return info;
        }

        private void Save(IEntry entry, StoreData data, CancellationToken cancellationToken)
        {
            Save(GetFileNameFor(entry), data, cancellationToken);
        }

        private StoreData Load(IEntry entry, bool useCache, CancellationToken cancellationToken)
        {
            return Load(GetFileNameFor(entry), useCache, cancellationToken);
        }

        private void Save(string fileName, StoreData data, CancellationToken cancellationToken)
        {
            try
            {
                var key = fileName.ToLower();
                _fileWritePolicy.Execute(ct => File.WriteAllText(fileName, JsonConvert.SerializeObject(data)), cancellationToken);
                _cache.Set(key, data);
            }
            catch (Exception)
            {
                // Ignore all exceptions for directories that cannot be modified
            }
        }

        private StoreData Load(string fileName, bool useCache, CancellationToken cancellationToken)
        {
            if (!File.Exists(fileName))
                return new StoreData();

            var key = fileName.ToLower();
            if (!useCache)
            {
                var result = JsonConvert.DeserializeObject<StoreData>(
                    _fileReadPolicy.Execute(ct => File.ReadAllText(fileName), cancellationToken));
                _cache.Set(key, result);
                return result;
            }

            return _cache.GetOrCreate(key, ce => JsonConvert.DeserializeObject<StoreData>(
                _fileReadPolicy.Execute(ct => File.ReadAllText(fileName), cancellationToken)));
        }

        private string GetFileNameFor(IEntry entry)
        {
            var isCollection = entry is ICollection;
            var path = GetFileSystemPath(entry);
            string result;
            if (isCollection)
            {
                result = Path.Combine(path, _storeEntryName);
            }
            else
            {
                var directoryName = Path.GetDirectoryName(path);
                Debug.Assert(directoryName != null, "directoryName != null");
                result = Path.Combine(directoryName, _storeEntryName);
            }

            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace($"Property store file name for {entry.Path} is {result}");

            return result;
        }

        private string GetFileSystemPath(IEntry entry)
        {
            var names = new List<string>();

            while (entry.Parent != null)
            {
                if (!string.IsNullOrEmpty(entry.Name))
                    names.Add(entry.Name);
                entry = entry.Parent;
            }

            names.Reverse();
            var result = Path.Combine(RootPath, Path.Combine(names.ToArray()));

            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace($"File system path for {entry.Path} is {result}");

            return result;
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class StoreData
        {
            public IDictionary<string, EntryInfo> Entries { get; } = new Dictionary<string, EntryInfo>();
        }

        private class EntryInfo
        {
            public IDictionary<XName, XElement> Attributes { get; } = new Dictionary<XName, XElement>();
        }
    }
}
