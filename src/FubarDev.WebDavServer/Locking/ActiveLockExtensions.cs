// <copyright file="ActiveLockExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Xml.Linq;

using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Model.Headers;

namespace FubarDev.WebDavServer.Locking
{
    public static class ActiveLockExtensions
    {
        public static XElement ToXElement(this IActiveLock l, bool omitOwner = false, bool omitToken = false)
        {
            var timeout = l.Timeout == TimeoutHeader.Infinite ? "Infinite" : $"Second-{l.Timeout.TotalSeconds:F0}";
            var depth = l.Recursive ? DepthHeader.Infinity : DepthHeader.Zero;
            var lockScope = LockShareMode.Parse(l.ShareMode);
            var lockType = LockAccessType.Parse(l.AccessType);
            var owner = l.GetOwner();
            var result = new XElement(
                WebDavXml.Dav + "activelock",
                new XElement(
                    WebDavXml.Dav + "lockscope",
                    new XElement(lockScope.Name)),
                new XElement(
                    WebDavXml.Dav + "locktype",
                    new XElement(lockType.Name)),
                new XElement(
                    WebDavXml.Dav + "depth",
                    depth.Value));

            if (owner != null && !omitOwner)
                result.Add(owner);

            result.Add(
                new XElement(
                    WebDavXml.Dav + "timeout",
                    timeout));

            if (!omitToken)
            {
                result.Add(
                    new XElement(
                        WebDavXml.Dav + "locktoken",
                        new XElement(WebDavXml.Dav + "href", l.StateToken)));
            }

            result.Add(
                new XElement(
                    WebDavXml.Dav + "lockroot",
                    new XElement(WebDavXml.Dav + "href", l.Href)));

            return result;
        }
    }
}
