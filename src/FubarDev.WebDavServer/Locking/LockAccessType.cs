// <copyright file="LockAccessType.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Xml.Linq;

using FubarDev.WebDavServer.Model;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Locking
{
    public struct LockAccessType : IEquatable<LockAccessType>
    {
        public static LockAccessType Write = new LockAccessType(WriteId);

        private const string WriteId = "write";

        public LockAccessType([NotNull] string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            Name = WebDavXml.Dav + id;
        }

        [NotNull]
        public XName Name { get; }

        public static bool operator ==(LockAccessType x, LockAccessType y)
        {
            return x.Name == y.Name;
        }

        public static bool operator !=(LockAccessType x, LockAccessType y)
        {
            return x.Name != y.Name;
        }

        public static LockAccessType Parse([NotNull] string accessType)
        {
            if (accessType == null)
                throw new ArgumentNullException(nameof(accessType));

            switch (accessType.ToLowerInvariant())
            {
                case WriteId:
                    return Write;
            }

            throw new ArgumentOutOfRangeException(nameof(accessType), $"The access type {accessType} is not supported.");
        }

        public bool Equals(LockAccessType other)
        {
            return Name.Equals(other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is LockAccessType && Equals((LockAccessType)obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
