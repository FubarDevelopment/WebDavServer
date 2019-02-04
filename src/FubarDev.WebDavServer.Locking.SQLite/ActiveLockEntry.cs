// <copyright file="ActiveLockEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Xml.Linq;

using SQLite;

namespace FubarDev.WebDavServer.Locking.SQLite
{
    [Table("locks")]
    internal class ActiveLockEntry : IActiveLock
    {
        [PrimaryKey]
        [Column("id")]
        [MaxLength(100)]
        [NotNull]
        public string StateToken { get; set; } = string.Empty;

        [Column("path")]
        [NotNull]
        public string Path { get; set; } = string.Empty;

        [Column("href")]
        [NotNull]
        public string Href { get; set; } = string.Empty;

        [Column("recursive")]
        public bool Recursive { get; set; }

        [Column("access_type")]
        [NotNull]
        public string AccessType { get; set; } = string.Empty;

        [Column("share_mode")]
        [NotNull]
        public string ShareMode { get; set; } = string.Empty;

        [Column("timeout")]
        public TimeSpan Timeout { get; set; }

        [Column("issued")]
        public DateTime Issued { get; set; }

        [Column("last_refresh")]
        public DateTime? LastRefresh { get; set; }

        [Column("expiration")]
        public DateTime Expiration { get; set; }

        [Column("owner")]
        public string Owner { get; set; }

        public XElement GetOwner()
        {
            if (Owner == null)
            {
                return null;
            }

            return XElement.Parse(Owner);
        }
    }
}
