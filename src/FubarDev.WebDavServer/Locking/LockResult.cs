// <copyright file="LockResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.WebDavServer.Locking;

/// <summary>
/// The result of a locking operation.
/// </summary>
public class LockResult
{
    private readonly IActiveLock? _lock;
    private readonly LockStatus? _conflictingLocks;

    /// <summary>
    /// Initializes a new instance of the <see cref="LockResult"/> class.
    /// </summary>
    /// <param name="activeLock">The active lock when locking succeeded.</param>
    public LockResult(IActiveLock activeLock)
    {
        _lock = activeLock;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LockResult"/> class.
    /// </summary>
    /// <param name="conflictingLocks">The collection of locks preventing locking the given destination.</param>
    public LockResult(LockStatus conflictingLocks)
    {
        if (conflictingLocks.Count == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(conflictingLocks), @"No conflicting locks found");
        }

        _conflictingLocks = conflictingLocks;
    }

    /// <summary>
    /// Gets a value indicating whether taking the lock was successful.
    /// </summary>
    public bool IsSuccess => _lock != null;

    /// <summary>
    /// Gets the active lock when locking succeeded.
    /// </summary>
    public IActiveLock Lock =>
        _lock ?? throw new InvalidOperationException("Lock operation was not successful");

    /// <summary>
    /// Gets the collection of locks preventing locking the given destination.
    /// </summary>
    public LockStatus ConflictingLocks => _conflictingLocks ?? LockStatus.Empty;
}
