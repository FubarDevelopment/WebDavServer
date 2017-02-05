// <copyright file="DocumentExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Tests.Support
{
    public static class DocumentExtensions
    {
        public static async Task FillWithAsync(this IDocument document, string text, CancellationToken ct)
        {
            using (var stream = await document.CreateAsync(ct).ConfigureAwait(false))
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(text);
                }
            }
        }

        public static async Task<string> ReadAllAsync(this IDocument document, CancellationToken ct)
        {
            using (var stream = await document.OpenReadAsync(ct).ConfigureAwait(false))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
