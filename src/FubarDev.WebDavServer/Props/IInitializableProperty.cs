// <copyright file="IInitializableProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Xml.Linq;

namespace FubarDev.WebDavServer.Props
{
    /// <summary>
    /// A property that can be initialized with a <see cref="XElement"/>
    /// </summary>
    /// <remarks>
    /// This avoids the (propably costly) fetching of the properties value.
    /// </remarks>
    public interface IInitializableProperty : IProperty
    {
        /// <summary>
        /// Initialize the property with an <see cref="XElement"/>
        /// </summary>
        /// <param name="initialValue">The element to intialize the property with</param>
        void Init(XElement initialValue);
    }
}
