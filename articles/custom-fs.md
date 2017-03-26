# How to develop your own file system

The implementation of a file system consists of the following parts as can be seen in the following picture:

![screenshot](~/images/overview-filesystem.png)

The order in which a file system gets implemented should be:

1. The [IFileSystemFactory](xref:FubarDev.WebDavServer.FileSystem.IFileSystemFactory)
2. The [IFileSystem](xref:FubarDev.WebDavServer.FileSystem.IFileSystem)
3. The [ICollection](xref:FubarDev.WebDavServer.FileSystem.ICollection)
4. The [IDocument](xref:FubarDev.WebDavServer.FileSystem.IDocument)

The [IEntry](xref:FubarDev.WebDavServer.FileSystem.IEntry) is the base for both the `ICollection` and `IDocument`. It depends on the underlying data source and the developers preferences if there should be a base class for `IEntry` or if the shared functionality should be implemented separately for both `ICollection` and `IDocument`.

Due to the limitation of the dependency injection frameworks it's not possible to use `async`/`await` during the resolve operation of a DI container, which is the main reason that [IFileSystem.Root](xref:FubarDev.WebDavServer.FileSystem.IFileSystem.Root) is an [AsyncLazy<T>](xref:FubarDev.WebDavServer.AsyncLazy`1) for the root collection. This allows the lazy loading of the root collection at a later time.

# IFileSystemFactory

The [IFileSystemFactory](xref:FubarDev.WebDavServer.FileSystem.IFileSystemFactory) should be kept very simple. It usually takes the same parameters as its [IFileSystem](xref:FubarDev.WebDavServer.FileSystem.IFileSystem) implementation, because those parameters must be passed to the `IFileSystem` implementation.

The implementation usually requires implementations of:

* [PathTraversalEngine](xref:FubarDev.WebDavServer.FileSystem.PathTraversalEngine)
* [IPropertyStoreFactory](xref:FubarDev.WebDavServer.Props.Store.IPropertyStoreFactory)
* [ILockManager](xref:FubarDev.WebDavServer.Locking.ILockManager)

# IFileSystem

The file system provides the following information:

* The root [ICollection](xref:FubarDev.WebDavServer.FileSystem.ICollection)
* The [IPropertyStore](xref:FubarDev.WebDavServer.Props.Store.IPropertyStore)
* The [ILockManager](xref:FubarDev.WebDavServer.Locking.ILockManager)

The [PathTraversalEngine](xref:FubarDev.WebDavServer.FileSystem.PathTraversalEngine) is used to implement the [IFileSystem.SelectAsync](xref:FubarDev.WebDavServer.FileSystem.IFileSystem.SelectAsync*) function.

## ILocalFileSystem

A file system might optionally implement the [ILocalFileSystem](xref:FubarDev.WebDavServer.FileSystem.ILocalFileSystem) interface. This interface can be used to determine the path where the file system is mapped to.

## IMountPointManager

This interface enables mount point support. It enables scenarios where the in-memory file system is used to provide a virtual read-only file system where the collections point to other file systems.

An example can be found in the unit tests.

# ICollection

A collection maps to a file system directory and is used to determine or create its child elements (either collections or documents).

# IDocument

The document maps to a file in a file system and is mainly used to read or write its content and to copy or move the file (within the same file system).

# IEntry

The base interface of `ICollection` and `IDocument` provides common information shared between a document and a collection, like its name, parent, creation date, etc...
