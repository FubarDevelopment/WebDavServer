// <copyright file="ParseErrorExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Text;

using Yoakke.Lexer;
using Yoakke.Parser;
using Yoakke.Text;

namespace FubarDev.WebDavServer.Parsing;

/// <summary>
/// Extension methods for <see cref="ParseError"/>.
/// </summary>
public static class ParseErrorExtensions
{
    /// <summary>
    /// Gets the full error message.
    /// </summary>
    /// <param name="error">The parse error.</param>
    /// <returns>The error message.</returns>
    public static string GetFullMessage(
        this ParseError error)
    {
        var message = new StringBuilder()
            .Append($"Error");
        var errorPosition = error.GetPosition();
        if (errorPosition != null)
        {
            message.Append($" ({errorPosition})");
        }

        message
            .Append(": ")
            .Append(error.GetMessage());
        return message.ToString();
    }

    /// <summary>
    /// Gets the error message.
    /// </summary>
    /// <param name="error">The parse error.</param>
    /// <returns>The error message.</returns>
    public static string GetMessage(
        this ParseError error)
    {
        var expectedEntries =
            from expected in error.Elements.Values
            select $"{string.Join(", or ", expected.Expected)} for {expected.Context}";
        return $"Expected {string.Join(", or ", expectedEntries)}, but got {error.GetGotMessage()}";
    }

    /// <summary>
    /// Gets the position of the error.
    /// </summary>
    /// <param name="error">The parse error.</param>
    /// <returns>The position.</returns>
    public static string? GetPosition(
        this ParseError error) =>
        error.Position switch
        {
            null => null,
            Position position => $"{position.Line}:{position.Column}",
            _ => error.Position.ToString(),
        };

    private static string GetGotMessage(
        this ParseError error) =>
        error.Got switch
        {
            null => "(null)",
            IToken<TokenType> token => $"{token.Kind} '{token.Text}'",
            _ => error.Got.ToString() ?? "(null)",
        };
}
