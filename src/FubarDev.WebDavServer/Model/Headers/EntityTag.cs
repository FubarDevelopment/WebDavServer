// <copyright file="EntityTag.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Utils;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model.Headers
{
    public struct EntityTag
    {
        public static readonly XName PropertyName = WebDavXml.Dav + "getetag";

        public EntityTag(bool isWeak)
            : this(isWeak, Guid.NewGuid().ToString("D"))
        {
        }

        internal EntityTag(bool isWeak, [NotNull] string value)
        {
            IsWeak = isWeak;
            Value = value;
        }

        public bool IsWeak { get; }

        [NotNull]
        public string Value { get; }

        public bool IsEmpty => string.IsNullOrEmpty(Value);

        public static bool operator ==(EntityTag x, EntityTag y)
        {
            return EntityTagComparer.Default.Equals(x, y);
        }

        public static bool operator !=(EntityTag x, EntityTag y)
        {
            return !EntityTagComparer.Default.Equals(x, y);
        }

        public static EntityTag FromXml([CanBeNull] XElement element)
        {
            if (element == null)
                return new EntityTag(false);

            return Parse(element.Value).Single();
        }

        public static IEnumerable<EntityTag> Parse([NotNull] string s)
        {
            var source = new StringSource(s);
            var result = Parse(source).ToList();
            if (!source.Empty)
                throw new ArgumentException($"{source.Remaining} is not a valid ETag", nameof(source));
            return result;
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

        [NotNull]
        public XElement ToXml()
        {
            return new XElement(GetETagProperty.PropertyName, ToString());
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            if (IsWeak)
                result.Append("W/");
            result.Append('"').Append(Value.Replace("\"", "\"\"")).Append('"');
            return result.ToString();
        }

        public bool Equals(EntityTag other)
        {
            return EntityTagComparer.Default.Equals(this, other);
        }

        public override bool Equals(object obj)
        {
            return obj is EntityTag && Equals((EntityTag)obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode()
                ^ IsWeak.GetHashCode();
        }

        [NotNull]
        internal static IEnumerable<EntityTag> Parse([NotNull] StringSource source)
        {
            while (!source.SkipWhiteSpace())
            {
                bool isWeak;
                if (source.AdvanceIf("W/\"", StringComparison.OrdinalIgnoreCase))
                {
                    isWeak = true;
                }
                else if (!source.AdvanceIf("\""))
                {
                    break;
                }
                else
                {
                    isWeak = false;
                }

                var etagText = source.GetUntil('"');
                if (etagText == null)
                    throw new ArgumentException($"{source.Remaining} is not a valid ETag", nameof(source));

                yield return new EntityTag(isWeak, etagText);

                if (source.Advance(1).SkipWhiteSpace())
                    break;

                if (!source.AdvanceIf(","))
                    break;
            }
        }
    }
}
