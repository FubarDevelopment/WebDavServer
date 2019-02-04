// <copyright file="IPropertyConverter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Xml.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props.Converters
{
    /// <summary>
    /// The property converter interface
    /// </summary>
    /// <typeparam name="T">The type to convert to/from an <see cref="XElement"/></typeparam>
    public interface IPropertyConverter<T>
    {
        /// <summary>
        /// Determines whether the value is valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <returns><see langword="true"/> when the <paramref name="value"/> is valid.</returns>
        bool IsValidValue([CanBeNull] T value);

        /// <summary>
        /// Convert to the type <typeparamref name="T"/> from a given <paramref name="element"/>.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> to extract the value from.</param>
        /// <returns>The value extracted from the <paramref name="element"/>.</returns>
        [NotNull]
        T FromElement([NotNull] XElement element);

        /// <summary>
        /// Covert from a given <paramref name="value"/> to an <see cref="XElement"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="XElement"/> to be created.</param>
        /// <param name="value">The value to be converted to an <see cref="XElement"/>.</param>
        /// <returns>The created <see cref="XElement"/>.</returns>
        [NotNull]
        XElement ToElement(XName name, [NotNull] T value);
    }
}
