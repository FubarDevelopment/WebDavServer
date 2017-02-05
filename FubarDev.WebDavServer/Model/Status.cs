// <copyright file="Status.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Net;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model
{
    public struct Status
    {
        private static readonly char[] _splitChars = { ' ', '\t' };

        public Status([NotNull] string protocol, HttpStatusCode statusCode, [CanBeNull] string reasonPhrase = null)
            : this(protocol, (int)statusCode, string.IsNullOrEmpty(reasonPhrase) ? GetReasonPhrase((int)statusCode) : reasonPhrase)
        {
        }

        public Status([NotNull] string protocol, int statusCode, [NotNull] string reasonPhrase)
        {
            if (string.IsNullOrEmpty(protocol))
                throw new ArgumentNullException(nameof(protocol));
            if (string.IsNullOrEmpty(reasonPhrase))
                throw new ArgumentNullException(nameof(reasonPhrase));

            Protocol = protocol;
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase;
        }

        public Status([NotNull] string protocol, WebDavStatusCode statusCode, [CanBeNull] string additionalReasonPhrase = null)
        {
            if (string.IsNullOrEmpty(protocol))
                throw new ArgumentNullException(nameof(protocol));

            Protocol = protocol;
            StatusCode = (int)statusCode;
            ReasonPhrase = statusCode.GetReasonPhrase(additionalReasonPhrase);
        }

        [NotNull]
        public string Protocol { get; }

        public int StatusCode { get; }

        [NotNull]
        public string ReasonPhrase { get; }

        public bool IsSuccessStatusCode => StatusCode >= 200 && StatusCode < 300;

        public static Status Parse(string status)
        {
            var parts = status.Split(_splitChars, 3, StringSplitOptions.RemoveEmptyEntries);
            var protocol = parts[0];
            var statusCode = Convert.ToInt32(parts[1], 10);
            var reasonPhrase = parts[2];

            if (string.IsNullOrEmpty(reasonPhrase))
                reasonPhrase = GetReasonPhrase(statusCode);

            return new Status(protocol, statusCode, reasonPhrase);
        }

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
                if (!Enum.TryParse(code, out httpStatusCode))
                {
                    reasonPhrase = "Unknown status code";
                }
                else
                {
                    reasonPhrase = httpStatusCode.ToString();
                }
            }
            else
            {
                reasonPhrase = webDavStatusCode.GetReasonPhrase();
            }

            return reasonPhrase;
        }
    }
}
