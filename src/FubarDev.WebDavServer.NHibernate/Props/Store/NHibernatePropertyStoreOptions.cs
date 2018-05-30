// <copyright file="NHibernatePropertyStoreOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.NHibernate.Props.Store
{
    /// <summary>
    /// The options for the <see cref="NHibernatePropertyStore"/>
    /// </summary>
    public class NHibernatePropertyStoreOptions
    {
        /// <summary>
        /// Gets or sets the default estimated cost for querying the dead properties values
        /// </summary>
        public int EstimatedCost { get; set; } = 20;
    }
}
