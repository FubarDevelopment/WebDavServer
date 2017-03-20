// <copyright file="XmlLangTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

using DecaTec.WebDav;
using DecaTec.WebDav.WebDavArtifacts;

using FubarDev.WebDavServer.Model;

using Xunit;

namespace FubarDev.WebDavServer.Tests.XmlLang
{
    public class XmlLangTests : ServerTestsBase
    {
        [Fact]
        public async Task LiveReturnsNoXmlLangTest()
        {
            var prop = new Prop
            {
                CreationDateSpecified = true,
                CreationDate = string.Empty,
            };

            var findResult = await Client.PropFindAsync(
                string.Empty,
                WebDavDepthHeaderValue.Zero,
                new PropFind
                {
                    Item = prop,
                }).ConfigureAwait(false);
            findResult.EnsureSuccessStatusCode();

            var findStatus = await Read(findResult.Content);
            var creationDateProp = findStatus.Root?.Element(WebDavXml.Dav + "response")?.Element(WebDavXml.Dav + "propstat")?.Element(WebDavXml.Dav + "prop")?.Element(WebDavXml.Dav + "creationdate");
            Assert.NotNull(creationDateProp);
            creationDateProp.SetAttributeValue(XNamespace.Xml + "lang", "en");

            var patchDoc = new XDocument(
                new XElement(
                    WebDavXml.Dav + "propertyupdate",
                    new XElement(
                        WebDavXml.Dav + "set",
                        creationDateProp)));

            var patchResult = await Client
                                  .PropPatchAsync(
                                      string.Empty,
                                      patchDoc.ToString(SaveOptions.OmitDuplicateNamespaces))
                                  .ConfigureAwait(false);
            patchResult.EnsureSuccessStatusCode();

            var findResult2 = await Client.PropFindAsync(
                string.Empty,
                WebDavDepthHeaderValue.Zero,
                new PropFind
                {
                    Item = prop,
                }).ConfigureAwait(false);
            findResult.EnsureSuccessStatusCode();

            var findStatus2 = await Read(findResult2.Content);
            var creationDateProp2 = findStatus2.Root?.Element(WebDavXml.Dav + "response")?.Element(WebDavXml.Dav + "propstat")?.Element(WebDavXml.Dav + "prop")?.Element(WebDavXml.Dav + "creationdate");
            Assert.NotNull(creationDateProp2);
            Assert.Null(creationDateProp2.Attribute(XNamespace.Xml + "lang"));
        }

        [Fact]
        public async Task DeadPropertyPreservesXmlLangTest()
        {
            var prop = new Prop
            {
                DisplayName = string.Empty,
            };

            var findResult = await Client.PropFindAsync(
                string.Empty,
                WebDavDepthHeaderValue.Zero,
                new PropFind
                {
                    Item = prop,
                }).ConfigureAwait(false);
            findResult.EnsureSuccessStatusCode();

            var findStatus = await Read(findResult.Content);
            var displayNameProp = findStatus.Root?.Element(WebDavXml.Dav + "response")?.Element(WebDavXml.Dav + "propstat")?.Element(WebDavXml.Dav + "prop")?.Element(WebDavXml.Dav + "displayname");
            Assert.NotNull(displayNameProp);
            displayNameProp.SetAttributeValue(XNamespace.Xml + "lang", "en");

            var patchDoc = new XDocument(
                new XElement(
                    WebDavXml.Dav + "propertyupdate",
                    new XElement(
                        WebDavXml.Dav + "set",
                        displayNameProp)));

            var patchResult = await Client
                                  .PropPatchAsync(
                                      string.Empty,
                                      patchDoc.ToString(SaveOptions.OmitDuplicateNamespaces))
                                  .ConfigureAwait(false);
            patchResult.EnsureSuccessStatusCode();

            var findResult2 = await Client.PropFindAsync(
                string.Empty,
                WebDavDepthHeaderValue.Zero,
                new PropFind
                {
                    Item = prop,
                }).ConfigureAwait(false);
            findResult.EnsureSuccessStatusCode();

            var findStatus2 = await Read(findResult2.Content);
            var displayNameProp2 = findStatus2.Root?.Element(WebDavXml.Dav + "response")?.Element(WebDavXml.Dav + "propstat")?.Element(WebDavXml.Dav + "prop")?.Element(WebDavXml.Dav + "displayname");
            Assert.NotNull(displayNameProp2);
            Assert.Null(displayNameProp2.Attribute(XNamespace.Xml + "lang"));
        }

        private async Task<XDocument> Read(HttpContent content)
        {
            using (var stream = await content.ReadAsStreamAsync().ConfigureAwait(false))
                return XDocument.Load(stream);
        }
    }
}
