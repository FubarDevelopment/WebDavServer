// <copyright file="Lexer.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using Yoakke.Lexer.Attributes;

namespace FubarDev.WebDavServer.Parsing;

/// <summary>
/// The lexer for WebDAV structures.
/// </summary>
[Lexer(typeof(TokenType))]
public partial class Lexer
{
    /// <summary>
    /// Regular expression for quoted strings that use <c>&quot;&quot;</c> to escape a single <c>&quot;</c>.
    /// </summary>
    public const string QuotedStringLiteral = @"""((\\[^\n\r])|[^\r\n\\""]|(""""))*""";
}
