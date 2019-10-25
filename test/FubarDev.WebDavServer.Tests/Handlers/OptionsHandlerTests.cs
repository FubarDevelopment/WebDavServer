// <copyright file="OptionsHandlerTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Net.Http;
using System.Threading.Tasks;

using Xunit;

namespace FubarDev.WebDavServer.Tests.Handlers
{
    public class OptionsHandlerTests : ServerTestsBase
    {
        [Fact]
        public async Task OptionsReturnsClass2Test()
        {
            var optionsRequest = new HttpRequestMessage(HttpMethod.Options, "/");
            var result = await Client.SendAsync(optionsRequest).ConfigureAwait(false);
            result.EnsureSuccessStatusCode();
            Assert.True(result.Headers.TryGetValues("DAV", out var davValues));
            Assert.Collection(
                davValues,
                v => Assert.Equal("1", v),
                v => Assert.Equal("2", v));
            Assert.True(result.Headers.TryGetValues("MS-Author-Via", out var msAuthorViaValues));
            Assert.Collection(
                msAuthorViaValues,
                v => Assert.Equal("DAV", v));
        }
    }
}
