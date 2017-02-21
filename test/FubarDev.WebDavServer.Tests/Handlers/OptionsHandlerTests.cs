// <copyright file="OptionsHandlerTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
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
            IEnumerable<string> davValues;
            Assert.True(result.Headers.TryGetValues("DAV", out davValues));
            Assert.Collection(
                davValues,
                v => Assert.Equal("1", v),
                v => Assert.Equal("2", v));
        }
    }
}
