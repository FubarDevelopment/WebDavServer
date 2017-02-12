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
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Props.Dead;

using Microsoft.Extensions.Caching.Memory;
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

        private readonly TextFilePropertyStoreOptions _options;

        private readonly string _storeEntryName = ".properties";

        public TextFilePropertyStore(IOptions<TextFilePropertyStoreOptions> options, IMemoryCache cache, IDeadPropertyFactory deadPropertyFactory = null)
            : this(options.Value, cache, deadPropertyFactory ?? new DeadPropertyFactory())
        {
        }

        public TextFilePropertyStore(TextFilePropertyStoreOptions options, IMemoryCache cache, IDeadPropertyFactory deadPropertyFactory)
            : this(options, cache, deadPropertyFactory, options.RootFolder)
        {
        }

        public TextFilePropertyStore(TextFilePropertyStoreOptions options, IMemoryCache cache, IDeadPropertyFactory deadPropertyFactory, string rootFolder)
            : base(deadPropertyFactory)
        {
            _cache = cache;
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

        public override async Task<IReadOnlyCollection<XElement>> GetAsync(IEntry entry, CancellationToken cancellationToken)
        {
            var storeData = Load(entry, false, cancellationToken);
            EntryInfo info;
            IReadOnlyCollection<XElement> result;
            if (storeData.Entries.TryGetValue(GetEntryKey(entry), out info))
            {
                result = info.Attributes.Values.ToList();
            }
            else
            {
                var etagXml = new EntityTag(false).ToXml();
                await SetAsync(entry, etagXml, cancellationToken).ConfigureAwait(false);
                result = new[] { etagXml };
            }

            return result;
        }

        public override Task SetAsync(IEntry entry, IEnumerable<XElement> elements, CancellationToken cancellationToken)
        {
            var info = GetInfo(entry, cancellationToken) ?? new EntryInfo();
            foreach (var element in elements)
            {
                info.Attributes[element.Name] = element;
            }

            UpdateInfo(entry, info, cancellationToken);
            return Task.FromResult(0);
        }

        public override Task<IReadOnlyCollection<bool>> RemoveAsync(IEntry entry, IEnumerable<XName> names, CancellationToken cancellationToken)
        {
            var info = GetInfo(entry, cancellationToken) ?? new EntryInfo();
            var result = new List<bool>();
            foreach (var name in names)
            {
                result.Add(info.Attributes.Remove(name));
            }

            UpdateInfo(entry, info, cancellationToken);
            return Task.FromResult<IReadOnlyCollection<bool>>(result);
        }

        public override Task RemoveAsync(IEntry entry, CancellationToken cancellationToken)
        {
            var fileName = GetFileNameFor(entry);
            if (!File.Exists(fileName))
                return Task.FromResult(0);
            return base.RemoveAsync(entry, cancellationToken);
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
            if (isCollection)
                return Path.Combine(path, _storeEntryName);

            var directoryName = Path.GetDirectoryName(path);
            Debug.Assert(directoryName != null, "directoryName != null");
            return Path.Combine(directoryName, _storeEntryName);
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
            return Path.Combine(RootPath, Path.Combine(names.ToArray()));
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
