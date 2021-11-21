# WebDAV-Server

[![Build Status](https://dev.azure.com/fubar-development/webdav-server/_apis/build/status/FubarDevelopment.WebDavServer?branchName=release%2F2.0)](https://dev.azure.com/fubar-development/webdav-server/_build/latest?definitionId=1&branchName=release%2F2.0)

This is a WebDAV server that provides an integration for ASP.NET Core. It's main goal is to be easily usable
and extensible. The extensibility is provided by making extension use of dependency injection and covers
the following points:

- HttpClient instantiation (for server-to-server COPY/MOVE)
- Virtual file system
- Property store (for dead properties)
- WebDAV implementation (down to the HTTP verb level)

# Documentation

A documentation is available via [GitHub Pages](https://fubardevelopment.github.io/WebDavServer/).

# Build instructions

You need the following packages:

- .NET Core SDK 3.1
- .NET SDK 6.0

The reason why both are required is that
the WebDAV server is built with .NET 6.0
while targeting .NET Core 3.1.

# Support the development

[![Patreon](https://img.shields.io/endpoint.svg?url=https:%2F%2Fshieldsio-patreon.herokuapp.com%2FFubarDevelopment&style=for-the-badge)](https://www.patreon.com/FubarDevelopment)
