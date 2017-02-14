// <copyright file="ITimeoutPolicy.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

namespace FubarDev.WebDavServer.Locking
{
    public interface ITimeoutPolicy
    {
        TimeSpan SelectTimeout(IReadOnlyCollection<TimeSpan> timeouts);
    }
}
