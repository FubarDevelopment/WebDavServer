// <copyright file="PutTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

using Encoding = System.Text.Encoding;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace FubarDev.WebDavServer.Tests.Handlers
{
    public class PutTests : ServerTestsBase
    {
        private static readonly Lazy<string> _testText = new Lazy<string>(() =>
        {
            var testTextBuilder = new StringBuilder();
            for (var i = 0; i != 7000; ++i)
                testTextBuilder.Append("1234567890");
            var testText = testTextBuilder.ToString();
            return testText;
        });

        private static readonly Lazy<byte[]> _testBlock = new Lazy<byte[]>(() => Encoding.UTF8.GetBytes(_testText.Value));

        public PutTests()
            : base(RecursiveProcessingMode.PreferFastest)
        {
        }

        [Fact]
        public async Task PutTextWithContentType()
        {
            var ct = CancellationToken.None;
            using (var client = Server.CreateClient())
            {
                var sourceContent = new ByteArrayContent(_testBlock.Value);
                sourceContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                using (var response = await client.PutAsync("test1.txt", sourceContent, ct).ConfigureAwait(false))
                {
                    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                }

                using (var response = await client.GetAsync("test1.txt", ct).ConfigureAwait(false))
                {
                    var content = response
                        .EnsureSuccessStatusCode().Content;

                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.Equal(_testBlock.Value.Length, content.Headers.ContentLength);
                    Assert.Equal("application/octet-stream", content.Headers.ContentType.ToString());
                }
            }
        }

        [Fact]
        public async Task PutTextWithoutContentType()
        {
            var ct = CancellationToken.None;
            using var client = Server.CreateClient();
            var sourceContent = new ByteArrayContent(_testBlock.Value);
            using (var response = await client.PutAsync("test1.txt", sourceContent, ct).ConfigureAwait(false))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            }

            using (var response = await client.GetAsync("test1.txt", ct).ConfigureAwait(false))
            {
                var content = response
                    .EnsureSuccessStatusCode().Content;

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(_testBlock.Value.Length, content.Headers.ContentLength);
                Assert.Equal("text/plain", content.Headers.ContentType.ToString());
            }
        }

        [Fact]
        public async Task PutTextWithContentRange()
        {
            var ct = CancellationToken.None;
            using var client = Server.CreateClient();
            var sourceContent1 = new ByteArrayContent(Encoding.UTF8.GetBytes("ab"));
            using (var response = await client.PutAsync("test1.txt", sourceContent1, ct).ConfigureAwait(false))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            }

            var sourceContent2 = new ByteArrayContent(Encoding.UTF8.GetBytes("c"))
            {
                Headers =
                {
                    ContentRange = new(1, 1),
                },
            };
            using (var response = await client.PutAsync("test1.txt", sourceContent2, ct).ConfigureAwait(false))
            {
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }

            using (var response = await client.GetAsync("test1.txt", ct).ConfigureAwait(false))
            {
                var content = response
                    .EnsureSuccessStatusCode().Content;

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(2, content.Headers.ContentLength);
                Assert.Equal("text/plain", content.Headers.ContentType.ToString());
                var text = await content.ReadAsStringAsync().ConfigureAwait(false);
                Assert.Equal("ac", text);
            }
        }
    }
}
