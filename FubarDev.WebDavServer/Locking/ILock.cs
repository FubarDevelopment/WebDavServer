// <copyright file="ILock.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Xml.Linq;

namespace FubarDev.WebDavServer.Locking
{
    public interface ILock
    {
        string RootUrl { get; }

        bool Recursive { get; }

        string AccessType { get; }

        string ShareMode { get; }

        TimeSpan Timeout { get; }

        XElement GetOwner();
    }
}
