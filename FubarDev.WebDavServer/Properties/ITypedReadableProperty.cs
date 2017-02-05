// <copyright file="ITypedReadableProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Properties
{
    public interface ITypedReadableProperty<T> : IUntypedReadableProperty
    {
        [NotNull]
        [ItemNotNull]
        Task<T> GetValueAsync(CancellationToken ct);
    }
}
