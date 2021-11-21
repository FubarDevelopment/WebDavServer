// <copyright file="Class2DeadPropertyFactory.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Props.Live;
using FubarDev.WebDavServer.Props.Store;

namespace FubarDev.WebDavServer.Props.Dead
{
    /// <summary>
    /// Implementation of <see cref="IDefaultDeadPropertyFactory"/> for WebDAV class 2.
    /// </summary>
    public class Class2DeadPropertyFactory : IDefaultDeadPropertyFactory
    {
        /// <inheritdoc />
        public bool TryCreateDeadProperty(IPropertyStore store, IEntry entry, XName name, [NotNullWhen(true)] out IDeadProperty? deadProperty)
        {
            deadProperty = null;
            return false;
        }

        /// <inheritdoc />
        public IEnumerable<IUntypedReadableProperty> GetProperties(IEntry entry)
        {
            yield return new LockDiscoveryProperty(entry);
            yield return new SupportedLockProperty(entry);
        }
    }
}
