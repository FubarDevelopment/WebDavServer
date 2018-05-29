using System;
using System.Collections.Generic;

using FubarDev.WebDavServer.Model.Headers;

namespace FubarDev.WebDavServer.NHibernate.Models
{
    internal class FileEntry
    {
        public virtual Guid Id { get; set; }

        public virtual Guid ParentId { get; set; }

        public virtual string Name { get; set; }

        public virtual string InvariantName { get; set; }

        public virtual bool IsCollection { get; set; }

        public virtual DateTime LastWriteTimeUtc { get; set; }

        public virtual DateTime CreationTimeUtc { get; set; }

        public virtual long Length { get; set; }

        public virtual EntityTag ETag { get; set; }

        public virtual FileData Data { get; set; }

        public virtual IDictionary<string, PropertyEntry> Properties { get; set; }
    }
}
