// <copyright file="SQLitePropertyStoreOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.Props.Store.SQLite
{
    /// <summary>
    /// The options for the <see cref="SQLitePropertyStore"/>
    /// </summary>
    public class SQLitePropertyStoreOptions
    {
        /// <summary>
        /// Gets or sets the default estimated cost for querying the dead properties values
        /// </summary>
        public int EstimatedCost { get; set; } = 10;
    }
}
