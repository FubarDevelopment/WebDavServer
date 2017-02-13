// <copyright file="PropertyChangeItem.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Xml.Linq;

namespace FubarDev.WebDavServer.Tests.Support
{
    public class PropertyChangeItem
    {
        private PropertyChangeItem(
            PropertyChange change,
            XElement left,
            XElement right)
        {
            Change = change;
            Left = left;
            Right = right;
            Name = (left ?? right).Name;
        }

        public PropertyChange Change { get; }

        public XName Name { get; }

        public XElement Left { get; }

        public XElement Right { get; }

        public static PropertyChangeItem Added(XElement element)
        {
            return new PropertyChangeItem(PropertyChange.Added, null, element);
        }

        public static PropertyChangeItem Removed(XElement element)
        {
            return new PropertyChangeItem(PropertyChange.Removed, element, null);
        }

        public static PropertyChangeItem Changed(XElement left, XElement right)
        {
            return new PropertyChangeItem(PropertyChange.Changed, left, right);
        }
    }
}
