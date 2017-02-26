// <copyright file="IDeadProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.Props.Dead
{
    /// <summary>
    /// The interface for a dead (read-only) property
    /// </summary>
    public interface IDeadProperty : IUntypedReadableProperty, IInitializableProperty
    {
    }
}
