// <copyright file="SimpleTypedProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props
{
    public abstract class SimpleTypedProperty<T> : SimpleUntypedProperty, ITypedWriteableProperty<T>
    {
        protected SimpleTypedProperty([NotNull] XName name, int cost, params XName[] alternativeNames)
            : base(name, cost, alternativeNames)
        {
        }

        public abstract Task<T> GetValueAsync(CancellationToken ct);

        public abstract Task SetValueAsync(T value, CancellationToken ct);
    }
}