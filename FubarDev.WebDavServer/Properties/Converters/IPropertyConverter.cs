// <copyright file="IPropertyConverter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Xml.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Properties.Converters
{
    public interface IPropertyConverter<T>
    {
        [NotNull]
        T FromElement([NotNull] XElement element);

        [NotNull]
        XElement ToElement(XName name, [NotNull] T value);
    }
}
