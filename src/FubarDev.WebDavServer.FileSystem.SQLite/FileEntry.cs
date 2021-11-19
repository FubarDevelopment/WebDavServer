// <copyright file="FileEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

#nullable disable warnings
#nullable enable annotations

using System;

using FubarDev.WebDavServer.Model.Headers;

using SQLite;

namespace FubarDev.WebDavServer.FileSystem.SQLite
{
    [Table("filesystementries")]
    internal class FileEntry
    {
        [PrimaryKey]
        [Column("id")]
        public string Id { get; set; } = default!;

        [Indexed]
        [Column("path")]
        public string Path { get; set; } = default!;

        [Indexed("name_type", 0)]
        [Column("name")]
        public string Name { get; set; } = default!;

        [Indexed("name_type", 1)]
        [Column("collection")]
        public bool IsCollection { get; set; }

        [Column("mtime")]
        public DateTime LastWriteTimeUtc { get; set; } = DateTime.UtcNow;

        [Column("ctime")]
        public DateTime CreationTimeUtc { get; set; } = DateTime.UtcNow;

        [Column("length")]
        public long Length { get; set; }

        [Column("etag")]
        public string ETag { get; set; } = new EntityTag(false).ToString();
    }
}
