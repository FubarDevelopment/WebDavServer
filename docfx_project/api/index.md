# .NET WebDAV server API documentation

This is the place where you can find the API documentation. The most interesting parts for a user are the following interfaces:

- [IFileSystem](xref:FubarDev.WebDavServer.FileSystem.IFileSystem)

  The root file system is a hierarchical system consisting of [ICollection](xref:FubarDev.WebDavServer.FileSystem.ICollection) and [IDocument](xref:FubarDev.WebDavServer.FileSystem.IDocument). The file system is accessed by using the [IFileSystemFactory](xref:FubarDev.WebDavServer.FileSystem.IFileSystemFactory) to get the file system for the currently authenticated [IPrincipal](xref:System.Security.Principal.IPrincipal).

- [IPropertyStore](xref:FubarDev.WebDavServer.Props.Store.IPropertyStore)

  The property store is used for storing dead properties and (if the file system entries don't implement [IEntityTagEntry](xref:FubarDev.WebDavServer.FileSystem.IEntityTagEntry)) the file system entries [EntityTag](xref:FubarDev.WebDavServer.Model.Headers.EntityTag). A property store is always created for a file system using the [IPropertyStoreFactory](xref:FubarDev.WebDavServer.Props.Store.IPropertyStoreFactory).

- [ILockManger](xref:FubarDev.WebDavServer.Locking.ILockManager)

  The lock manager is used to manage active locks issued by the clients.
