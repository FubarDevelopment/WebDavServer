﻿// Copyright (c) Barry Dorrans. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication;

using idunno.Authentication;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Contains the options used by the BasicAuthenticationMiddleware
    /// </summary>
    /// <summary>
    /// Contains the options used by the BasicAuthenticationMiddleware
    /// </summary>
    public class BasicAuthenticationOptions : AuthenticationSchemeOptions
    {
        private string _realm;

        /// <summary>
        /// Gets or sets the Realm sent in the WWW-Authenticate header.
        /// </summary>
        /// <remarks>
        /// The realm value (case-sensitive), in combination with the canonical root URL
        /// of the server being accessed, defines the protection space.
        /// These realms allow the protected resources on a server to be partitioned into a
        /// set of protection spaces, each with its own authentication scheme and/or
        /// authorization database.
        /// </remarks>
        public string Realm
        {
            get
            {
                return _realm;
            }

            set
            {
                if (!string.IsNullOrEmpty(value) && !IsAscii(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(Realm), "Realm must be US ASCII");
                }

                _realm = value;
            }
        }

        /// <summary>
        /// Gets or sets a flag indicating if the handler will prompt for authentication on HTTP requests.
        /// </summary>
        /// <remarks>
        /// If you set this to true you're a horrible person.
        /// </remarks>
        public bool AllowInsecureProtocol
        {
            get; set;
        }

        /// <summary>
        /// The object provided by the application to process events raised by the basic authentication middleware.
        /// The application may implement the interface fully, or it may create an instance of BasicAuthenticationEvents
        /// and assign delegates only to the events it wants to process.
        /// </summary>
        public new BasicAuthenticationEvents Events { get; set; } = new BasicAuthenticationEvents();


        private bool IsAscii(string input)
        {
            foreach (char c in input)
            {
                if (c < 32 || c >= 127)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
