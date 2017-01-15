using System;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Properties.Events
{
    public class EntryMoved
    {
        public EntryMoved(string fromPath, IEntry newEntry)
        {
            FromPath = fromPath;
            NewEntry = newEntry;
        }

        public string FromPath { get; }
        public IEntry NewEntry { get; }
    }
}
