// <copyright file="AsyncLazy.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer
{
    /// <summary>
    /// Provides support for asynchronous lazy initialization. This type is fully threadsafe.
    /// </summary>
    /// <typeparam name="T">The type of object that is being asynchronously initialized.</typeparam>
    [DebuggerDisplay("State = {" + nameof(GetStateForDebugger) + "}")]
    [DebuggerTypeProxy(typeof(AsyncLazy<>.DebugView))]
    public sealed class AsyncLazy<T>
    {
        /// <summary>
        /// The synchronization object protecting <c>_instance</c>.
        /// </summary>
        [NotNull]
        private readonly object _mutex = new object();

        /// <summary>
        /// The underlying lazy task.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        private readonly Lazy<Task<T>> _instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLazy&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="factory">The asynchronous delegate that is invoked to produce the value when it is needed. May not be <c>null</c>.</param>
        public AsyncLazy([NotNull] Func<Task<T>> factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            var factoryFunc = RunOnThreadPool(factory);
            _instance = new Lazy<Task<T>>(factoryFunc);
        }

        /// <summary>
        /// The current status of the <see cref="Task"/> of this <see cref="AsyncLazy{T}"/>
        /// </summary>
        internal enum LazyState
        {
            /// <summary>
            /// The underlying task wasn't started yet
            /// </summary>
            NotStarted,

            /// <summary>
            /// The underlying task is still executing
            /// </summary>
            Executing,

            /// <summary>
            /// The underlying task is finished
            /// </summary>
            Completed,
        }

        /// <summary>
        /// Gets the resulting task.
        /// </summary>
        /// <remarks>
        /// Starts the asynchronous factory method, if it has not already started.
        /// </remarks>
        [NotNull]
        public Task<T> Task
        {
            get
            {
                lock (_mutex)
                {
                    return _instance.Value;
                }
            }
        }

        [DebuggerNonUserCode]
        internal LazyState GetStateForDebugger
        {
            get
            {
                if (!_instance.IsValueCreated)
                {
                    return LazyState.NotStarted;
                }

                if (!_instance.Value.IsCompleted)
                {
                    return LazyState.Executing;
                }

                return LazyState.Completed;
            }
        }

        /// <summary>
        /// Asynchronous infrastructure support. This method permits instances of <see cref="AsyncLazy&lt;T&gt;"/> to be await'ed.
        /// </summary>
        /// <returns>the task awaiter</returns>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public TaskAwaiter<T> GetAwaiter()
        {
            return Task.GetAwaiter();
        }

        /// <summary>
        /// Asynchronous infrastructure support. This method permits instances of <see cref="AsyncLazy&lt;T&gt;"/> to be await'ed.
        /// </summary>
        /// <param name="continueOnCapturedContext">Continue on captured context?</param>
        /// <returns>The configured task awaiter</returns>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public ConfiguredTaskAwaitable<T> ConfigureAwait(bool continueOnCapturedContext)
        {
            return Task.ConfigureAwait(continueOnCapturedContext);
        }

        private Func<Task<T>> RunOnThreadPool(Func<Task<T>> factory)
        {
            return () => System.Threading.Tasks.Task.Run(factory);
        }

        [DebuggerNonUserCode]
        internal sealed class DebugView
        {
            [NotNull]
            private readonly AsyncLazy<T> _lazy;

            public DebugView([NotNull] AsyncLazy<T> lazy)
            {
                _lazy = lazy;
            }

            public LazyState State => _lazy.GetStateForDebugger;

            public Task Task
            {
                get
                {
                    if (!_lazy._instance.IsValueCreated)
                    {
                        throw new InvalidOperationException("Not yet created.");
                    }

                    return _lazy._instance.Value;
                }
            }

            public T Value
            {
                get
                {
                    if (!_lazy._instance.IsValueCreated || !_lazy._instance.Value.IsCompleted)
                    {
                        throw new InvalidOperationException("Not yet created.");
                    }

                    return _lazy._instance.Value.Result;
                }
            }
        }
    }
}
