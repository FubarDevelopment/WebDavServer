// <copyright file="LockTokenHeader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model.Headers
{
    public class LockTokenHeader
    {
        public LockTokenHeader([NotNull] Uri stateToken)
        {
            StateToken = stateToken;
        }

        [NotNull]
        public Uri StateToken { get; }

        [NotNull]
        public static LockTokenHeader Parse(string s)
        {
            Uri stateToken;
            if (!CodedUrlParser.TryParse(s, out stateToken))
                throw new ArgumentException($"{s} is not a valid lock token", nameof(s));
            return new LockTokenHeader(stateToken);
        }
    }
}
