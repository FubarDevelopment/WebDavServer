using Xunit;

namespace FubarDev.WebDavServer.Models.Tests;

public class EntityTagTests
{
    [Theory]
    [InlineData(false, "", "\"\"", "\"\"")]
    [InlineData(false, "foo", "\"foo\"", "\"foo\"")]
    [InlineData(false, "fo\"o", "\"fo\\\"o\"", "\"fo\"\"o\"")]
    [InlineData(false, "fo\"o", "\"fo\\\"o\"", "\"fo\\\"o\"")]
    [InlineData(true, "", "W/\"\"", "W/\"\"")]
    [InlineData(true, "", "W/\"\"", "w/\"\"")]
    [InlineData(true, "foo", "W/\"foo\"", "W/\"foo\"")]
    [InlineData(true, "foo", "W/\"foo\"", "w/\"foo\"")]
    [InlineData(true, "fo\"o", "W/\"fo\\\"o\"", "W/\"fo\"\"o\"")]
    [InlineData(true, "fo\"o", "W/\"fo\\\"o\"", "W/\"fo\\\"o\"")]
    [InlineData(true, "fo\"o", "W/\"fo\\\"o\"", "w/\"fo\"\"o\"")]
    [InlineData(true, "fo\"o", "W/\"fo\\\"o\"", "w/\"fo\\\"o\"")]
    public void TestSingle(bool isWeak, string value, string toString, string input)
    {
        // \"fo\\\"o\",W/\"foo\" , \"foo\" , w/\"foo\", \"\"
        var entityTags = EntityTag.Parse(input);
        var entityTag = Assert.Single(entityTags);
        Assert.Equal(isWeak, entityTag.IsWeak);
        Assert.Equal(value, entityTag.Value);
        Assert.Equal(toString, entityTag.ToString());
        Assert.False(entityTag.IsEmpty);
    }

    [Theory]
    [InlineData("  \"fo\"\"o\" , W/\"foo\"  ")]
    [InlineData("  \"fo\\\"o\" , W/\"foo\"  ")]
    [InlineData("  \"fo\"\"o\" , w/\"foo\"  ")]
    [InlineData("  \"fo\\\"o\" , w/\"foo\"  ")]
    [InlineData("\"fo\"\"o\",W/\"foo\"")]
    [InlineData("\"fo\\\"o\",W/\"foo\"")]
    [InlineData("\"fo\"\"o\",w/\"foo\"")]
    [InlineData("\"fo\\\"o\",w/\"foo\"")]
    public void TestMultiple(string input)
    {
        var entityTags = EntityTag.Parse(input);
        Assert.Collection(
            entityTags,
            entityTag =>
            {
                Assert.False(entityTag.IsEmpty);
                Assert.False(entityTag.IsWeak);
                Assert.Equal("fo\"o", entityTag.Value);
            },
            entityTag =>
            {
                Assert.False(entityTag.IsEmpty);
                Assert.True(entityTag.IsWeak);
                Assert.Equal("foo", entityTag.Value);
            });
    }
}
