// <copyright file="LockAccessType.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;

namespace FubarDev.WebDavServer.Locking
{
    public struct LockAccessType
    {
        public static LockAccessType Write = new LockAccessType("write");

        public LockAccessType(string id)
        {
            Id = id;
        }

        public string Id { get; }
    }
}
