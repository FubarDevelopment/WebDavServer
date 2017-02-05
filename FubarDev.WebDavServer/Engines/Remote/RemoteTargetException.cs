// <copyright file="RemoteTargetException.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines.Remote
{
    public class RemoteTargetException : Exception
    {
        public RemoteTargetException()
        {
            Href = new Uri[0];
        }

        public RemoteTargetException(string message)
            : base(message)
        {
            Href = new Uri[0];
        }

        public RemoteTargetException(string message, Exception innerException)
            : base(message, innerException)
        {
            Href = new Uri[0];
        }

        public RemoteTargetException(IReadOnlyCollection<Uri> href)
        {
            Href = href;
        }

        public RemoteTargetException(params Uri[] href)
        {
            Href = href;
        }

        public RemoteTargetException(string message, IReadOnlyCollection<Uri> href)
            : base(message)
        {
            Href = href;
        }

        public RemoteTargetException(string message, params Uri[] href)
            : base(message)
        {
            Href = href;
        }

        public RemoteTargetException(string message, IReadOnlyCollection<Uri> href, Exception innerException)
            : base(message, innerException)
        {
            Href = href;
        }

        [NotNull]
        [ItemNotNull]
        public IReadOnlyCollection<Uri> Href { get; }
    }
}
