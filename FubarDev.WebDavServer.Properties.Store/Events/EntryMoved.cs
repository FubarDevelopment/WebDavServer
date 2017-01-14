using System;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Properties.Store.Events
{
    public class EntryMoved
    {
        public string FromPath { get; }
        public IEntry NewEntry { get; }
    }
}
