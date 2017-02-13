// <copyright file="IInitializableProperty.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Xml.Linq;

namespace FubarDev.WebDavServer.Props
{
    public interface IInitializableProperty : IProperty
    {
        void Init(XElement initialValue);
    }
}
