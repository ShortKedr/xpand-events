#nullable enable

using System;
using System.Collections.Generic;

namespace Xpand.Events.Testing {
    /// <summary>Thread-safe named invocation log for asserting cross-handler ordering.</summary>
    public sealed class InvocationLog {
        private readonly object _gate = new object();
        private readonly List<string> _entries = new List<string>();

        /// <summary>Creates a handler that appends <paramref name="name"/> on every call.</summary>
        public Action Handler(string name) {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return () => Add(name);
        }

        /// <summary>Creates a payload handler that appends a formatted entry.</summary>
        public Action<T> Handler<T>(Func<T, string> formatter) {
            if (formatter == null) throw new ArgumentNullException(nameof(formatter));
            return payload => Add(formatter(payload));
        }

        /// <summary>Returns a stable copy of entries in observation order.</summary>
        public string[] Snapshot() {
            lock (_gate) return _entries.ToArray();
        }

        /// <summary>Clears all entries.</summary>
        public void Reset() {
            lock (_gate) _entries.Clear();
        }

        private void Add(string entry) {
            lock (_gate) _entries.Add(entry);
        }
    }
}
