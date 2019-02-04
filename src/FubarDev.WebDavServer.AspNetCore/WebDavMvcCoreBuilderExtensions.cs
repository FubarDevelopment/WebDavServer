// <copyright file="WebDavMvcCoreBuilderExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.WebDavServer.AspNetCore
{
    /// <summary>
    /// Extensions for the <see cref="IMvcCoreBuilder"/>.
    /// </summary>
    public static class WebDavMvcCoreBuilderExtensions
    {
        /// <summary>
        /// Adds the WebDAV services that are essential to run a WebDAV server.
        /// </summary>
        /// <remarks>
        /// The user must still add the following services.
        /// <list type="bullet">
        /// <item>
        ///     <term><see cref="FileSystem.IFileSystemFactory"/></term>
        ///     <description>The file system factory</description>
        /// </item>
        /// <item>
        ///     <term><see cref="Props.Store.IPropertyStoreFactory"/></term>
        ///     <description>The property store factory</description>
        /// </item>
        /// <item>
        ///     <term><see cref="Locking.ILockManager"/></term>
        ///     <description>The lock manager</description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="builder">The <see cref="IMvcCoreBuilder"/></param>
        /// <returns>The <paramref name="builder"/></returns>
        public static IMvcCoreBuilder AddWebDav(this IMvcCoreBuilder builder)
        {
            builder.Services.AddWebDav();
            return builder;
        }
    }
}
