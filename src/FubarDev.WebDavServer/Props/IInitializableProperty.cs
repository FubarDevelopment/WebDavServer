// <copyright file="IInitializableProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Xml.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props
{
    /// <summary>
    /// A property that can be initialized with a <see cref="XElement"/>
    /// </summary>
    /// <remarks>
    /// This avoids the (probably costly) fetching of the properties value.
    /// </remarks>
    public interface IInitializableProperty : IProperty
    {
        /// <summary>
        /// Initialize the property with an <see cref="XElement"/>.
        /// </summary>
        /// <param name="initialValue">The element to initialize the property with.</param>
        void Init([NotNull] XElement initialValue);
    }
}
