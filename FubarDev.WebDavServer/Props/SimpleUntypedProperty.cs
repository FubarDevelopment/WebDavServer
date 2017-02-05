// <copyright file="SimpleUntypedProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Props
{
    public abstract class SimpleUntypedProperty : IUntypedWriteableProperty
    {
        protected SimpleUntypedProperty([NotNull] XName name, int cost)
        {
            Name = name;
            Cost = cost;
        }

        public XName Name { get; }

        public int Cost { get; }

        public abstract Task SetXmlValueAsync(XElement element, CancellationToken ct);

        public abstract Task<XElement> GetXmlValueAsync(CancellationToken ct);
    }
}
