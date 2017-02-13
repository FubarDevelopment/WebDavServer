// <copyright file="ITypedWriteableProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props
{
    public interface ITypedWriteableProperty<T> : ITypedReadableProperty<T>, IUntypedWriteableProperty
    {
        [NotNull]
        Task SetValueAsync([NotNull] T value, CancellationToken ct);
    }
}
