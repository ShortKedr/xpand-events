#nullable enable

using System;
using System.Threading;

namespace Xpand.Events.Testing {
    /// <summary>Thread-safe no-payload handler probe.</summary>
    public sealed class SignalProbe {
        private readonly Action _handler;
        private int _count;

        /// <summary>Creates a probe with a cached handler delegate.</summary>
        public SignalProbe() {
            _handler = Record;
        }

        /// <summary>Gets the cached handler to subscribe.</summary>
        public Action Handler => _handler;

        /// <summary>Gets the observed invocation count.</summary>
        public int Count => Volatile.Read(ref _count);

        /// <summary>Subscribes this probe and returns the signal's token.</summary>
        public IDisposable SubscribeTo(Signal signal, int priority = 0) {
            if (signal == null) throw new ArgumentNullException(nameof(signal));
            return signal.Subscribe(_handler, priority);
        }

        /// <summary>Resets the count atomically.</summary>
        public void Reset() {
            Interlocked.Exchange(ref _count, 0);
        }

        private void Record() {
            Interlocked.Increment(ref _count);
        }
    }
}
