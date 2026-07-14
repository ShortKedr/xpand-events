#nullable enable

using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

namespace Xpand.Events.Unity {
    /// <summary>
    /// Explicitly queues work for execution from this component's Unity Update loop.
    /// It does not alter Core signal dispatch semantics.
    /// </summary>
    public sealed class UnityMainThreadDispatcher : MonoBehaviour {
        private readonly ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();
        private int _mainThreadId;

        /// <summary>Gets whether the caller is the thread that initialized this component.</summary>
        public bool IsMainThread => Thread.CurrentThread.ManagedThreadId == Volatile.Read(ref _mainThreadId);

        /// <summary>Queues work from any managed thread.</summary>
        public void Post(Action action) {
            if (action == null) throw new ArgumentNullException(nameof(action));
            _queue.Enqueue(action);
        }

        /// <summary>
        /// Executes all work currently queued. Exceptions are fail-fast and remaining
        /// work stays queued for the next drain.
        /// </summary>
        public int Drain() {
            if (!IsMainThread) throw new InvalidOperationException("The dispatcher can only be drained on its Unity main thread.");

            int count = 0;
            while (_queue.TryDequeue(out Action? action)) {
                action();
                count++;
            }

            return count;
        }

        private void Awake() {
            Volatile.Write(ref _mainThreadId, Thread.CurrentThread.ManagedThreadId);
        }

        private void Update() {
            Drain();
        }
    }
}
