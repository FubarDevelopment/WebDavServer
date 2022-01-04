// <copyright file="IssueTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using DecaTec.WebDav;
using DecaTec.WebDav.Headers;

using FubarDev.WebDavServer.Tests.Support.Controllers;

using Xunit;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
namespace FubarDev.WebDavServer.Tests.Issues.Issue53
{
    public class IssueTests : ServerTestsBase, IAsyncLifetime
    {
        private const string BasePath = "_dav/";
        private const string NameTest1 = "test%201";

        public IssueTests()
        {
            Assert.NotNull(Client.BaseAddress);
            Client.BaseAddress = new Uri(Client.BaseAddress!, new Uri(BasePath, UriKind.Relative));
        }

        protected override IEnumerable<Type> ControllerTypes { get; } = new[]
        {
            typeof(WinRootCompatController),
            typeof(WinCompatibleWebDavController),
        };

        public async Task InitializeAsync()
        {
            var fileSystem = GetFileSystem();
            var root = await fileSystem.Root;
            await root.CreateCollectionAsync(NameTest1, CancellationToken.None);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task CheckRootWithoutSlash()
        {
            Assert.NotNull(Client.BaseAddress);
            var requestUrl = new Uri(Client.BaseAddress!.OriginalString.TrimEnd('/'));
            var propFindResponse = await Client.PropFindAsync(requestUrl, WebDavDepthHeaderValue.Zero);
            Assert.Equal(WebDavStatusCode.MultiStatus, propFindResponse.StatusCode);
            var multiStatus = await WebDavResponseContentParser
                .ParseMultistatusResponseContentAsync(propFindResponse.Content).ConfigureAwait(false);
            Assert.Collection(
                multiStatus.Response,
                response => Assert.Equal("/" + BasePath, response.Href));
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
                response => Assert.Equal("/" + BasePath, response.Href),
                response => { Assert.Equal($"/{BasePath}{Uri.EscapeDataString(NameTest1)}/", response.Href); });
        }

        [Fact]
        public async Task CheckTest1()
        {
            var propFindResponse = await Client.PropFindAsync(Uri.EscapeDataString(NameTest1), WebDavDepthHeaderValue.One);
            Assert.Equal(WebDavStatusCode.MultiStatus, propFindResponse.StatusCode);
            var multiStatus = await WebDavResponseContentParser
                .ParseMultistatusResponseContentAsync(propFindResponse.Content).ConfigureAwait(false);
            Assert.Collection(
                multiStatus.Response,
                response =>
                {
                    Assert.Equal($"/{BasePath}{Uri.EscapeDataString(NameTest1)}/", response.Href);
                });
        }
    }
}
