// <copyright file="Parser.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

using FubarDev.WebDavServer.Models;

using Yoakke.Lexer;
using Yoakke.Parser.Attributes;

namespace FubarDev.WebDavServer.Parsing;

/// <summary>
/// The parser for WebDAV structures.
/// </summary>
[Parser(typeof(TokenType))]
public partial class Parser
{
    private static readonly Regex _escapeRegex = new Regex(@"(\\.)|("""")", RegexOptions.Compiled);

    [Rule("resource_tag: UrlReference")]
    private static Uri MakeResourceTag(
        IToken<TokenType> urlReference) =>
        new(urlReference.Text[1..^1], UriKind.RelativeOrAbsolute);

    [Rule("coded_url: UrlReference")]
    private static Uri MakeCodedUrl(
        IToken<TokenType> urlReference) =>
        new(urlReference.Text[1..^1], UriKind.Absolute);

    [Rule("state_token: coded_url")]
    private static Uri MakeStateToken(
        Uri codedUrl) =>
        codedUrl;

    [Rule("quoted_string: QuotedString")]
    private static string MakeQuotedString(
        IToken<TokenType> quotedString) =>
        Unescape(quotedString.Text[1..^1]);

    [Rule("entity_tag: QuotedString | WeakEntityTag")]
    private static EntityTag MakeEntityTag(
        IToken<TokenType> entityTag) =>
        entityTag switch
        {
            { Kind: TokenType.QuotedString } => new EntityTag(false, Unescape(entityTag.Text[1..^1])),
            { Kind: TokenType.WeakEntityTag } => new EntityTag(true, Unescape(entityTag.Text[3..^1])),
            _ => throw new InvalidOperationException($"Unexpected token {entityTag.Kind}"),
        };

    [Rule("entity_tag_list: entity_tag (Comma entity_tag)*")]
    private static IReadOnlyList<EntityTag> MakeEntityTagList(
        EntityTag entityTag,
        IReadOnlyCollection<(IToken<TokenType> Comma, EntityTag EntityTag)>? entityTags)
    {
        var result = new List<EntityTag>()
        {
            entityTag,
        };

        if (entityTags != null && entityTags.Count != 0)
        {
            result.AddRange(entityTags.Select(x => x.EntityTag));
        }

        return result.AsReadOnly();
    }

    [Rule("condition: Not? (state_token | (LeftBracket entity_tag RightBracket))")]
    private static IfCondition MakeIfCondition(
        IToken<TokenType>? not,
        Uri stateToken)
    {
        return new IfCondition(not != null, stateToken, null);
    }

    private static IfCondition MakeIfCondition(
        IToken<TokenType>? not,
        (IToken<TokenType> LeftBracket, EntityTag EntityTag, IToken<TokenType> RightBracket) entityTag)
    {
        return new IfCondition(not != null, null, entityTag.EntityTag);
    }

    [Rule("condition_list: LeftParen condition+ RightParen")]
    [SuppressMessage("ReSharper", "UnusedParameter.Local", Justification = "Required for Yoakke")]
    private static IfList MakeIfConditionList(
        IToken<TokenType> lparen,
        IReadOnlyList<IfCondition> conditions,
        IToken<TokenType> rparen)
    {
        return new IfList(conditions);
    }

    [Rule("no_tag_list: condition_list")]
    private static IfNoTagList MakeNoTagList(
        IfList conditions)
    {
        return new IfNoTagList(conditions);
    }

    [Rule("tagged_list: resource_tag condition_list+")]
    private static IfTaggedList MakeTaggedList(
        Uri resourceTag,
        IReadOnlyList<IfList> conditions)
    {
        return new IfTaggedList(
            resourceTag,
            conditions);
    }

    [Rule("if_header: (no_tag_list+ | tagged_list+)")]
    private static IfHeader MakeIfHeader(
        IReadOnlyList<IfNoTagList> noTagLists)
    {
        return new IfHeader(noTagLists);
    }

    private static IfHeader MakeIfHeader(
        IReadOnlyList<IfTaggedList> taggedLists)
    {
        return new IfHeader(taggedLists);
    }

    private static string Unescape(string s) =>
        _escapeRegex.Replace(
            s,
            match =>
            {
                return match.Value[0] switch
                {
                    '\\' => Regex.Unescape(match.Value),
                    '"' => "\"",
                    _ => throw new InvalidOperationException($"Unexpected escape sequence {match.Value}"),
                };
            });
}
