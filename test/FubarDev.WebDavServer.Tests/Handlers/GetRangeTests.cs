// <copyright file="GetRangeTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;

using Xunit;

namespace FubarDev.WebDavServer.Tests.Handlers
{
    public class GetRangeTests : ServerTestsBase
    {
        private static readonly Lazy<byte[]> _testBlock = new Lazy<byte[]>(() =>
        {
            var testTextBuilder = new StringBuilder();
            for (var i = 0; i != 7000; ++i)
                testTextBuilder.Append("1234567890");
            var testText = testTextBuilder.ToString();
            var testData = Encoding.UTF8.GetBytes(testText);
            return testData;
        });

        public GetRangeTests()
            : base(RecursiveProcessingMode.PreferFastest)
        {
        }

        [Fact]
        public async Task GetWithoutRangeTest()
        {
            var ct = CancellationToken.None;
            var root = await FileSystem.Root;
            var testFile = await root.CreateDocumentAsync("test1.txt", ct).ConfigureAwait(false);
            await FillAsync(testFile, int.MaxValue, ct).ConfigureAwait(false);

            using (var client = Server.CreateClient())
            {
                var range = new Range("bytes", new RangeItem(0, 1));
                var request = new HttpRequestMessage(HttpMethod.Get, "test1.txt")
                {
                    Headers =
                    {
                        Range = RangeHeaderValue.Parse(range.ToString()),
                    },
                };

                using (var response = await client.SendAsync(request, ct).ConfigureAwait(false))
                {
                    var content = response
                        .EnsureSuccessStatusCode().Content;

                    Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
                    Assert.Equal(2, content.Headers.ContentLength);

                    var contentRange = content.Headers.ContentRange;
                    Assert.NotNull(contentRange);
                    Assert.Equal(0, contentRange.From);
                    Assert.Equal(1, contentRange.To);
                    Assert.Equal(_testBlock.Value.Length, contentRange.Length);

                    var data = await content
                        .ReadAsByteArrayAsync().ConfigureAwait(false);
                    var s = Encoding.UTF8.GetString(data);
                    Assert.Equal("12", s);
                    /*
                    var responseStream = await response
                        .EnsureSuccessStatusCode().Content
                        .ReadAsStreamAsync().ConfigureAwait(false);

                    var opt = new MimeKit.ParserOptions()
                    {
                        RespectContentLength = true,
                    };
                    var parser = new MimeKit.MimeParser(opt, responseStream);
                    */
                }
            }
        }

        private async Task FillAsync(IDocument document, int maxLength, CancellationToken ct)
        {
            using (var stream = await document.CreateAsync(ct).ConfigureAwait(false))
            {
                await stream.WriteAsync(_testBlock.Value, 0, Math.Min(_testBlock.Value.Length, maxLength), ct).ConfigureAwait(false);
            }
        }
    }
}
