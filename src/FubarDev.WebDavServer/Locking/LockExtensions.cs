// <copyright file="LockExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Diagnostics.Contracts;

namespace FubarDev.WebDavServer.Locking
{
    /// <summary>
    /// Extension methods for <see cref="ILock"/>.
    /// </summary>
    public static class LockExtensions
    {
        /// <summary>
        /// Determines whether the locks have the same owner.
        /// </summary>
        /// <param name="lock">The reference lock.</param>
        /// <param name="other">The lock's owner to compare to.</param>
        /// <returns><see langword="true"/> when the owners are the same.</returns>
        [Pure]
        public static bool IsSameOwner(this ILock @lock, ILock other)
        {
            var lockOwner = @lock.Owner;
            if (lockOwner is null)
            {
                return true;
            }

            var otherOwner = other.Owner;
            if (otherOwner is null)
            {
                return true;
            }

            return string.Equals(lockOwner, otherOwner, StringComparison.OrdinalIgnoreCase);
        }
    }
}
