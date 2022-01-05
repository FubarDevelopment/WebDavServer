// <copyright file="LockTokenHeader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.Parsing;
using FubarDev.WebDavServer.Properties;

namespace FubarDev.WebDavServer.Models
{
    /// <summary>
    /// The <c>Lock-Token</c> header.
    /// </summary>
    public class LockTokenHeader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LockTokenHeader"/> class.
        /// </summary>
        /// <param name="stateToken">The lock token.</param>
        public LockTokenHeader(Uri stateToken)
        {
            StateToken = stateToken;
        }

        /// <summary>
        /// Gets the lock token.
        /// </summary>
        public Uri StateToken { get; }

        /// <summary>
        /// Parses the header string to get a new instance of the <see cref="LockTokenHeader"/> class.
        /// </summary>
        /// <param name="s">The header string to parse.</param>
        /// <returns>The new instance of the <see cref="LockTokenHeader"/> class.</returns>
        public static LockTokenHeader Parse(string s)
        {
            var lexer = new Lexer(s);
            var parser = new Parser(lexer);
            var result = parser.ParseCodedUrl();
            if (result.IsOk)
            {
                if (lexer.IsEnd || lexer.Next().Kind == TokenType.End)
                {
                    return new LockTokenHeader(result.Ok.Value);
                }
            }

            throw new ArgumentException(
                string.Format(Resources.InvalidLockTokenFormat, s),
                nameof(s));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"<{StateToken}>";
        }
    }
}
