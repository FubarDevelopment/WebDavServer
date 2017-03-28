# How to develop your own property store

WebDAV knows two kinds of properties: Live and Dead properties. The live properties are what is usually returned by the file system (calculated or determined live) and dead properties are stored 1:1 for the next retrieval. Everything that is not a live property is automatically a dead property. A special case is the entity tag handling which is handled using the property store (for dead properties), but might still be a live property.

The architectural overview of properties can be seen in the following picture:

![screenshot](~/images/overview-properties.png)

# Properties

Before we start implementing a property store, we first have to understand the properties.

## Interfaces

The basic interface is [IProperty](xref:FubarDev.WebDavServer.Props.IProperty) which is shared between dead ([IDeadProperty](xref:FubarDev.WebDavServer.Props.Dead.IDeadProperty)) and live ([ILiveProperty](xref:FubarDev.WebDavServer.Props.Live.ILiveProperty)).

A readable property must implement [IUntypedReadableProperty](xref:FubarDev.WebDavServer.Props.IUntypedReadableProperty) while a writeable property must also implement [IUntypedWriteableProperty](xref:FubarDev.WebDavServer.Props.IUntypedWriteableProperty). In WebDAV, all properties are represented as an XML element and those interfaces are used to access the property as raw XML element.

## Typed properties

There are also typed versions of this interface and some basic implementations for the following types:

Type                                                                    | Converter                                                                                         | Default implementation
------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------|------------
[String](xref:System.String)                                            | [StringConverter](xref:FubarDev.WebDavServer.Props.Converters.StringConverter)                    | [GenericStringProperty](xref:FubarDev.WebDavServer.Props.Generic.GenericStringProperty)
[long (Int64)](xref:System.Int64)                                       | [LongConverter](xref:FubarDev.WebDavServer.Props.Converters.LongConverter)                        | No implementation, derive from [SimpleTypedProperty&lt;T&gt;](xref:FubarDev.WebDavServer.Props.SimpleTypedProperty%601)
[DateTime](xref:System.DateTime)                                        | [DateTimeRfc1123Converter](xref:FubarDev.WebDavServer.Props.Converters.DateTimeRfc1123Converter)  | [GenericDateTimeRfc1123Property](xref:FubarDev.WebDavServer.Props.Generic.GenericDateTimeRfc1123Property)
[ETag (EntityTag)](xref:FubarDev.WebDavServer.Model.Headers.EntityTag)  | [EntityTagConverter](xref:FubarDev.WebDavServer.Props.Converters.EntityTagConverter)              | [GetETagProperty](xref:FubarDev.WebDavServer.Props.Dead.GetETagProperty)
[Object (XML)](xref:System.Object)                                      | [XmlConverter&lt;T&gt;](xref:FubarDev.WebDavServer.Props.Converters.XmlConverter%601)             | No implementation, derive from [SimpleTypedProperty&lt;T&gt;](xref:FubarDev.WebDavServer.Props.SimpleTypedProperty%601)

