// <copyright file="PropertyEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.WebDavServer.NHibernate.Models
{
    /// <summary>
    /// An entity for properties
    /// </summary>
    public class PropertyEntry
    {
        /// <summary>
        /// Gets or sets the unique identifier
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the file entry
        /// </summary>
        public virtual FileEntry Entry { get; set; }

        /// <summary>
        /// Gets or sets the serialized XML name
        /// </summary>
        public virtual string XmlName { get; set; }

        /// <summary>
        /// Gets or sets the XML language identifier
        /// </summary>
        public virtual string Language { get; set; }

        /// <summary>
        /// Gets or sets the XML element
        /// </summary>
        public virtual string Value { get; set; }
    }
}
