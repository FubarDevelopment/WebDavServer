# Introduction

This WebDAV server is extensible and mainly a framework that allows hosting
your own WebDAV server using ASP.NET Core.

# Main architecture

The following extension points must be implemented by every hoster.

![Architectural overview](~/images/WebDavServerComponents.png)

- [IFileSystemFactory](xref:FubarDev.WebDavServer.FileSystem.IFileSystemFactory)
- [IPropertyStoreFactory](xref:FubarDev.WebDavServer.Props.Store.IPropertyStoreFactory)

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
An walk-through is available.
