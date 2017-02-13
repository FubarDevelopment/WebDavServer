// <copyright file="EntityTagConverter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Xml.Linq;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Props.Converters
{
    public class EntityTagConverter : IPropertyConverter<EntityTag>
    {
        public EntityTag FromElement(XElement element)
        {
            return EntityTag.FromXml(element);
        }

        public XElement ToElement(XName name, EntityTag value)
        {
            return value.ToXml();
        }
    }
}
