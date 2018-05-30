// <copyright file="ActiveLockEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Xml.Linq;

using FubarDev.WebDavServer.Locking;

namespace FubarDev.WebDavServer.NHibernate.Models
{
    public class ActiveLockEntry : IActiveLock
    {
        public virtual string StateToken { get; set; } = string.Empty;

        public virtual string Path { get; set; } = string.Empty;

        public virtual string Href { get; set; } = string.Empty;

        public virtual bool Recursive { get; set; }

        public virtual string AccessType { get; set; } = string.Empty;

        public virtual string ShareMode { get; set; } = string.Empty;

        public virtual TimeSpan Timeout { get; set; }

        public virtual DateTime Issued { get; set; }

        public virtual DateTime? LastRefresh { get; set; }

        public virtual DateTime Expiration { get; set; }

        public virtual string Owner { get; set; }

        public virtual XElement GetOwner()
        {
            if (Owner == null)
                return null;
            return XElement.Parse(Owner);
        }
    }
}
