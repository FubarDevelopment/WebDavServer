// <copyright file="DeleteTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace FubarDev.WebDavServer.Tests.Handlers
{
    public class DeleteTests : ServerTestsBase
    {
        [Fact]
        public async Task SimpleDelete()
        {
            var ct = CancellationToken.None;
            var root = await GetFileSystem().Root;
            await root.CreateDocumentAsync("test.txt", ct);
            await Client.DeleteAsync("test.txt", ct);
            var child = await root.GetChildAsync("test.txt", ct);
            Assert.Null(child);
        }
    }
}
