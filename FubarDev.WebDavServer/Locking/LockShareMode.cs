// <copyright file="LockShareMode.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Locking
{
    public struct LockShareMode
    {
        public static readonly LockShareMode Shared = new LockShareMode(SharedId);
        public static readonly LockShareMode Exclusive = new LockShareMode(ExclusiveId);

        private const string SharedId = "shared";
        private const string ExclusiveId = "exclusive";

        public LockShareMode(string id)
        {
            Id = id;
        }

        public string Id { get; }

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
    }
}
