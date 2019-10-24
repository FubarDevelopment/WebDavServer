// <copyright file="FileData.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

#nullable disable warnings
#nullable enable annotations

using SQLite;

namespace FubarDev.WebDavServer.FileSystem.SQLite
{
    [Table("filesystementrydata")]
    internal class FileData
    {
        [PrimaryKey]
        [Column("id")]
        public string Id { get; set; }

        [Column("data")]
        public byte[] Data { get; set; }
    }
}
