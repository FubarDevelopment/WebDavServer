// <copyright file="Status.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Net;

namespace FubarDev.WebDavServer.Model
{
    /// <summary>
    /// The WebDAV status line.
    /// </summary>
    public struct Status
    {
        private static readonly char[] _splitChars = { ' ', '\t' };

        /// <summary>
        /// Initializes a new instance of the <see cref="Status"/> struct.
        /// </summary>
        /// <param name="protocol">The HTTP protocol (usually <c>HTTP/1.1</c>).</param>
        /// <param name="statusCode">The WebDAV status code.</param>
        /// <param name="reasonPhrase">The status reason phrase.</param>
        public Status(string protocol, HttpStatusCode statusCode, string? reasonPhrase = null)
            : this(protocol, (int)statusCode, string.IsNullOrEmpty(reasonPhrase) ? GetReasonPhrase((int)statusCode) : reasonPhrase)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Status"/> struct.
        /// </summary>
        /// <param name="protocol">The HTTP protocol (usually <c>HTTP/1.1</c>).</param>
        /// <param name="statusCode">The WebDAV status code.</param>
        /// <param name="reasonPhrase">The status reason phrase.</param>
        public Status(string protocol, int statusCode, string reasonPhrase)
        {
            if (string.IsNullOrEmpty(protocol))
            {
                throw new ArgumentNullException(nameof(protocol));
            }

            if (string.IsNullOrEmpty(reasonPhrase))
            {
                throw new ArgumentNullException(nameof(reasonPhrase));
            }

            Protocol = protocol;
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Status"/> struct.
        /// </summary>
        /// <param name="protocol">The HTTP protocol (usually <c>HTTP/1.1</c>).</param>
        /// <param name="statusCode">The WebDAV status code.</param>
        /// <param name="additionalReasonPhrase">The additional text to the reason phrase.</param>
        public Status(string protocol, WebDavStatusCode statusCode, string? additionalReasonPhrase = null)
        {
            if (string.IsNullOrEmpty(protocol))
            {
                throw new ArgumentNullException(nameof(protocol));
            }

            Protocol = protocol;
            StatusCode = (int)statusCode;
            ReasonPhrase = statusCode.GetReasonPhrase(additionalReasonPhrase);
        }

        /// <summary>
        /// Gets the HTTP protocol (usually <c>HTTP/1.1</c>).
        /// </summary>
        public string Protocol { get; }

        /// <summary>
        /// Gets the status code.
        /// </summary>
        public int StatusCode { get; }

        /// <summary>
        /// Gets the <see cref="StatusCode"/>s reason phrase.
        /// </summary>
        public string ReasonPhrase { get; }

        /// <summary>
        /// Gets a value indicating whether the status code indicates success.
        /// </summary>
        public bool IsSuccessStatusCode => StatusCode >= 200 && StatusCode < 300;

        /// <summary>
        /// Parses the header value to get a new instance of the <see cref="Status"/> class.
        /// </summary>
        /// <param name="status">The header value to parse.</param>
        /// <returns>The new instance of the <see cref="Status"/> class.</returns>
        public static Status Parse(string status)
        {
            var parts = status.Split(_splitChars, 3, StringSplitOptions.RemoveEmptyEntries);
            var protocol = parts[0];
            var statusCode = Convert.ToInt32(parts[1], 10);
            var reasonPhrase = parts[2];

            if (string.IsNullOrEmpty(reasonPhrase))
            {
                reasonPhrase = GetReasonPhrase(statusCode);
            }

            return new Status(protocol, statusCode, reasonPhrase);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Protocol} {StatusCode} {ReasonPhrase}";
        }

        private static string GetReasonPhrase(int statusCode)
        {
            string reasonPhrase;
            var code = $"{statusCode}";
            WebDavStatusCode webDavStatusCode;
            if (!Enum.TryParse(code, out webDavStatusCode))
            {
                HttpStatusCode httpStatusCode;
                reasonPhrase = !Enum.TryParse(code, out httpStatusCode)
                    ? "Unknown status code"
                    : httpStatusCode.ToString();
            }
            else
            {
                reasonPhrase = webDavStatusCode.GetReasonPhrase();
            }

            return reasonPhrase;
        }
    }
}
