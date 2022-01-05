// <copyright file="TokenType.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using Yoakke.Lexer;
using Yoakke.Lexer.Attributes;

namespace FubarDev.WebDavServer.Parsing;

/// <summary>
/// All tokens used by the <see cref="Lexer"/> and <see cref="Parser"/>.
/// </summary>
public enum TokenType
{
    /// <summary>
    /// The error token
    /// </summary>
    [Error]
    Error,

    /// <summary>
    /// Token that indicates the end of the stream
    /// </summary>
    [End]
    End,

    /// <summary>
    /// Whitespace characters to ignore
    /// </summary>
    [Ignore]
    [Regex(Regexes.Whitespace)]
    Whitespace,

    /// <summary>
    /// Weak ETag
    /// </summary>
    [Regex("[Ww]/" + Lexer.QuotedStringLiteral)]
    WeakEntityTag,

    /// <summary>
    /// Quoted string
    /// </summary>
    [Regex(Lexer.QuotedStringLiteral)]
    QuotedString,

    /// <summary>
    /// Comma character
    /// </summary>
    [Token(",")]
    Comma,

    /// <summary>
    /// Left parenthesis
    /// </summary>
    [Token("(")]
    LeftParen,

    /// <summary>
    /// Right parenthesis
    /// </summary>
    [Token(")")]
    RightParen,

    /// <summary>
    /// URL reference
    /// </summary>
    [Regex("<[^>]+>")]
    UrlReference,

    /// <summary>
    /// Left angle bracket
    /// </summary>
    [Token("<")]
    LeftAngle,

    /// <summary>
    /// Right angle bracket
    /// </summary>
    [Token(">")]
    RightAngle,

    /// <summary>
    /// Left square bracket
    /// </summary>
    [Token("[")]
    LeftBracket,

    /// <summary>
    /// Right square bracket
    /// </summary>
    [Token("]")]
    RightBracket,

    /// <summary>
    /// The <c>Not</c> keyword
    /// </summary>
    [Regex("[Nn][Oo][Tt]")]
    Not,
}
