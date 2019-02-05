---
uid: configuration
title: Configuration
---

# Configuration

Several parts of this library are configurable using `Microsoft.Extensions.Options`.

## PUT buffer management

The default implementation [ArrayPoolBufferPoolFactory](xref:FubarDev.WebDavServer.BufferPools.ArrayPoolBufferPoolFactory) of the PUT handler is optimized for throughput which might cause problems in a high load environment. It can be replaced with the following snippet:

```cs
services
    .AddSingleton<IBufferPoolFactory, FixedSizeBufferPoolFactory>();
```

This factory can be configured using:

```cs
services
    .Configure<FixedSizeBufferPoolOptions>(opt => opt.Size = 123456);
```