When you use [SimpleTypedProperty<T>](xref:FubarDev.WebDavServer.Props.SimpleTypedProperty`1), then you also have to ensure that you preservere the `xml:lang` attribute. You can access its value using the [SimpleUntypedProperty.Language](xref:FubarDev.WebDavServer.Props.SimpleUntypedProperty.Language) property.

# Property store

A property store only stores dead properties. The entity tags must nut be stored by the property store when the file system supports entity tags natively. Instead, it should pass the entity tags to the file system entry.

## Interfaces

The most important interfaces are [IPropertyStoreFactory](xref:FubarDev.WebDavServer.Props.Store.IPropertyStoreFactory) and [IPropertyStore](xref:FubarDev.WebDavServer.Props.Store.IPropertyStore). The factory is used to create a `IPropertyStore` instance for a given [IFileSystem](xref:FubarDev.WebDavServer.FileSystem.IFileSystem).

# Example implementation

In this example, we'll show you how to implement a property store using SQLite. You can simplify the implementation of a property store by using [PropertyStoreBase](xref:FubarDev.WebDavServer.Props.Store.PropertyStoreBase).

## Factory

The factory is quite simple and consists of the following parts:

* Initialization of the store (DB creation, Table creation)
* Creation of a [SQLitePropertyStore](xref:FubarDev.WebDavServer.Props.Store.SQLite.SQLitePropertyStore)

Both steps are done in the [SQLitePropertyStoreFactory.Create(FubarDev.WebDavServer.FileSystem.IFileSystem)](xref:FubarDev.WebDavServer.Props.Store.SQLite.SQLitePropertyStoreFactory.Create(FubarDev.WebDavServer.FileSystem.IFileSystem)) function.

## Property store

We store the properties in a database file which might exist in the directories exposed to the user. To avoid showing the database file when a user lists a collection, we also implement the [IFileSystemPropertyStore](xref:FubarDev.WebDavServer.Props.Store.IFileSystemPropertyStore) and return true in the [SQLitePropertyStore.IgnoreEntry](xref:FubarDev.WebDavServer.Props.Store.SQLite.SQLitePropertyStore.IgnoreEntry(FubarDev.WebDavServer.FileSystem.IEntry)) function when there is a file in the root directory with the same name as the database file.

### Functions to implement

For the basic implemetation, we only have to implement a couple of functions, like:

* [PropertyStoreBase.GetAsync](xref:FubarDev.WebDavServer.Props.Store.PropertyStoreBase.GetAsync(FubarDev.WebDavServer.FileSystem.IEntry,System.Threading.CancellationToken))
* [PropertyStoreBase.SetAsync](xref:FubarDev.WebDavServer.Props.Store.PropertyStoreBase.SetAsync(FubarDev.WebDavServer.FileSystem.IEntry,System.Collections.Generic.IEnumerable{System.Xml.Linq.XElement},System.Threading.CancellationToken))
* [PropertyStoreBase.RemoveAsync](xref:FubarDev.WebDavServer.Props.Store.PropertyStoreBase.RemoveAsync(FubarDev.WebDavServer.FileSystem.IEntry,System.Threading.CancellationToken))
* [PropertyStoreBase.RemoveAsync](xref:FubarDev.WebDavServer.Props.Store.PropertyStoreBase.RemoveAsync(FubarDev.WebDavServer.FileSystem.IEntry,System.Collections.Generic.IEnumerable{System.Xml.Linq.XName},System.Threading.CancellationToken))
* [PropertyStoreBase.GetDeadETagAsync](xref:FubarDev.WebDavServer.Props.Store.PropertyStoreBase.GetDeadETagAsync(FubarDev.WebDavServer.FileSystem.IEntry,System.Threading.CancellationToken))
* [PropertyStoreBase.UpdateDeadETagAsync](xref:FubarDev.WebDavServer.Props.Store.PropertyStoreBase.UpdateDeadETagAsync(FubarDev.WebDavServer.FileSystem.IEntry,System.Threading.CancellationToken))

The functions `GetDeadETagAsync` and `UpdateDeadETagAsync` are only called when the file system itself doesn't support entity tags. A file system that supports entity tags must implement the [IEntityTagEntry](xref:FubarDev.WebDavServer.FileSystem.IEntityTagEntry).

### Entity Tag property handling

The entity tags must not be modifiable by [PropertyStoreBase.RemoveAsync(IEntry, IEnumerable<XName>, CancellationToken)](xref:FubarDev.WebDavServer.Props.Store.PropertyStoreBase.RemoveAsync(FubarDev.WebDavServer.FileSystem.IEntry,System.Collections.Generic.IEnumerable{System.Xml.Linq.XName},System.Threading.CancellationToken)) and [PropertyStoreBase.SetAsync](xref:FubarDev.WebDavServer.Props.Store.PropertyStoreBase.SetAsync(FubarDev.WebDavServer.FileSystem.IEntry,System.Collections.Generic.IEnumerable{System.Xml.Linq.XElement},System.Threading.CancellationToken)). When the user tries this, then the property store implementation should silently ignore this (`SetAsync`) or should return false (`RemoveAsync`).
