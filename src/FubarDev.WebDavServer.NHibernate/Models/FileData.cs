using System;

namespace FubarDev.WebDavServer.NHibernate.Models
{
    public class FileData
    {
        public virtual Guid Id { get; set; }

        public virtual byte[] Data { get; set; }

        public virtual FileEntry Entry { get; set; }
    }
}
