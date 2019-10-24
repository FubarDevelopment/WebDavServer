// <copyright file="prop.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

// ReSharper disable InconsistentNaming
namespace FubarDev.WebDavServer.Model
{
    /// <summary>
    /// The WebDAV prop element.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Name created by xsd tool.")]
    public partial class prop
    {
        /// <summary>
        /// Gets or sets the language code.
        /// </summary>
        [XmlAttribute("xml:lang", DataType = "language")]
        public string? Language { get; set; }
    }
}
