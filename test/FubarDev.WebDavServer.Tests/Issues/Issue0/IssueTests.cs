// <copyright file="IssueTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using DecaTec.WebDav;
using DecaTec.WebDav.Headers;

using Xunit;

namespace FubarDev.WebDavServer.Tests.Issues.Issue0
{
    public class IssueTests : ServerTestsBase, IAsyncLifetime
    {
        public IssueTests()
        {
            Client.BaseAddress = new Uri(Client.BaseAddress, new Uri("_dav/", UriKind.Relative));
        }

        protected override IEnumerable<Type> ControllerTypes { get; } = new[]
        {
            typeof(WinRootCompatController),
            typeof(TestWebDavController),
        };

        public async Task InitializeAsync()
        {
            var root = await FileSystem.Root;
            var test1 = await root.CreateCollectionAsync("test1", CancellationToken.None);
            await test1.CreateCollectionAsync("test2", CancellationToken.None);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task CheckRoot()
        {
            var propFindResponse = await Client.PropFindAsync(string.Empty, WebDavDepthHeaderValue.One);
            Assert.Equal(WebDavStatusCode.MultiStatus, propFindResponse.StatusCode);
            var multiStatus = await WebDavResponseContentParser
                .ParseMultistatusResponseContentAsync(propFindResponse.Content).ConfigureAwait(false);
            Assert.Collection(
                multiStatus.Response,
                response => { Assert.Equal("/", response.Href); },
                response => { Assert.Equal("/test1/", response.Href); });
        }

        [Fact]
        public async Task CheckTest1()
        {
            var propFindResponse = await Client.PropFindAsync("test1", WebDavDepthHeaderValue.One);
            Assert.Equal(WebDavStatusCode.MultiStatus, propFindResponse.StatusCode);
            var multiStatus = await WebDavResponseContentParser
                .ParseMultistatusResponseContentAsync(propFindResponse.Content).ConfigureAwait(false);
            Assert.Collection(
                multiStatus.Response,
                response => { Assert.Equal("/test1/", response.Href); },
                response => { Assert.Equal("/test1/test2/", response.Href); });
        }

        [Fact]
        public async Task CheckTest2()
        {
            var propFindResponse = await Client.PropFindAsync("test1/test2", WebDavDepthHeaderValue.One);
            Assert.Equal(WebDavStatusCode.MultiStatus, propFindResponse.StatusCode);
            var multiStatus = await WebDavResponseContentParser
                .ParseMultistatusResponseContentAsync(propFindResponse.Content).ConfigureAwait(false);
            Assert.Collection(
                multiStatus.Response,
                response => { Assert.Equal("/test1/test2/", response.Href); });
        }
    }
}
