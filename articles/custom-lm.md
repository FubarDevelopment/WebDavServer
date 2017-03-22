# How to develop your own lock manager

A lock manager must implement the [ILockManager](xref:FubarDev.WebDavServer.Locking.ILockManager) interface, but for your
own sanity, you should derive your implementation from [LockManagerBase](xref:FubarDev.WebDavServer.Locking.LockManagerBase).

The [LockManagerBase](xref:FubarDev.WebDavServer.Locking.LockManagerBase) class
already implements the core functionality and uses a notion of a transaction
that must be used to store and retrieve the active locks. No lock can be modified
until the transaction was either committed or rolled back (by disposing without
committing).

For this guide, we implement a lock manager that stores the active locks in files in a special directory.

You can find the repository for this [project on GitHub](https://github.com/FubarDevelopment/WebDavServer-TextFileLockManager).

# Create a new class

First, you have to create a new class derived from [LockManagerBase](xref:FubarDev.WebDavServer.Locking.LockManagerBase). It needs to implement the
`BeginTransactionAsync` method.

We don't have a database that supports transactions, so we have to use a semaphore
to restrict concurrent access to the file system. Another solution might be using
a lock file, which is overkill for this example.

# Storage

The file used to store the locks should be configured using an options class. This class should implement the
[ILockManagerOptions](xref:FubarDev.WebDavServer.Locking.ILockManagerOptions) interface to avoid multiple
options classes for the lock manager. A sufficient way to initialize the [Rounding](xref:FubarDev.WebDavServer.Locking.ILockManagerOptions.Rounding)
property is to create a new instance of [DefaultLockTimeRounding](xref:FubarDev.WebDavServer.Locking.DefaultLockTimeRounding) with
[DefaultLockTimeRoundingMode.OneSecond](xref:FubarDev.WebDavServer.Locking.DefaultLockTimeRoundingMode.OneSecond) as constructor
parameter.

# Constructor

The constructor should take a parameter of type `IOptions<TextFileLockManagerOptions>` and we pass its `Value` to the base
class.

# Synchronization

For the synchronization, we use a [SemaphoreSlim](xref:System.Threading.SemaphoreSlim) and create it with an initial count of 1. In the `BeginTransactionAsync` method, we first have to call [SemaphoreSlim.WaitAsync](xref:System.Threading.SemaphoreSlim.WaitAsync) and then we have to call `Release` on the semaphore in the `Dispose` method of our `ILockManagerTransaction` implementation. For a cluster of WebDAV servers using the
same lock file, one may use a lock file to synchronize access.

# File format

We just use a simple JSON file because we usually don't have many active locks and reading and writing a whole
file doesn't cause a huge performance penalty.

## Structure

The structure of the file is just a list of objects that implement the [IActiveLock](xref:FubarDev.WebDavServer.Locking.IActiveLock) interface,
but it also has to implement every property with a setter and an additional `Owner` property.

## Load locks after restart

We have to load all active locks when we first open a transaction. Those locks must be passed to the lock cleanup task to
ensure that the locks will be released when they expire.

# Implement the transaction interface

Now we need to implement the transaction interface [LockManagerBase.ILockManagerTransaction](xref:FubarDev.WebDavServer.Locking.LockManagerBase.ILockManagerTransaction).

In the new transaction class, we load the JSON file during construction and save the JSON file in the [ILockManagerTransaction.CommitAsync](xref:FubarDev.WebDavServer.Locking.LockManagerBase.ILockManagerTransaction.CommitAsync(System.Threading.CancellationToken)) method.

## Transaction interface methods

The transaction interface consists of the following parts:

* [ILockManagerTransaction.GetActiveLocksAsync](xref:FubarDev.WebDavServer.Locking.LockManagerBase.ILockManagerTransaction.GetActiveLocksAsync(System.Threading.CancellationToken))

  This function is used to get all active locks. We just cast the values of the dicitonary to
  an [IActiveLock](xref:FubarDev.WebDavServer.Locking.IActiveLock) and return those as a list.

* [ILockManagerTransaction.AddAsync](xref:FubarDev.WebDavServer.Locking.LockManagerBase.ILockManagerTransaction.AddAsync(FubarDev.WebDavServer.Locking.IActiveLock,System.Threading.CancellationToken))

  Adds a new active lock. We're just adding the lock to the dictionary.

* [ILockManagerTransaction.UpdateAsync](xref:FubarDev.WebDavServer.Locking.LockManagerBase.ILockManagerTransaction.UpdateAsync(FubarDev.WebDavServer.Locking.IActiveLock,System.Threading.CancellationToken))

  Updates a new active lock.

* [ILockManagerTransaction.RemoveAsync](xref:FubarDev.WebDavServer.Locking.LockManagerBase.ILockManagerTransaction.RemoveAsync(System.String,System.Threading.CancellationToken))

  Removes an active lock.

* [ILockManagerTransaction.GetAsync](xref:FubarDev.WebDavServer.Locking.LockManagerBase.ILockManagerTransaction.GetAsync(System.String,System.Threading.CancellationToken))

  Gets an active lock by its state token.

* [ILockManagerTransaction.CommitAsync](xref:FubarDev.WebDavServer.Locking.LockManagerBase.ILockManagerTransaction.CommitAsync(System.Threading.CancellationToken))

  Commits all changes made during the transaction. In our implementation, we'll just save the locks as JSON file.

## GetActiveLocksAsync

This function just returns every row in the table holding the active locks.

## IDisposable implementation

This interface also inherits from [IDisposable](xref:System.IDisposable). It depends on the state of the transaction
what happens when the `Dispose` function is called:

* `CommitAsync` called before `Dispose` results in a disposable of the resources.
* `Dispose` **without** a `CommitAsync` results in a rollback and a disposable of the resources.

# Summary

The easiest way to implement a lock manager is to use a database that already supports transactions, but almost everything
can be used to store the transactions. The most important thing is the synchronized access to the locks. It is also
very important, that the `BeginTransaction` method blocks the caller until it's safe to update the locks.

For databases like MongoDB, a [two-phase commit](https://docs.mongodb.com/manual/tutorial/perform-two-phase-commits/) is encouraged.
