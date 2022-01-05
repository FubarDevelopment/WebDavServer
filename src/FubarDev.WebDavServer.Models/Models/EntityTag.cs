// <copyright file="EntityTag.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Text;
using System.Xml.Linq;

using FubarDev.WebDavServer.Parsing;

namespace FubarDev.WebDavServer.Models;

/// <summary>
/// Structure for the HTTP entity tag.
/// </summary>
public readonly struct EntityTag : IEquatable<EntityTag>
{
    /// <summary>
    /// The default property name for the <c>getetag</c> WebDAV property.
    /// </summary>
    public static readonly XName PropertyName = WebDavXml.Dav + "getetag";

    private readonly string? _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityTag"/> struct.
    /// </summary>
    /// <param name="isWeak">Indicates whether the entity tag is weak.</param>
    public EntityTag(bool isWeak)
        : this(isWeak, Guid.NewGuid().ToString("D"))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityTag"/> struct.
    /// </summary>
    /// <param name="isWeak">Indicates whether the entity tag is weak.</param>
    /// <param name="value">The ETag value.</param>
    public EntityTag(bool isWeak, string? value)
    {
        IsWeak = isWeak;
        _value = value;
    }

    /// <summary>
    /// Gets an empty entity tag.
    /// </summary>
    public static EntityTag Empty { get; } = new(false, null);

    /// <summary>
    /// Gets a value indicating whether the entity tag is weak.
    /// </summary>
    public bool IsWeak { get; }

    /// <summary>
    /// Gets the entity tag.
    /// </summary>
    public string Value => _value ?? string.Empty;

    /// <summary>
    /// Gets a value indicating whether the entity tag structure is empty.
    /// </summary>
    public bool IsEmpty => _value == null;

    /// <summary>
    /// Compares two entity tags for their equality.
    /// </summary>
    /// <param name="x">The first entity tag to compare.</param>
    /// <param name="y">The second entity tag to compare.</param>
    /// <returns><see langword="true"/> when both entity tags are of equal value.</returns>
    public static bool operator ==(EntityTag x, EntityTag y)
    {
        return EntityTagComparer.Strong.Equals(x, y);
    }

    /// <summary>
    /// Compares two entity tags for their inequality.
    /// </summary>
    /// <param name="x">The first entity tag to compare.</param>
    /// <param name="y">The second entity tag to compare.</param>
    /// <returns><see langword="true"/> when both entity tags are not of equal value.</returns>
    public static bool operator !=(EntityTag x, EntityTag y)
    {
        return !EntityTagComparer.Strong.Equals(x, y);
    }

    /// <summary>
    /// Extracts an entity tag from the XML of a <c>getetag</c>.
    /// </summary>
    /// <param name="element">The XML of a <c>getetag</c>.</param>
    /// <returns>The found entity tag.</returns>
    /// <remarks>
    /// Returns a new strong entity tag when <paramref name="element"/> is <see langword="null"/>.
    /// </remarks>
    public static EntityTag FromXml(XElement? element)
    {
        if (element == null)
        {
            return new EntityTag(false);
        }

        return Parse(element.Value).Single();
    }

    /// <summary>
    /// Parses the entity tags as passed in the HTTP header.
    /// </summary>
    /// <param name="s">The textual entity tag representation.</param>
    /// <returns>The found entity tags.</returns>
    public static IEnumerable<EntityTag> Parse(string s)
    {
        var lexer = new Lexer(s);
        var parser = new Parser(lexer);
        var parseResult = parser.ParseEntityTagList();
        if (!parseResult.IsOk)
        {
            throw new ArgumentException($@"{s} is not a valid ETag", nameof(s));
        }

        if (!lexer.IsEnd)
        {
            var endToken = lexer.Next();
            if (endToken.Kind != TokenType.End)
            {
                throw new ArgumentException($@"{s} is not a valid ETag", nameof(s));
            }
        }

        return parseResult.Ok.Value;
    }

    /// <summary>
    /// Returns this entity tag as a new strong entity tag.
    /// </summary>
    /// <returns>The new strong entity tag.</returns>
    public EntityTag AsStrong()
    {
        if (!IsWeak)
        {
            return this;
        }

        return new EntityTag(false, _value);
    }

    /// <summary>
    /// Returns this entity tag as a new weak entity tag.
    /// </summary>
    /// <returns>The new weak entity tag.</returns>
    public EntityTag AsWeak()
    {
        if (IsWeak)
        {
            return this;
        }

        return new EntityTag(true, _value);
    }

    /// <summary>
    /// Returns an updated entity tag with a new <see cref="Value"/>.
    /// </summary>
    /// <returns>The updated entity tag.</returns>
    public EntityTag Update()
    {
        return new EntityTag(IsWeak, Guid.NewGuid().ToString("D"));
    }

    /// <summary>
    /// Creates a <c>getetag</c> XML from this entity tag.
    /// </summary>
    /// <returns>The new <see cref="XElement"/>.</returns>
    public XElement ToXml()
    {
        return new XElement(PropertyName, ToString());
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var result = new StringBuilder();
        if (IsWeak)
        {
            result.Append("W/");
        }

        result.Append('"')
            .Append(Value.Replace(@"\", @"\\").Replace("\"", "\\\""))
            .Append('"');
        return result.ToString();
    }

    /// <inheritdoc />
    public bool Equals(EntityTag other)
    {
        return EntityTagComparer.Strong.Equals(this, other);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is EntityTag tag && Equals(tag);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Value.GetHashCode()
               ^ IsWeak.GetHashCode();
    }
}
