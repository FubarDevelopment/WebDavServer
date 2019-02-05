# Project overview

We will start with a simple directory based overview of the WebDAV server project.

Path                                              | Description
--------------------------------------------------|---------------------------------------------------------------------------------------------------------------------
`api`                                             | API documentation
`articles`                                        | The articles for the documentation
`docs`                                            | The compiled documentation
`images`                                          | Images for the documentation
`src`                                             | The source code
`src/FubarDev.WebDavServer`                       | The core WebDAV server assembly
`src/FubarDev.WebDavServer.FileSystem.DotNet`     | A file system for the WebDAV server using [System.IO](xref:System.IO)
`src/FubarDev.WebDavServer.FileSystem.InMemory`   | A file system for the WebDAV server storing all data in memory only
`src/FubarDev.WebDavServer.FileSystem.SQLite`     | A file system for the WebDAV server storing all data in a SQLite database
`src/FubarDev.WebDavServer.Locking.InMemory`      | A [ILockManager](xref:FubarDev.WebDavServer.Locking.ILockManager) implementation that holds its locks only in memory
`src/FubarDev.WebDavServer.Locking.SQLite`        | An `ILockManager` implementation that holds its locks only in an SQLite database
`src/FubarDev.WebDavServer.Props.Store.InMemory`  | A property store implementation that holds all dead properties in memory only
`src/FubarDev.WebDavServer.Props.Store.TextFile`  | A property store implementation that stores the dead properties in a JSON text file
`src/FubarDev.WebDavServer.Props.Store.SQLite`    | A property store implementation that stores the dead properties in an SQLite database
`src-doc`                                         | Project files for docfx
`test`                                            | Test projects
`third-party`                                     | Third party library
`tools`                                           | binary tools directory
`webdav-docs`                                     | WebDAV specifications

# Namespaces

