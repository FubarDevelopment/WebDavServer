# How to develop your own lock manager

A lock manager must implement the [ILockManager](xref:FubarDev.WebDavServer.Locking.ILockManager) interface, but for your
own sanity, you should derive your implementation from [LockManagerBase](xref:FubarDev.WebDavServer.Locking.LockManagerBase).

The [LockManagerBase](xref:FubarDev.WebDavServer.Locking.LockManagerBase) class
already implements the core functionality and uses a notion of a transaction
that must be used to store and retrieve the active locks.

For this guide, we implement a lock manager that stores the active locks in files in a special directory.

# Create a new class

First, you have to create a new class derived from [LockManagerBase](xref:FubarDev.WebDavServer.Locking.LockManagerBase). It needs to implement the
`BeginTransactionAsync` method.

We don't have a database that supports transactions, so we have to use a semaphore
to restrict concurrent access to the file system.

# Synchronization

For the synchronization, we use a [SemaphoreSlim](xref:System.Threading.SemaphoreSlim) and create it with an initial count of 1. In the `BeginTransactionAsync` method, we first have to call [SemaphoreSlim.WaitAsync](xref:System.Threading.SemaphoreSlim.WaitAsync) and then we have to call `Release` on the semaphore in the `Dispose` method of our `ILockManagerTransaction` implementation.

# Implement the tranaction interface

Now we need to implement the transaction interface [LockManagerBase.ILockManagerTransaction](xref:FubarDev.WebDavServer.Locking.LockManagerBase.ILockManagerTransaction).
