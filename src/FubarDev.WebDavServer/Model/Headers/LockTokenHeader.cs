// <copyright file="LockTokenHeader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model.Headers
{
    /// <summary>
    /// The <code>Lock-Token</code> header
    /// </summary>
    public class LockTokenHeader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LockTokenHeader"/> class.
        /// </summary>
        /// <param name="stateToken">The lock token</param>
        public LockTokenHeader([NotNull] Uri stateToken)
        {
            StateToken = stateToken;
        }

        /// <summary>
        /// Gets the lock token
        /// </summary>
        [NotNull]
        public Uri StateToken { get; }

        /// <summary>
        /// Parses the header string to get a new instance of the <see cref="LockTokenHeader"/> class
        /// </summary>
        /// <param name="s">The header string to parse</param>
        /// <returns>The new instance of the <see cref="LockTokenHeader"/> class</returns>
        [NotNull]
        public static LockTokenHeader Parse(string s)
        {
            Uri stateToken;
            if (!CodedUrlParser.TryParse(s, out stateToken))
                throw new ArgumentException($"{s} is not a valid lock token", nameof(s));
            return new LockTokenHeader(stateToken);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"<{StateToken}>";
        }
    }
}
