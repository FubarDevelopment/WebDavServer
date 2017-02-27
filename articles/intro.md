# Introduction

This WebDAV server is extensible and mainly a framework that allows hosting
your own WebDAV server using ASP.NET Core.

# Main architecture

The following extension points must be implemented by every hoster.

- IFileSystemFactory
- IPropertyStoreFactory

This project provides default implementation for those extension points.

## Default implementations for IFileSystemFactory

The following default implementation is available:

- DotNetFileSystemFactory
- InMemoryFileSystemFactory

## Default implementation for IPropertyStoreFactory

- TextFilePropertyStoreFactory
- InMemoryPropertyStoreFactory

# Getting started

The easiest way to get started is creating a ASP.NET Core project.
An walk-through is available.
