using System;

using FubarDev.WebDavServer.Parsing;

using Xunit;

namespace FubarDev.WebDavServer.Models.Tests;

public class IfHeaderTests
{
    [Fact]
    public void TestIfHeaderWithSingleNoTagList()
    {
        var lexer = new Lexer("( <http://statetoken> not <http://statetoken> [\"etag\"] not [w/\"etag\"] )");
        var parser = new Parser(lexer);
        var parseResult = parser.ParseIfHeader().EnsureSuccess();
        Assert.True(parseResult.Ok.Value.IsNoTagList);
        Assert.Collection(
            parseResult.Ok.Value.NoTagLists,
            item => Assert.Collection(
                item.List,
                condition => Assert.Equal(new IfCondition(false, new Uri("http://statetoken"), null), condition),
                condition => Assert.Equal(new IfCondition(true, new Uri("http://statetoken"), null), condition),
                condition => Assert.Equal(new IfCondition(false, null, new EntityTag(false, "etag")), condition),
                condition => Assert.Equal(new IfCondition(true, null, new EntityTag(true, "etag")), condition)));
    }

    [Fact]
    public void TestIfHeaderWithMultipleNoTagLists()
    {
        var lexer = new Lexer("( <http://statetoken> ) ( not <http://statetoken> ) ( [\"etag\"] ) ( not [w/\"etag\"] )");
        var parser = new Parser(lexer);
        var parseResult = parser.ParseIfHeader().EnsureSuccess();
        Assert.True(parseResult.Ok.Value.IsNoTagList);
        Assert.Collection(
            parseResult.Ok.Value.NoTagLists,
            item => Assert.Collection(
                item.List,
                condition => Assert.Equal(new IfCondition(false, new Uri("http://statetoken"), null), condition)),
            item => Assert.Collection(
                item.List,
                condition => Assert.Equal(new IfCondition(true, new Uri("http://statetoken"), null), condition)),
            item => Assert.Collection(
                item.List,
                condition => Assert.Equal(new IfCondition(false, null, new EntityTag(false, "etag")), condition)),
            item => Assert.Collection(
                item.List,
                condition => Assert.Equal(new IfCondition(true, null, new EntityTag(true, "etag")), condition)));
    }

    [Fact]
    public void TestIfHeaderWithSingleTaggedList()
    {
        var lexer = new Lexer("</test> ( <http://st> not <http://st> [\"t\"] not [w/\"t\"] )");
        var parser = new Parser(lexer);
        var parseResult = parser.ParseIfHeader().EnsureSuccess();
        Assert.True(parseResult.Ok.Value.IsTaggedList);
        Assert.Collection(
            parseResult.Ok.Value.TaggedLists,
            item =>
            {
                Assert.Equal("/test", item.ResourceTag.ToString());
                Assert.Collection(
                    item.Lists,
                    list => Assert.Collection(
                        list,
                        condition => Assert.Equal(new IfCondition(false, new Uri("http://st"), null), condition),
                        condition => Assert.Equal(new IfCondition(true, new Uri("http://st"), null), condition),
                        condition => Assert.Equal(new IfCondition(false, null, new EntityTag(false, "t")), condition),
                        condition => Assert.Equal(new IfCondition(true, null, new EntityTag(true, "t")), condition)));
            });
    }

    [Fact]
    public void TestIfHeaderWithSingleTaggedListWithMultipleLists()
    {
        var lexer = new Lexer("</test> ( <http://st> ) ( not <http://st> ) ( [\"t\"] ) ( not [w/\"t\"] )");
        var parser = new Parser(lexer);
        var parseResult = parser.ParseIfHeader().EnsureSuccess();
        Assert.True(parseResult.Ok.Value.IsTaggedList);
        Assert.Collection(
            parseResult.Ok.Value.TaggedLists,
            item =>
            {
                Assert.Equal("/test", item.ResourceTag.ToString());
                Assert.Collection(
                    item.Lists,
                    list => Assert.Collection(
                        list,
                        condition => Assert.Equal(new IfCondition(false, new Uri("http://st"), null), condition)),
                    list => Assert.Collection(
                        list,
                        condition => Assert.Equal(new IfCondition(true, new Uri("http://st"), null), condition)),
                    list => Assert.Collection(
                        list,
                        condition => Assert.Equal(new IfCondition(false, null, new EntityTag(false, "t")), condition)),
                    list => Assert.Collection(
                        list,
                        condition => Assert.Equal(new IfCondition(true, null, new EntityTag(true, "t")), condition)));
            });
    }

    [Fact]
    public void TestIfHeaderWithMultipleTaggedLists()
    {
        var lexer = new Lexer("</test1> ( <http://st> ) </test2> ( not <http://st> )");
        var parser = new Parser(lexer);
        var parseResult = parser.ParseIfHeader().EnsureSuccess();
        Assert.True(parseResult.Ok.Value.IsTaggedList);
        Assert.Collection(
            parseResult.Ok.Value.TaggedLists,
            item =>
            {
                Assert.Equal("/test1", item.ResourceTag.ToString());
                Assert.Collection(
                    item.Lists,
                    list => Assert.Collection(
                        list,
                        condition => Assert.Equal(new IfCondition(false, new Uri("http://st"), null), condition)));
            },
            item =>
            {
                Assert.Equal("/test2", item.ResourceTag.ToString());
                Assert.Collection(
                    item.Lists,
                    list => Assert.Collection(
                        list,
                        condition => Assert.Equal(new IfCondition(true, new Uri("http://st"), null), condition)));
            });
    }
}
