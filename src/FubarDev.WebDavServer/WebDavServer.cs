// <copyright file="WebDavServer.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;

using FubarDev.WebDavServer.Dispatchers;
using FubarDev.WebDavServer.Formatters;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer
{
    public class WebDavServer : IWebDavDispatcher
    {
        public WebDavServer([NotNull] IWebDavClass1 webDavClass1, [NotNull] IWebDavOutputFormatter formatter, [CanBeNull] IWebDavClass2 webDavClass2)
        {
            Formatter = formatter;
            Class1 = webDavClass1;
            Class2 = webDavClass2;
            var classes = new IWebDavClass[] { webDavClass1, webDavClass2 }.Where(x => x != null).ToList();
            SupportedClasses = classes.Select(x => x.Version).ToList();
            SupportedHttpMethods = classes.SelectMany(x => x.HttpMethods).ToList();
        }

        public IReadOnlyCollection<string> SupportedHttpMethods { get; }

        public IReadOnlyCollection<string> SupportedClasses { get; }

        public IWebDavOutputFormatter Formatter { get; }

        public IWebDavClass1 Class1 { get; }

        public IWebDavClass2 Class2 { get; }
    }
}
