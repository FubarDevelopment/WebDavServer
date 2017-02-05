// <copyright file="EntityTag.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Text;
using System.Xml.Linq;

using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Properties.Dead;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Properties
{
    public class EntityTag
    {
        public static readonly XName PropertyName = WebDavXml.Dav + "getetag";

        public EntityTag()
            : this(false)
        {
        }

        public EntityTag(bool isWeak)
            : this(isWeak, Guid.NewGuid().ToString("D"))
        {
        }

        internal EntityTag(bool isWeak, string value)
        {
            IsWeak = isWeak;
            Value = value;
        }

        public bool IsWeak { get; }

        public string Value { get; }

        public static EntityTag FromXml([CanBeNull] XElement element)
        {
            if (element == null)
                return new EntityTag();

            return Parse(element.Value);
        }

        public static EntityTag Parse(string value)
        {
            var isWeak = value.StartsWith("W/");
            if (isWeak)
                value = value.Substring(2);
            if (value.StartsWith("\"") && value.EndsWith("\""))
                value = value.Substring(1, value.Length - 2);
            return new EntityTag(isWeak, value);
        }

        public EntityTag AsStrong()
        {
            return new EntityTag(false, Value);
        }

        public EntityTag AsWeak()
        {
            return new EntityTag(true, Value);
        }

        public EntityTag Update()
        {
            return new EntityTag(IsWeak, Guid.NewGuid().ToString("D"));
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            if (IsWeak)
                result.Append("W/");
            result.Append('"').Append(Value.Replace("\"", "\"\"")).Append('"');
            return result.ToString();
        }

        public XElement ToXml()
        {
            return new XElement(GetETagProperty.PropertyName, ToString());
        }
    }
}
