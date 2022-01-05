using System;

using FubarDev.WebDavServer.Parsing;

using Xunit;

namespace FubarDev.WebDavServer.Models.Tests;

public class IfConditionTests
{
    [Theory]
    [InlineData(false, "http://localhost/", "<http://localhost/>")]
    [InlineData(true, "http://localhost/", "not <http://localhost/>")]
    [InlineData(true, "http://localhost/", "Not <http://localhost/>")]
    public void TestStateToken(bool not, string stateToken, string input)
    {
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var parseResult = parser.ParseCondition();
        Assert.True(
            parseResult.IsOk,
            parseResult.IsOk ? "OK" : parseResult.Error.GetFullMessage());

        if (!lexer.IsEnd)
        {
            Assert.Equal(TokenType.End, lexer.Next().Kind);
        }

        var condition = parseResult.Ok.Value;
        Assert.Equal(not, condition.Not);
        Assert.Null(condition.EntityTag);
        Assert.Equal(new Uri(stateToken), condition.StateToken);
    }

    [Theory]
    [InlineData(false, false, "foo", "[\"foo\"]")]
    [InlineData(true, false, "foo", "not [\"foo\"]")]
    [InlineData(true, false, "foo", "Not [\"foo\"]")]
    [InlineData(true, false, "foo", "Not [ \"foo\" ]")]
    [InlineData(false, true, "foo", "[w/\"foo\"]")]
    [InlineData(true, true, "foo", "Not [w/\"foo\"]")]
    public void TestEntityTag(bool not, bool weak, string value, string input)
    {
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var parseResult = parser.ParseCondition();
        Assert.True(
            parseResult.IsOk,
            parseResult.IsOk ? "OK" : parseResult.Error.GetFullMessage());

        if (!lexer.IsEnd)
        {
            Assert.Equal(TokenType.End, lexer.Next().Kind);
        }

        var condition = parseResult.Ok.Value;
        Assert.Equal(not, condition.Not);
        Assert.Null(condition.StateToken);
        Assert.NotNull(condition.EntityTag);
        Assert.Equal(new EntityTag(weak, value), condition.EntityTag!.Value, EntityTagComparer.Weak);
    }
}
