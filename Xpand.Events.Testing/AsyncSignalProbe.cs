#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xpand.Events.Async;

namespace Xpand.Events.Testing {
    /// <summary>Thread-safe async payload probe that completes synchronously.</summary>
    public sealed class AsyncSignalProbe<T> {
        private readonly object _gate = new object();
        private readonly List<T> _values = new List<T>();
        private readonly AsyncSignalHandler<T> _handler;

        /// <summary>Creates a probe with a cached ValueTask handler.</summary>
        public AsyncSignalProbe() {
            _handler = RecordAsync;
        }

        /// <summary>Gets the cached handler to subscribe.</summary>
        public AsyncSignalHandler<T> Handler => _handler;

        /// <summary>Gets the observed invocation count.</summary>
        public int Count {
            get {
                lock (_gate) return _values.Count;
            }
        }

        /// <summary>Returns a stable copy of all payloads in observation order.</summary>
        public T[] Snapshot() {
            lock (_gate) return _values.ToArray();
        }

        /// <summary>Subscribes this probe and returns the signal's token.</summary>
        public IDisposable SubscribeTo(AsyncSignal<T> signal, int priority = 0) {
            if (signal == null) throw new ArgumentNullException(nameof(signal));
            return signal.Subscribe(_handler, priority);
        }

        /// <summary>Clears all observed payloads.</summary>
        public void Reset() {
            lock (_gate) _values.Clear();
        }

        private ValueTask RecordAsync(T payload, CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            lock (_gate) _values.Add(payload);
            return default;
        }
    }
}
