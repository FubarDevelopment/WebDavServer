using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Properties.Dead;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

namespace FubarDev.WebDavServer.Properties.Store.TextFile
{
    public class TextFilePropertyStore : PropertyStoreBase, IFileSystemPropertyStore
    {
        private static readonly XElement[] _emptyElements = new XElement[0];

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
        }

        public override int Cost => _options.EstimatedCost;

        public string RootPath { get; set; }

        public bool IgnoreEntry(IEntry entry)
        {
            return entry is IDocument && entry.Name == _storeEntryName;
        }

        public override Task<IReadOnlyCollection<XElement>> GetAsync(IEntry entry, CancellationToken cancellationToken)
        {
            var storeData = Load(entry, false);
            EntryInfo info;
            IReadOnlyCollection<XElement> result;
            if (storeData.Entries.TryGetValue(GetEntryKey(entry), out info))
            {
                result = info.Attributes.Values.ToList();
            }
            else
            {
                result = _emptyElements;
            }

            return Task.FromResult(result);
        }

        public override Task SetAsync(IEntry entry, IEnumerable<XElement> elements, CancellationToken cancellationToken)
        {
            var info = GetInfo(entry) ?? new EntryInfo();
            foreach (var element in elements)
            {
                info.Attributes[element.Name] = element;
            }

            UpdateInfo(entry, info);
            return Task.FromResult(0);
        }

        public override Task<IReadOnlyCollection<bool>> RemoveAsync(IEntry entry, IEnumerable<XName> names, CancellationToken cancellationToken)
        {
            var info = GetInfo(entry) ?? new EntryInfo();
            var result = new List<bool>();
            foreach (var name in names)
            {
                result.Add(info.Attributes.Remove(name));
            }

            UpdateInfo(entry, info);
            return Task.FromResult<IReadOnlyCollection<bool>>(result);
        }

        private static string GetEntryKey(IEntry entry)
        {
            if (entry is ICollection)
                return ".";
            return entry.Name.ToLower();
        }

        private void UpdateInfo(IEntry entry, EntryInfo info)
        {
            var storeData = Load(entry, true);
            var entryKey = GetEntryKey(entry);
            storeData.Entries[entryKey] = info;
            Save(entry, storeData);
        }

        private EntryInfo GetInfo(IEntry entry)
        {
            var storeData = Load(entry, false);

            EntryInfo info;
            if (!storeData.Entries.TryGetValue(GetEntryKey(entry), out info))
                info = new EntryInfo();

            return info;
        }

        private void Save(IEntry entry, StoreData data)
        {
            Save(GetFileNameFor(entry), data);
        }

        private StoreData Load(IEntry entry, bool useCache)
        {
            return Load(GetFileNameFor(entry), useCache);
        }

        private void Save(string fileName, StoreData data)
        {
            try
            {
                var key = fileName.ToLower();
                File.WriteAllText(fileName, JsonConvert.SerializeObject(data));
                _cache.Set(key, data);
            }
            catch (Exception)
            {
                // Ignore all exceptions for directories that cannot be modified
            }
        }

        private StoreData Load(string fileName, bool useCache)
        {
            if (!File.Exists(fileName))
                return new StoreData();

            var key = fileName.ToLower();
            if (!useCache)
            {
                var result = JsonConvert.DeserializeObject<StoreData>(File.ReadAllText(fileName));
                _cache.Set(key, result);
                return result;
            }

            return _cache.GetOrCreate(key, ce => JsonConvert.DeserializeObject<StoreData>(File.ReadAllText(fileName)));
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
            do
            {
                if (!string.IsNullOrEmpty(entry.Name))
                    names.Add(entry.Name);
                entry = entry.Parent;
            } while (entry != null);
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
