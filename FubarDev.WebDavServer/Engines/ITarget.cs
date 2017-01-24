using System;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines
{
    public interface ITarget
    {
        [NotNull]
        string Name { get; }

        [NotNull]
        Uri DestinationUrl { get; }
    }
}