Namespace                                                                               | Description 
----------------------------------------------------------------------------------------|----------------------------------------------
[FubarDev.WebDavServer](xref:FubarDev.WebDavServer)                                     | Common interfaces and classes of global interest
[FubarDev.WebDavServer.Dispatchers](xref:FubarDev.WebDavServer.Dispatchers)             | WebDAV class interfaces and dispatchers to their handlers
[FubarDev.WebDavServer.Engines](xref:FubarDev.WebDavServer.Engines)                     | Some commonly used engines. The only one here is currently [RecursiveExecutionEngine](xref:FubarDev.WebDavServer.Engines.RecursiveExecutionEngine`3).
[FubarDev.WebDavServer.FileSystem](xref:FubarDev.WebDavServer.FileSystem)               | The interfaces and classes for the underlying file system support
[FubarDev.WebDavServer.Formatters](xref:FubarDev.WebDavServer.Formatters)               | The formatter for WebDAV XML responses
[FubarDev.WebDavServer.Handlers](xref:FubarDev.WebDavServer.Handlers)                   | The interfaces for the handlers of the HTTP WebDAV methods
[FubarDev.WebDavServer.Handlers.Impl](xref:FubarDev.WebDavServer.Handlers.Impl)         | The default implementations of the interfaces for the handlers of the HTTP WebDAV methods
[FubarDev.WebDavServer.Locking](xref:FubarDev.WebDavServer.Locking)                     | The interfaces and classes the `LOCK`/`UNLOCK` support for WebDAV
[FubarDev.WebDavServer.Model](xref:FubarDev.WebDavServer.Model)                         | The model for the WebDAV XML messages
[FubarDev.WebDavServer.Model.Headers](xref:FubarDev.WebDavServer.Model.Headers)         | The model for the WebDAV HTTP headers
[FubarDev.WebDavServer.Props](xref:FubarDev.WebDavServer.Props)                         | The interfaces and classes for the underlying WebDAV property support
[FubarDev.WebDavServer.Props.Converters](xref:FubarDev.WebDavServer.Props.Converters)   | The interfaces and classes for WebDAV property converters that support conversion between a given value and an [XElement](xref:System.Xml.Linq.XElement)
[FubarDev.WebDavServer.Props.Dead](xref:FubarDev.WebDavServer.Props.Dead)               | The interfaces and classes for the support of dead properties
[FubarDev.WebDavServer.Props.Filters](xref:FubarDev.WebDavServer.Props.Filters)         | Filters for properties used by the `PROPFIND` implementation
[FubarDev.WebDavServer.Props.Generic](xref:FubarDev.WebDavServer.Props.Generic)         | Implementations of generic typed WebDAV properties
[FubarDev.WebDavServer.Props.Live](xref:FubarDev.WebDavServer.Props.Live)               | The interfaces and classes for the support of live properties
[FubarDev.WebDavServer.Props.Store](xref:FubarDev.WebDavServer.Props.Store)             | The interfaces and classes for the a storage of dead properties (and [EntityTag](xref:FubarDev.WebDavServer.Model.Headers.EntityTag))
[FubarDev.WebDavServer.Utils](xref:FubarDev.WebDavServer.Utils)                         | Utility classes

# File system

A file system implementation starts with a [IFileSystemFactory](xref:FubarDev.WebDavServer.FileSystem.IFileSystemFactory).
It's used to create an instance of an [IFileSystem](xref:FubarDev.WebDavServer.FileSystem.IFileSystem) for a given user.

The file system consists of a root [ICollection](xref:FubarDev.WebDavServer.FileSystem.ICollection) which contains
child [ICollection](xref:FubarDev.WebDavServer.FileSystem.ICollection)s and [IDocument](xref:FubarDev.WebDavServer.FileSystem.IDocument)s.
Both inherit from a common interface [IEntry](xref:FubarDev.WebDavServer.FileSystem.IEntry).

![screenshot](~/images/overview-filesystem.png)

# WebDAV properties

WebDAV knows two types of properties, live and dead properties. The dead properties must be stored using a
[IPropertyStore](xref:FubarDev.WebDavServer.Props.Store.IPropertyStore). The property store is created
using the [IPropertyStoreFactory](xref:FubarDev.WebDavServer.Props.Store.IPropertyStoreFactory) for a
given [IFileSystem](xref:FubarDev.WebDavServer.FileSystem.IFileSystem).

Every property should at least implement [ILiveProperty](xref:FubarDev.WebDavServer.Props.Live.ILiveProperty) or
[IDeadProperty](xref:FubarDev.WebDavServer.Props.Dead.IDeadProperty). That a dead property isn't writeable is
a design decision to allow storing an entity tag as dead property too.

The [ITypedReadableProperty`1](xref:FubarDev.WebDavServer.Props.ITypedReadableProperty`1) and
[ITypedWriteableProperty`1](xref:FubarDev.WebDavServer.Props.ITypedWriteableProperty`1) interfaces are convenience
interfaces that allow accessing the dead property value without the need to handle all the XML fluff.

![screenshot](~/images/overview-properties.png)

# Locking

The locking functionality consists of the following main parts:

* [ILockManager](xref:FubarDev.WebDavServer.Locking.ILockManager)

  This is the interface used to manage WebDAV locks. It ensures that lock conflicts are detected
  and stored.

* [ILock](xref:FubarDev.WebDavServer.Locking.ILock)

  This is the base interface for all lock related information. It's implemented by both the
  [Lock](xref:FubarDev.WebDavServer.Locking.Lock) and [IActiveLock](xref:FubarDev.WebDavServer.Locking.IActiveLock)
  classes.

* [Lock](xref:FubarDev.WebDavServer.Locking.Lock)

  This is the class that contains all information about a lock that the caller wants to create.
  It's created by a `LOCK` request or any other request that creates an implicit lock.

* [IActiveLock](xref:FubarDev.WebDavServer.Locking.IActiveLock)

  This interface contains all information about an active lock, like e.g. the state token, the time
  this lock was issued and when it expires.

![screenshot](~/images/overview-locking.png)

# `xml:lang` behaviour

The RFC4918 specification, section 4.4 explicitly states that it's not possible to define the same property
twice on a single resource. This means, that xml:lang isn't used to provide multi-language translations.
Instead, it's just an indicator for the language of the properties value.

The `xml:lang` property is handled in the following way:

* Live properties never return an `xml:lang`
* The `getetag` property never returns an `xml:lang`
* The `getcontenttype`, and `getcontentlanguage` may return an `xml:lang` even though it doesn't
  make sense for them
* The `xml:lang` attribute of a dead property element has precedence over  the `xml:lang` property
  of the `DAV:props` element
* A `PROPFIND` doesn't use `xml:lang` at all
* A `PROPPATCH` `set` respects the transmitted `xml:lang`
* A `PROPPATCH` `remove` doesn't use `xml:lang`
