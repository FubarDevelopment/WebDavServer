// <copyright file="PropertyEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using SQLite;

namespace FubarDev.WebDavServer.Props.Store.SQLite
{
    /// <summary>
    /// An entity for properties.
    /// </summary>
    [Table("props")]
    internal class PropertyEntry
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        [PrimaryKey]
        [Column("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the serialized XML name.
        /// </summary>
        [Column("path")]
        [Unique(Name = "unq_idx", Order = 0)]
        [Indexed]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the serialized XML name.
        /// </summary>
        [Column("name")]
        [Unique(Name = "unq_idx", Order = 1)]
        public string XmlName { get; set; }

        /// <summary>
        /// Gets or sets the XML language identifier.
        /// </summary>
        [Column("lang")]
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the XML element.
        /// </summary>
        [Column("value")]
        public string Value { get; set; }
    }
}
