// <copyright file="SelectionResultType.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.FileSystem
{
    public enum SelectionResultType
    {
        FoundCollection,
        FoundDocument,
        MissingDocumentOrCollection,
        MissingCollection
    }
}
