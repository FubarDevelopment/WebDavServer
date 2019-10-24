// <copyright file="SimpleTypedProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FubarDev.WebDavServer.Props
{
    /// <summary>
    /// A simple typed writeable property.
    /// </summary>
    /// <typeparam name="T">The type to be converted from or to.</typeparam>
    public abstract class SimpleTypedProperty<T> : SimpleUntypedProperty, ITypedWriteableProperty<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleTypedProperty{T}"/> class.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="language">The language for the property value.</param>
        /// <param name="cost">The cost to get the properties value.</param>
        /// <param name="alternativeNames">The alternative names.</param>
        protected SimpleTypedProperty(XName name, string? language, int cost, params XName[] alternativeNames)
            : base(name, language, cost, alternativeNames)
        {
        }

        /// <inheritdoc />
        public abstract Task<T> GetValueAsync(CancellationToken ct);

        /// <inheritdoc />
        public abstract Task SetValueAsync(T value, CancellationToken ct);
    }
}
