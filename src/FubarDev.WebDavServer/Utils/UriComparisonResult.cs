// <copyright file="UriComparisonResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.Utils
{
    /// <summary>
    /// The result of an URL comparison.
    /// </summary>
    public enum UriComparisonResult
    {
        /// <summary>
        /// The URL points to different host
        /// </summary>
        PrecedingDifferentHost = -4,

        /// <summary>
        /// The URL points to a path that is a preceding sibling of the target path
        /// </summary>
        PrecedingSibling,

        /// <summary>
        /// The URL points to a resource above
        /// </summary>
        Parent,

        /// <summary>
        /// Both URLs point to the same resource.
        /// </summary>
        Equal,

        /// <summary>
        /// The URL points to a resource below
        /// </summary>
        Child,

        /// <summary>
        /// The URL points to a path that is a following sibling of the target path
        /// </summary>
        FollowingSibling,

        /// <summary>
        /// The URL points to different host
        /// </summary>
        FollowingDifferentHost,
    }
}
