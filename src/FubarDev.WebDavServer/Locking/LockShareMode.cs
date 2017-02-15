// <copyright file="LockShareMode.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Xml.Linq;

using FubarDev.WebDavServer.Model;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Locking
{
    public struct LockShareMode : IEquatable<LockShareMode>
    {
        public static readonly LockShareMode Shared = new LockShareMode(SharedId, new lockscope()
        {
            ItemElementName = ItemChoiceType.shared,
            Item = new object(),
        });

        public static readonly LockShareMode Exclusive = new LockShareMode(ExclusiveId, new lockscope()
        {
            ItemElementName = ItemChoiceType.exclusive,
            Item = new object(),
        });

        private const string SharedId = "shared";
        private const string ExclusiveId = "exclusive";

        public LockShareMode([NotNull] string id, [NotNull] lockscope xmlValue)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            Name = WebDavXml.Dav + id;
            XmlValue = xmlValue;
        }

        [NotNull]
        public XName Name { get; }

        [NotNull]
        public lockscope XmlValue { get; }

        public static bool operator ==(LockShareMode x, LockShareMode y)
        {
            return x.Name == y.Name;
        }

        public static bool operator !=(LockShareMode x, LockShareMode y)
        {
            return x.Name != y.Name;
        }

        public static LockShareMode Parse([NotNull] string shareMode)
        {
            if (shareMode == null)
                throw new ArgumentNullException(nameof(shareMode));

            switch (shareMode.ToLowerInvariant())
            {
                case SharedId:
                    return Shared;
                case ExclusiveId:
                    return Exclusive;
            }

            throw new ArgumentOutOfRangeException(nameof(shareMode), $"The share mode {shareMode} is not supported.");
        }

        public bool Equals(LockShareMode other)
        {
            return Name.Equals(other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is LockShareMode && Equals((LockShareMode)obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
