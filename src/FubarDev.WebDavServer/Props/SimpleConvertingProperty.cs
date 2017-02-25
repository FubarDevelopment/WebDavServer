// <copyright file="SimpleConvertingProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Props.Converters;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props
{
    /// <summary>
    /// Simple converting property
    /// </summary>
    /// <typeparam name="T">The type to be converted from or to</typeparam>
    public abstract class SimpleConvertingProperty<T> : SimpleTypedProperty<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleConvertingProperty{T}"/> class.
        /// </summary>
        /// <param name="name">The property name</param>
        /// <param name="cost">The cost to get the properties value</param>
        /// <param name="converter">The converter to copy the value to/from an <see cref="XElement"/></param>
        /// <param name="alternativeNames">The alternative names</param>
        protected SimpleConvertingProperty([NotNull] XName name, int cost, [NotNull] IPropertyConverter<T> converter, params XName[] alternativeNames)
            : base(name, cost, alternativeNames)
        {
            Converter = converter;
        }

        /// <summary>
        /// Gets the converter to be used to copy to/from an <see cref="XElement"/>
        /// </summary>
        [NotNull]
        protected IPropertyConverter<T> Converter { get; }

        /// <inheritdoc />
        public override async Task<XElement> GetXmlValueAsync(CancellationToken ct)
        {
            var result = await GetValueAsync(ct).ConfigureAwait(false);
            return Converter.ToElement(Name, result);
        }

        /// <inheritdoc />
        public override Task SetXmlValueAsync(XElement element, CancellationToken ct)
        {
            return SetValueAsync(Converter.FromElement(element), ct);
        }
    }
}
