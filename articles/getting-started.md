# You own WebDAV server

This walk-through shows how to create your own WebDAV server using Visual Studio 2017.

# Create the basic project

The easiest (and currently only supported) way to create a WebDAV server
is using a ASP.NET Core project as host.

1. Create the ASP.NET Core project

   ![screenshot](~/images/walk-through/01-create-project.png)

2. Select the WebAPI template

   ![screenshot](~/images/walk-through/02-select-webapi.png)

# Configure the WebDAV NuGet repository (optional)

1. Open the package manager setup

   ![screenshot](~/images/walk-through/03-package-manager-setup.png)

2. Add the package source

   The package source for the WebDAV server is (until release) on [MyGet](https://www.myget.org/feed/Packages/webdav-server). The URL is for the NuGet v3 feed is [https://www.myget.org/F/webdav-server/api/v3/index.json](https://www.myget.org/F/webdav-server/api/v3/index.json).

   ![screenshot](~/images/walk-through/03-webdav-server-nuget-repository.png)

# Add the WebDAV NuGet packages

This are the packages that must be installed:

* [FubarDev.WebDavServer](https://www.myget.org/feed/webdav-server/package/nuget/FubarDev.WebDavServer)

   This is the main package containing the WebDAV implementation.

* [FubarDev.WebDavServer.FileSystem.DotNet](https://www.myget.org/feed/webdav-server/package/nuget/FubarDev.WebDavServer.FileSystem.DotNet)

   This package contains the [System.IO](xref:System.IO) based file system implementation.

* [FubarDev.WebDavServer.Props.Store.TextFile](https://www.myget.org/feed/webdav-server/package/nuget/FubarDev.WebDavServer.Props.Store.TextFile)

   This package stores the dead properties in a JSON file.

* [FubarDev.WebDavServer.Locking.InMemory](https://www.myget.org/feed/webdav-server/package/nuget/FubarDev.WebDavServer.Locking.InMemory)

   This is the implementation of the locking that doesn't persist the information about the active locks.

* [FubarDev.WebDavServer.AspNetCore](https://www.myget.org/feed/webdav-server/package/nuget/FubarDev.WebDavServer.AspNetCore)

   This package contains the glue between ASP.NET Core and the WebDAV server.

![screenshot](~/images/walk-through/04-packages.png)

Finally, we should install some remaining package updates.

![screenshot](~/images/walk-through/05-update-packages.png)

# Create the WebDAV controller

1. Rename the `ValuesController.cs` to `WebDavController.cs`

   ![screenshot](~/images/walk-through/06-rename-controller.png)

2. Replace the content of `WebDavController.cs` with the following:

    ```csharp
    using System;

    using FubarDev.WebDavServer;
    using FubarDev.WebDavServer.AspNetCore;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    namespace TestWebDavServer.Controllers
    {
        [Route("{*path}")]
        public class WebDavController : WebDavControllerBase
        {
            public WebDavController(IWebDavContext context, IWebDavDispatcher dispatcher, ILogger<WebDavIndirectResult> responseLogger = null)
                : base(context, dispatcher, responseLogger)
            {
            }
        }
    }
    ```

    ![screenshot](~/images/walk-through/07-modify-controller.png)

## Explanation

This controller uses the base controller class from [FubarDev.WebDavServer.AspNetCore](xref:FubarDev.WebDavServer.AspNetCore). This
controller contains everything that's needed for a WebDAV server class 1,2 implementation.

The [IWebDavContext](xref:FubarDev.WebDavServer.IWebDavContext) is the request context (similar to the `HttpContext` used in ASP.NET Core). It is used to query some basic information about the current request.

The [IWebDavDispatcher](xref:FubarDev.WebDavServer.IWebDavDispatcher) is the interface to the main WebDAV server functionality.

# Configure the servies

Replace in `Startup.cs` the `.AddMvc()` in function `ConfigureServices` with the following code:

```csharp
.Configure<DotNetFileSystemOptions>(
    opt =>
    {
        opt.RootPath = Path.Combine(Path.GetTempPath(), "webdav");
        opt.AnonymousUserName = "anonymous";
    })
.AddTransient<IPropertyStoreFactory, TextFilePropertyStoreFactory>()
.AddSingleton<IFileSystemFactory, DotNetFileSystemFactory>()
.AddSingleton<ILockManager, InMemoryLockManager>()
.AddMvcCore()
.AddAuthorization()
.AddWebDav();
```

![screenshot](~/images/walk-through/09-after-replace.png)

## Explanation

The configuration of `DotNetFileSystemOptions` uses the `%TEMP%\webdav` folder as root folder. It also specifies that the unauthenticated user always gets the name `anonymous`.

This code also adds some required services:

* [IPropertyStoreFactory](xref:FubarDev.WebDavServer.Props.Store.IPropertyStoreFactory) is the interface used for getting an [IPropertyStore](xref:FubarDev.WebDavServer.Props.Store.IPropertyStore) for an [IFileSystem](xref:FubarDev.WebDavServer.FileSystem.IFileSystem).

* [IFileSystemFactory](xref:FubarDev.WebDavServer.FileSystem.IFileSystemFactory) is the unterface used for getting an [IFileSystem](xref:FubarDev.WebDavServer.FileSystem.IFileSystem) for the currently authenticated [IPrincipal](xref:System.Security.Principal.IPrincipal).

* [ILockManager](xref:FubarDev.WebDavServer.Locking.ILockManager) is used for the coordination of WebDAV locks.

# Add NTLM authentication

To allow `NTLM` (or `Negotiate`) authentication, the following steps must be done:

1. Add the `Microsoft.AspNetCore.Authentication` package

   ![screenshot](~/images/walk-through/08-add-auth-package.png)

2. Add the `[Authorize]` attribute to the `WebDavController`

   ![screenshot](~/images/walk-through/10-add-authorize-controller-attribute.png)

3. Replace in `Program.cs` the `.UseKestrel()` with the WebListener

    ```csharp
        .UseWebListener(opt =>
        {
            opt.ListenerSettings.Authentication.Schemes = AuthenticationSchemes.NTLM;
            opt.ListenerSettings.Authentication.AllowAnonymous = true;
        })
    ```

4. Remove the IIS integration

Now, your `Program.cs` file should like like this:

![screenshot](~/images/walk-through/10-remove-iis-integration.png)

# Disable the automatic browser launch at application start

To disable the automatic browser launch, you have to set the `launchBrowser` entry in the `launchSettings.json` file (can befound under `Properties`) to `false`.

Before:

![screenshot](~/images/walk-through/11-before-browser-disable.png)

After:

![screenshot](~/images/walk-through/11-after-browser-disable.png)

# Change the start project (**important!**)

You must change the start project from `IIS Express` to `TestWebDavServer` (your test project). Otherwise, using the `NTLM` authentication configured above, doesn't work.

![screenshot](~/images/walk-through/12-start-server.png)

# Running the test server

When you start the test server, you can see the WebDAV url in the console window:

![screenshot](~/images/walk-through/13-use-explorer-with-url.png)

Enter this URL into the Windows Explorers address bar and you should be able to connect to the server.
