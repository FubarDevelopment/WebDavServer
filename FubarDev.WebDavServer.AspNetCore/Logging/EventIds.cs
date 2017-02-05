// <copyright file="EventIds.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.AspNetCore.Logging
{
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "EventId is a struct.")]
    public static class EventIds
    {
        public static EventId Unspecified = new EventId(0);
    }
}
