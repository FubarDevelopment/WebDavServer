# Introduction

This WebDAV server is extensible and mainly a framework that allows hosting
your own WebDAV server using ASP.NET Core.

# Main architecture

The following extension points must be implemented by every hoster.

![Architectural overview](~/images/WebDavServerComponents.png)

- [IFileSystem](xref:FubarDev.WebDavServer.FileSystem.IFileSystem)

  The root file system is a hierarchical system consisting of [ICollection](xref:FubarDev.WebDavServer.FileSystem.ICollection) and [IDocument](xref:FubarDev.WebDavServer.FileSystem.IDocument). The file system is accessed by using the [IFileSystemFactory](xref:FubarDev.WebDavServer.FileSystem.IFileSystemFactory) to get the file system for the currently authenticated [IPrincipal](xref:System.Security.Principal.IPrincipal).

- [IPropertyStore](xref:FubarDev.WebDavServer.Props.Store.IPropertyStore)

  The property store is used for storing dead properties and (if the file system entries don't implement [IEntityTagEntry](xref:FubarDev.WebDavServer.FileSystem.IEntityTagEntry)) the file system entries [EntityTag](xref:FubarDev.WebDavServer.Model.Headers.EntityTag). A property store is always created for a file system using the [IPropertyStoreFactory](xref:FubarDev.WebDavServer.Props.Store.IPropertyStoreFactory).

This project provides default implementation for those extension points.

## Default implementations for [IFileSystemFactory](xref:FubarDev.WebDavServer.FileSystem.IFileSystemFactory)

The following default implementation is available:

- [DotNetFileSystemFactory](xref:FubarDev.WebDavServer.FileSystem.DotNet.DotNetFileSystemFactory)
- [InMemoryFileSystemFactory](xref:FubarDev.WebDavServer.FileSystem.InMemory.InMemoryFileSystemFactory)

## Default implementation for [IPropertyStoreFactory](xref:FubarDev.WebDavServer.Props.Store.IPropertyStoreFactory)

- [TextFilePropertyStoreFactory](xref:FubarDev.WebDavServer.Props.Store.TextFile.TextFilePropertyStoreFactory)
- [InMemoryPropertyStoreFactory](xref:FubarDev.WebDavServer.Props.Store.InMemory.InMemoryPropertyStoreFactory)

# Getting started

The easiest way to get started is creating a ASP.NET Core project.
A [walk-through](getting-started.md) is available.
