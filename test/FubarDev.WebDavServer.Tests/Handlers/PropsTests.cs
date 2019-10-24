// <copyright file="PropsTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using DecaTec.WebDav.WebDavArtifacts;

using FubarDev.WebDavServer.FileSystem.InMemory;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Live;
using FubarDev.WebDavServer.Tests.Support;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace FubarDev.WebDavServer.Tests.Handlers
{
    public class PropsTests : ServerTestsBase
    {
        private readonly XName[] _propsToIgnoreDocument;
        private readonly XName[] _propsToIgnoreCollection;

        public PropsTests()
            : base(RecursiveProcessingMode.PreferFastest)
        {
            _propsToIgnoreDocument = new[] { LockDiscoveryProperty.PropertyName, DisplayNameProperty.PropertyName, GetETagProperty.PropertyName };
            _propsToIgnoreCollection = new[] { LockDiscoveryProperty.PropertyName, DisplayNameProperty.PropertyName, GetETagProperty.PropertyName };
            Dispatcher = ServiceProvider.GetRequiredService<IWebDavDispatcher>();
        }

        private IWebDavDispatcher Dispatcher { get; }

        [Fact]
        public async Task SetNewProp()
        {
            var ct = CancellationToken.None;
            var root = await FileSystem.Root.ConfigureAwait(false);
            const string resourceName = "text1.txt";

            var doc1 = await root.CreateDocumentAsync(resourceName, ct).ConfigureAwait(false);
            await doc1.FillWithAsync("Dokument 1", ct).ConfigureAwait(false);

            var propsBefore = await doc1.GetPropertyElementsAsync(Dispatcher, ct).ConfigureAwait(false);

            var requestUri = new Uri(Client.BaseAddress, new Uri(resourceName, UriKind.Relative));
            var propertyValue = "<testProp>someValue</testProp>";
            var response = await Client
                .PropPatchAsync(
                    requestUri,
                    new PropertyUpdate
                    {
                        Items = new[]
                        {
                            new Set
                            {
                                Prop = new Prop
                                {
                                    AdditionalProperties = new[]
                                    {
                                        XElement.Parse(propertyValue),
                                    },
                                },
                            },
                        },
                    },
                    ct)
                .ConfigureAwait(false);

            Assert.True(response.IsSuccessStatusCode);

            var child = await root.GetChildAsync(resourceName, ct).ConfigureAwait(false);
            var doc2 = Assert.IsType<InMemoryFile>(child);
            var props2 = await doc2.GetPropertyElementsAsync(Dispatcher, ct).ConfigureAwait(false);
            var changes = PropertyComparer.FindChanges(propsBefore, props2, _propsToIgnoreDocument);
            var addedProperty = Assert.Single(changes);

            Assert.NotNull(addedProperty);

            var expectedAddedChangeItem = PropertyChangeItem.Added(XElement.Parse(propertyValue));

            Assert.Equal(expectedAddedChangeItem.Name, addedProperty.Name);
            Assert.Equal(expectedAddedChangeItem.Change, addedProperty.Change);
            Assert.Equal(expectedAddedChangeItem.Left, addedProperty.Left);
            Assert.True(XNode.DeepEquals(expectedAddedChangeItem.Right, addedProperty.Right));
        }
    }
}
