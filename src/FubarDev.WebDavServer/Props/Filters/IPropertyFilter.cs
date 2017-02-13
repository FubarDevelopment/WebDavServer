// <copyright file="IPropertyFilter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace FubarDev.WebDavServer.Props.Filters
{
    public interface IPropertyFilter
    {
        void Reset();

        bool IsAllowed(IProperty property);

        void NotifyOfSelection(IProperty property);

        IEnumerable<MissingProperty> GetMissingProperties();
    }
}
