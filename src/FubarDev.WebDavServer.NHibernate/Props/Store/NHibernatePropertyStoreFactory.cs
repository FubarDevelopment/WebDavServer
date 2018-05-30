// <copyright file="NHibernatePropertyStoreFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Diagnostics;
using System.IO;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.NHibernate.FileSystem;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Store;
using FubarDev.WebDavServer.Utils;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NHibernate;

namespace FubarDev.WebDavServer.NHibernate.Props.Store
{
    /// <summary>
    /// The factory for the <see cref="NHibernatePropertyStore"/>
    /// </summary>
    public class NHibernatePropertyStoreFactory : IPropertyStoreFactory
    {
        [NotNull]
        private readonly ISessionFactory _sessionFactory;

        [NotNull]
        private readonly ILogger<NHibernatePropertyStore> _logger;

        [NotNull]
        private readonly IDeadPropertyFactory _deadPropertyFactory;

        [CanBeNull]
        private readonly IOptions<NHibernatePropertyStoreOptions> _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="NHibernatePropertyStoreFactory"/> class.
        /// </summary>
        /// <param name="sessionFactory">The NHibernate session factory</param>
        /// <param name="logger">The logger for the property store factory</param>
        /// <param name="deadPropertyFactory">The factory for dead properties</param>
        /// <param name="options">The options for this property store</param>
        public NHibernatePropertyStoreFactory(
            [NotNull] ISessionFactory sessionFactory,
            [NotNull] ILogger<NHibernatePropertyStore> logger,
            [NotNull] IDeadPropertyFactory deadPropertyFactory,
            [CanBeNull] IOptions<NHibernatePropertyStoreOptions> options = null)
        {
            _sessionFactory = sessionFactory;
            _logger = logger;
            _deadPropertyFactory = deadPropertyFactory;
            _options = options;
        }

        /// <inheritdoc />
        public IPropertyStore Create(IFileSystem fileSystem)
        {
            var fs = (NHibernateFileSystem)fileSystem;
            var session = fs.Connection;
            return new NHibernatePropertyStore(_deadPropertyFactory, session, _options, _logger);
        }
    }
}
