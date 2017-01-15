using System;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Properties.Events
{
    public class EntryMoved
    {
        public string FromPath { get; }
        public IEntry NewEntry { get; }
    }
}
