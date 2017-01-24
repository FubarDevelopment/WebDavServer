using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Properties;

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
