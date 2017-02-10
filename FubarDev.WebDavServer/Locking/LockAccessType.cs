// <copyright file="LockAccessType.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Locking
{
    public struct LockAccessType
    {
        public static LockAccessType Write = new LockAccessType(WriteId);

        private const string WriteId = "write";

        public LockAccessType(string id)
        {
            Id = id;
        }

        public string Id { get; }

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
    }
}
