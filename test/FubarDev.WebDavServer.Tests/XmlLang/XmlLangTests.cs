// <copyright file="XmlLangTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using DecaTec.WebDav;
using DecaTec.WebDav.WebDavArtifacts;

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

            var findStatus = await WebDavResponseContentParser.ParseMultistatusResponseContentAsync(findResult.Content).ConfigureAwait(false);
            var creationDateProp = findStatus.Response[0].Items.OfType<Propstat>().Where(x => x.Prop.CreationDateSpecified).Select(x => x.Prop).Single();
            creationDateProp.Language = "en";

            var patchResult = await Client.PropPatchAsync(
                string.Empty,
                new PropertyUpdate()
                {
                    Items = new object[]
                    {
                        new Set()
                        {
                            Prop = creationDateProp,
                        },
                        new Remove(),
                    },
                }).ConfigureAwait(false);
            patchResult.EnsureSuccessStatusCode();

            var findResult2 = await Client.PropFindAsync(
                string.Empty,
                WebDavDepthHeaderValue.Zero,
                new PropFind
                {
                    Item = prop,
                }).ConfigureAwait(false);
            findResult.EnsureSuccessStatusCode();

            var findStatus2 = await WebDavResponseContentParser.ParseMultistatusResponseContentAsync(findResult2.Content).ConfigureAwait(false);
            var creationDateProp2 = findStatus2.Response[0].Items.OfType<Propstat>().Where(x => x.Prop.CreationDateSpecified).Select(x => x.Prop).Single();
            Assert.Null(creationDateProp2.Language);
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

            var findStatus = await WebDavResponseContentParser.ParseMultistatusResponseContentAsync(findResult.Content).ConfigureAwait(false);
            var displayName = findStatus.Response[0].Items.OfType<Propstat>().Select(x => x.Prop).Single();
            displayName.Language = "en";

            var patchResult = await Client.PropPatchAsync(
                string.Empty,
                new PropertyUpdate()
                {
                    Items = new object[]
                    {
                        new Set()
                        {
                            Prop = displayName,
                        },
                        new Remove(),
                    },
                }).ConfigureAwait(false);
            patchResult.EnsureSuccessStatusCode();

            var findResult2 = await Client.PropFindAsync(
                string.Empty,
                WebDavDepthHeaderValue.Zero,
                new PropFind
                {
                    Item = prop,
                }).ConfigureAwait(false);
            findResult.EnsureSuccessStatusCode();

            using (var s = await findResult2.Content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                var doc = XDocument.Load(s);
                var nsmgr = new XmlNamespaceManager(new NameTable());
                nsmgr.AddNamespace("D", "DAV:");
                var displaynameElement = doc.XPathSelectElements("/D:multistatus/D:response/D:propstat/D:prop/D:displayname", nsmgr).Single();
                Assert.Equal("en", displaynameElement.Attribute(XNamespace.Xml + "lang")?.Value);
            }
        }
    }
}
