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
`src/FubarDev.WebDavServer.Props.Store.InMemory`  | A property store implementation that holds all dead properties in memory only
`src/FubarDev.WebDavServer.Props.Store.TextFile`  | A property store implementation that stores the dead properties in a JSON text file
`src-doc`                                         | Project files for docfx
`test`                                            | Test projects
`third-party`                                     | Third party library
`tools`                                           | binary tools directory
`webdav-docs`                                     | WebDAV specifications

# Namespace overview

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
