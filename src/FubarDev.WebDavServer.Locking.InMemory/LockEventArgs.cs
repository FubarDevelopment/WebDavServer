// <copyright file="LockEventArgs.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.WebDavServer.Locking.InMemory
{
    public class LockEventArgs : EventArgs
    {
        public LockEventArgs(IActiveLock activeLock)
        {
            Lock = activeLock;
        }

        public IActiveLock Lock { get; }
    }
}
