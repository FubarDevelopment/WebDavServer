// <copyright file="GetRangeTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model.Headers;

using MimeKit;
using MimeKit.IO;

using Xunit;

using Encoding = System.Text.Encoding;

namespace FubarDev.WebDavServer.Tests.Handlers
{
    public class GetRangeTests : ServerTestsBase
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

        public GetRangeTests()
            : base(RecursiveProcessingMode.PreferFastest)
        {
        }

        [Fact]
        public async Task GetWithoutRangeTest()
        {
            var ct = CancellationToken.None;
            var fileSystem = GetFileSystem();
            var root = await fileSystem.Root.ConfigureAwait(false);
            var testFile = await root.CreateDocumentAsync("test1.txt", ct).ConfigureAwait(false);
            await FillAsync(testFile, int.MaxValue, ct).ConfigureAwait(false);

            using (var client = Server.CreateClient())
            {
                using (var response = await client.GetAsync("test1.txt", ct).ConfigureAwait(false))
                {
                    var content = response
                        .EnsureSuccessStatusCode().Content;

                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.Equal(_testBlock.Value.Length, content.Headers.ContentLength);

                    var data = await content
                        .ReadAsByteArrayAsync().ConfigureAwait(false);
                    var s = Encoding.UTF8.GetString(data);
                    Assert.Equal(_testText.Value, s);
                }
            }
        }

        [Fact]
        public async Task GetWithSingleRangeTest()
        {
            var ct = CancellationToken.None;
            var fileSystem = GetFileSystem();
            var root = await fileSystem.Root.ConfigureAwait(false);
            var testFile = await root.CreateDocumentAsync("test1.txt", ct).ConfigureAwait(false);
            await FillAsync(testFile, int.MaxValue, ct).ConfigureAwait(false);

            using (var client = Server.CreateClient())
            {
                var range = new RangeHeader("bytes", new RangeHeaderItem(0, 1));
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
                }
            }
        }

        [Fact]
        public async Task GetWithTwoOverlappingRangeTest()
        {
            var ct = CancellationToken.None;
            var fileSystem = GetFileSystem();
            var root = await fileSystem.Root.ConfigureAwait(false);
            var testFile = await root.CreateDocumentAsync("test1.txt", ct).ConfigureAwait(false);
            await FillAsync(testFile, int.MaxValue, ct).ConfigureAwait(false);

            using (var client = Server.CreateClient())
            {
                var range = new RangeHeader("bytes", new RangeHeaderItem(0, 5), new RangeHeaderItem(3, 9));
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
                    Assert.Equal(10, content.Headers.ContentLength);

                    var contentRange = content.Headers.ContentRange;
                    Assert.NotNull(contentRange);
                    Assert.Equal(0, contentRange.From);
                    Assert.Equal(9, contentRange.To);
                    Assert.Equal(_testBlock.Value.Length, contentRange.Length);

                    var data = await content
                        .ReadAsByteArrayAsync().ConfigureAwait(false);
                    var s = Encoding.UTF8.GetString(data);
                    Assert.Equal("1234567890", s);
                }
            }
        }

        [Fact]
        public async Task GetWithTwoRangesTest()
        {
            var ct = CancellationToken.None;
            var fileSystem = GetFileSystem();
            var root = await fileSystem.Root.ConfigureAwait(false);
            var testFile = await root.CreateDocumentAsync("test1.txt", ct).ConfigureAwait(false);
            await FillAsync(testFile, int.MaxValue, ct).ConfigureAwait(false);

            using (var client = Server.CreateClient())
            {
                var range = new RangeHeader("bytes", new RangeHeaderItem(0, 1), new RangeHeaderItem(3, 4));
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

                    var multipart = await ReadMultipartAsync(content, ct).ConfigureAwait(false);
                    Assert.Equal(2, multipart.Count);
                    Assert.All(multipart, entity => Assert.True(entity.ContentType.IsMimeType("text", "plain")));
                    Assert.Collection(
                        multipart,
                        entity =>
                        {
                            var textPart = Assert.IsType<TextPart>(entity);
                            Assert.Equal($"bytes 0-1/{_testBlock.Value.Length}", textPart.Headers["Content-Range"]);
                            Assert.Equal("12", textPart.Text);
                        },
                        entity =>
                        {
                            var textPart = Assert.IsType<TextPart>(entity);
                            Assert.Equal($"bytes 3-4/{_testBlock.Value.Length}", textPart.Headers["Content-Range"]);
                            Assert.Equal("45", textPart.Text);
                        });
                }
            }
        }

        [Fact]
        public async Task GetWithUnsatisfiableRangeTest()
        {
            var ct = CancellationToken.None;
            var fileSystem = GetFileSystem();
            var root = await fileSystem.Root.ConfigureAwait(false);
            var testFile = await root.CreateDocumentAsync("test1.txt", ct).ConfigureAwait(false);
            await FillAsync(testFile, int.MaxValue, ct).ConfigureAwait(false);

            using (var client = Server.CreateClient())
            {
                var range = new RangeHeader("bytes", new RangeHeaderItem(_testBlock.Value.Length - 1, _testBlock.Value.Length));
                var request = new HttpRequestMessage(HttpMethod.Get, "test1.txt")
                {
                    Headers =
                    {
                        Range = RangeHeaderValue.Parse(range.ToString()),
                    },
                };

                using (var response = await client.SendAsync(request, ct).ConfigureAwait(false))
                {
                    Assert.Equal(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
                }
            }
        }

        private static async Task<Multipart> ReadMultipartAsync(HttpContent content, CancellationToken ct)
        {
            var responseStream = await content
                .ReadAsStreamAsync().ConfigureAwait(false);

            var headerStream = new MemoryStream();
            using (var headerWriter = new StreamWriter(headerStream, new UTF8Encoding(false), 1000, true)
            {
                NewLine = "\r\n",
            })
            {
                foreach (var contentHeader in content.Headers)
                {
                    var line = $"{contentHeader.Key}: {string.Join(", ", contentHeader.Value)}";
                    await headerWriter.WriteLineAsync(line).ConfigureAwait(false);
                }

                await headerWriter.WriteLineAsync().ConfigureAwait(false);
            }

            headerStream.Position = 0;

            using (var input = new ChainedStream())
            {
                input.Add(headerStream);
                input.Add(responseStream, true);

                var multipart = MimeEntity.Load(input, ct);
                return Assert.IsType<Multipart>(multipart);
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
