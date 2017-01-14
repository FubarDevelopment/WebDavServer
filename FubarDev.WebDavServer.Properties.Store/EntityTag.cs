using System;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using FubarDev.WebDavServer.Model;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Properties.Store
{
    public class EntityTag
    {
        public static readonly XName PropertyName = WebDavXml.Dav + "getetag";

        public bool IsWeak { get; set; }
        public string Value { get; set; }

        public static EntityTag FromXml([CanBeNull] XElement element)
        {
            if (element == null)
                return new EntityTag() { Value = Guid.NewGuid().ToString("D") };

            var isWeak = element.Attributes("is-weak").Select(x => XmlConvert.ToBoolean(x.Value)).FirstOrDefault();
            return new EntityTag()
            {
                IsWeak = isWeak,
                Value = element.Value
            };
        }

        public EntityTag Update()
        {
            return new EntityTag()
            {
                IsWeak = this.IsWeak,
                Value = Guid.NewGuid().ToString("D")
            };
        }

        public XElement ToXml()
        {
            return new XElement(
                PropertyName,
                new XAttribute("is-weak", XmlConvert.ToString(IsWeak)),
                Value);
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            if (IsWeak)
                result.Append("W/");
            result.Append('"').Append(Value.Replace("\"", "\"\"")).Append('"');
            return result.ToString();
        }
    }
}
