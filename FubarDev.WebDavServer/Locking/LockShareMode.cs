// <copyright file="LockShareMode.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.Locking
{
    public struct LockShareMode
    {
        public static readonly LockShareMode Shared = new LockShareMode("shared");
        public static readonly LockShareMode Exclusive = new LockShareMode("exclusive");

        public LockShareMode(string id)
        {
            Id = id;
        }

        public string Id { get; }
    }
}
