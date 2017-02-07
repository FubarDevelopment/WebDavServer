[![Build status](https://ci.appveyor.com/api/projects/status/3sjjbui36gpc3pvr?svg=true)](https://ci.appveyor.com/project/DecaTec/portable-webdav-library)
[![NuGet](https://img.shields.io/nuget/v/PortableWebDavLibrary.svg)](https://www.nuget.org/packages/PortableWebDavLibrary/)

# Project description

The Portable WebDAV Library is a strongly typed, async WebDAV client library which is fully [RFC 4918](http://tools.ietf.org/html/rfc4918)/[RFC 4331](https://tools.ietf.org/html/rfc4331) compliant. It is implemented as portable class library (PCL) for use on desktop environments as well as mobile devices.

# Main project aims
* Full compliance to [RFC 4918 (*HTTP Extensions for Web Distributed Authoring and Versioning (WebDAV)*)](http://tools.ietf.org/html/rfc4918) and [RFC 4331 (*Quota and Size Properties for Distributed Authoring and Versioning (DAV) Collections*)](https://tools.ietf.org/html/rfc4331)
* Portability: Mobile and desktop environment (project targets .NET Framework 4.5.1, Windows 8.1 and Windows Phone 8.1 or later)
* Level of abstraction: There is a low level of abstraction (class **WebDavClient**) which supports all WebDAV operations directly. This is recommended for users who are familiar with the RFC 4918 specification. A higher level of abstraction is also provided (class **WebDavSession**) that hides most of the WebDAV specific operations and provides an easy access to WebDAV Servers
* Fast and fluid: All operations which might last longer are implemented as asynchronous methods
* WebDAV object model: Object model that represents all WebDAV artifacts used in WebDAV communication (as XML request/response content). No need to build own request XML content strings or parsing the contents of a response of a WebDAV server
So far the project is tested against IIS and ownCloud (sabre/dav) (note that WebDAV locking is only working with ownCloud 8 or earlier releases as with ownCloud 9 WebDAV locking is not supported anymore).

# Obtaining the library
* There is a NuGet package available: Just search for '**PortableWebDavLibrary**' in the '*Manage NuGet Packages...*' window in Visual Studio. You can also find the package [here](https://www.nuget.org/packages/PortableWebDavLibrary/). When using the NuGet package, you do not have to worry about which version of the library should be used (NetFx/Uwp - see below), as the correct reference is added to your project automatically.
* If you prefer the manual installation method, you can find the current release as ZIP file on the [GitHub release page](https://github.com/DecaTec/Portable-WebDAV-Library/releases).

# Two versions of the Portable WebDAV Library
Since v0.3.0.0, the Portable WebDAV Library is split into two parts:
* **DecaTec.WebDav.NetFx**: To be used in projects targeting .NET Framework 4.5 (or later), Windows 8 and ASP.NET Core.
* **DecaTec.WebDav.Uwp**: To be used in projects targeting Windows 8.1, Windows Phone 8.1 and Universal Windows Platform (UWP) apps.

The split was necessary because these were too many limitations when using the library in UWP projects. Now, the two versions of the library use different APIs for web resource access to offer best support for all target platforms.
 
# Documentation
There is a full documentation of the library with some example code available.

* **DecaTec.WebDav.NetFx**:

**[Portable WebDAV Library online documentation (NetFx)](https://decatec.de/ext/PortableWebDAVLibrary/Doc/NetFx/index.html)**

For offline use, you can download the help file (CHM) here:
**[Portable WebDAV Library offline documentation (NexFx)](https://decatec.de/ext/PortableWebDAVLibrary/Doc/NetFx/DecaTec.WebDav.NetFx.Documentation.chm)**
* **DecaTec.WebDav.Uwp**:

**[Portable WebDAV Library online documentation (UWP)](https://decatec.de/ext/PortableWebDAVLibrary/Doc/Uwp/index.html)**

For offline use, you can download the help file (CHM) here:
**[Portable WebDAV Library offline documentation (UWP)](https://decatec.de/ext/PortableWebDAVLibrary/Doc/Uwp/DecaTec.WebDav.Uwp.Documentation.chm)**

# Projects using the Portable WebDAV Library
* Official [Nextcloud](https://nextcloud.com/) Windows app: [Windows App Store](https://www.microsoft.com/store/apps/9nblggh532xq)/[GitHub](https://github.com/nextcloud/windows-universal)
* CCPlayer Pro ([Windows App Store](https://www.microsoft.com/store/apps/9wzdncrfjljw))/CCPlayer UWP Ad ([Windows App Store](https://www.microsoft.com/store/apps/9nblggh4z7q0))
* [WebDAV-AudioPlayer](https://github.com/StefH/WebDAV-AudioPlayer)
