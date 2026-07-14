#nullable enable

using System;
using System.Threading;

namespace Xpand.Events.Signals.Internal {
    internal readonly struct SubscriptionEntry<THandler> where THandler : class {
        internal SubscriptionEntry(THandler handler, int priority, long sequence) {
            Handler = handler;
            Priority = priority;
            Sequence = sequence;
        }

        internal THandler Handler { get; }

        internal int Priority { get; }

        internal long Sequence { get; }
    }

    internal sealed class SubscriptionStore<THandler> where THandler : class {
        private readonly object _gate = new object();
        private SubscriptionEntry<THandler>[] _snapshot = Array.Empty<SubscriptionEntry<THandler>>();
        private long _nextSequence;
        private int _suspensionCount;

        internal int Count => Volatile.Read(ref _snapshot).Length;

        internal bool IsSuspended => Volatile.Read(ref _suspensionCount) != 0;

        internal IDisposable Subscribe(THandler handler, int priority) {
            lock (_gate) {
                SubscriptionEntry<THandler>[] current = _snapshot;
                int insertionIndex = 0;
                while (insertionIndex < current.Length && current[insertionIndex].Priority >= priority) {
                    insertionIndex++;
                }

                long sequence = _nextSequence;
                _nextSequence = unchecked(sequence + 1);
                SubscriptionEntry<THandler>[] updated = new SubscriptionEntry<THandler>[current.Length + 1];
                if (insertionIndex != 0) {
                    Array.Copy(current, 0, updated, 0, insertionIndex);
                }

                updated[insertionIndex] = new SubscriptionEntry<THandler>(handler, priority, sequence);
                if (insertionIndex != current.Length) {
                    Array.Copy(current, insertionIndex, updated, insertionIndex + 1, current.Length - insertionIndex);
                }

                SubscriptionToken token = new SubscriptionToken(this, sequence);
                Volatile.Write(ref _snapshot, updated);
                return token;
            }
        }

        internal SubscriptionEntry<THandler>[] GetSnapshot() {
            return Volatile.Read(ref _snapshot);
        }

        internal void Clear() {
            lock (_gate) {
                Volatile.Write(ref _snapshot, Array.Empty<SubscriptionEntry<THandler>>());
            }
        }

        internal IDisposable Suspend() {
            SuspensionToken token = new SuspensionToken(this);
            Interlocked.Increment(ref _suspensionCount);
            return token;
        }

        private void Unsubscribe(long sequence) {
            lock (_gate) {
                SubscriptionEntry<THandler>[] current = _snapshot;
                int removalIndex = -1;
                for (int i = 0; i < current.Length; i++) {
                    if (current[i].Sequence == sequence) {
                        removalIndex = i;
                        break;
                    }
                }

                if (removalIndex < 0) return;

                if (current.Length == 1) {
                    Volatile.Write(ref _snapshot, Array.Empty<SubscriptionEntry<THandler>>());
                    return;
                }

                SubscriptionEntry<THandler>[] updated = new SubscriptionEntry<THandler>[current.Length - 1];
                if (removalIndex != 0) {
                    Array.Copy(current, 0, updated, 0, removalIndex);
                }

                if (removalIndex != updated.Length) {
                    Array.Copy(current, removalIndex + 1, updated, removalIndex, updated.Length - removalIndex);
                }

                Volatile.Write(ref _snapshot, updated);
            }
        }

        private void EndSuspension() {
            Interlocked.Decrement(ref _suspensionCount);
        }

        private sealed class SubscriptionToken : IDisposable {
            private SubscriptionStore<THandler>? _owner;
            private readonly long _sequence;

            internal SubscriptionToken(SubscriptionStore<THandler> owner, long sequence) {
                _owner = owner;
                _sequence = sequence;
            }

            public void Dispose() {
                SubscriptionStore<THandler>? owner = Interlocked.Exchange(ref _owner, null);
                if (owner != null) owner.Unsubscribe(_sequence);
            }
        }

        private sealed class SuspensionToken : IDisposable {
            private SubscriptionStore<THandler>? _owner;

            internal SuspensionToken(SubscriptionStore<THandler> owner) {
                _owner = owner;
            }

            public void Dispose() {
                SubscriptionStore<THandler>? owner = Interlocked.Exchange(ref _owner, null);
                if (owner != null) owner.EndSuspension();
            }
        }
    }
}
