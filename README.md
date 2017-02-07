# WebDAV-Server

This is a WebDAV server that provides an integration for ASP.NET Core. It's main goal is to be easily usable
and extensible. The extensibility is provided by making extension use of dependency injection and covers
the following points:

- HttpClient instantiation (for server-to-server COPY/MOVE)
- Virtual file system
- Property store (for dead properties)
- WebDAV implementation (down to the HTTP verb level)
