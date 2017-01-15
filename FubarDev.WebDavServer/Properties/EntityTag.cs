using System;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using FubarDev.WebDavServer.Model;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Properties
{
    public class EntityTag
    {
        public static readonly XName PropertyName = WebDavXml.Dav + "getetag";

        public EntityTag()
            : this(false, Guid.NewGuid().ToString("D"))
        {
        }

        internal EntityTag(bool isWeak, string value)
        {
            IsWeak = isWeak;
            Value = value;
        }

        public bool IsWeak { get; }
        public string Value { get; }

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
            return new XElement(
                GetETagProperty.PropertyName,
                new XAttribute("is-weak", XmlConvert.ToString(IsWeak)),
                Value);
        }

        public static EntityTag FromXml([CanBeNull] XElement element)
        {
            if (element == null)
                return new EntityTag();

            var isWeak = element.Attributes("is-weak").Select(x => XmlConvert.ToBoolean(x.Value)).FirstOrDefault();
            return new EntityTag(isWeak, element.Value);
        }
    }
}
