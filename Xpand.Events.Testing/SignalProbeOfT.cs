#nullable enable

using System;
using System.Collections.Generic;

namespace Xpand.Events.Testing {
    /// <summary>Thread-safe payload probe with deterministic snapshots.</summary>
    public sealed class SignalProbe<T> {
        private readonly object _gate = new object();
        private readonly List<T> _values = new List<T>();
        private readonly Action<T> _handler;

        /// <summary>Creates a probe with a cached handler delegate.</summary>
        public SignalProbe() {
            _handler = Record;
        }

        /// <summary>Gets the cached handler to subscribe.</summary>
        public Action<T> Handler => _handler;

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
        public IDisposable SubscribeTo(Signal<T> signal, int priority = 0) {
            if (signal == null) throw new ArgumentNullException(nameof(signal));
            return signal.Subscribe(_handler, priority);
        }

        /// <summary>Clears all observed payloads.</summary>
        public void Reset() {
            lock (_gate) _values.Clear();
        }

        private void Record(T payload) {
            lock (_gate) _values.Add(payload);
        }
    }
}
