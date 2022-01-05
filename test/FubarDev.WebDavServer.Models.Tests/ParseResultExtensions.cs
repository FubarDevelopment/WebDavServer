using FubarDev.WebDavServer.Parsing;

using Xunit;

using Yoakke.Parser;

namespace FubarDev.WebDavServer.Models.Tests;

public static class ParseResultExtensions
{
    /// <summary>
    /// Ensure that the result is successful.
    /// </summary>
    /// <param name="result">The result to validate.</param>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <returns>The result.</returns>
    public static ParseResult<T> EnsureSuccess<T>(this ParseResult<T> result)
    {
        if (result.IsOk)
        {
            return result;
        }

        Assert.True(result.IsOk, result.Error.GetFullMessage());
        return result;
    }
}
