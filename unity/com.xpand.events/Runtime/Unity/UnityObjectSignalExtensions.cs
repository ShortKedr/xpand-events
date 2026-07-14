#nullable enable

using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Xpand.Events.Unity {
    /// <summary>Subscriptions that stop invoking after an explicitly owned Unity object is destroyed.</summary>
    public static class UnityObjectSignalExtensions {
        /// <summary>
        /// Subscribes while <paramref name="owner"/> is alive. The registration removes
        /// itself on the first publish observed after Unity destroys the owner.
        /// </summary>
        public static IDisposable SubscribeUnity(this Signal signal, Object owner, Action handler, int priority = 0) {
            if (signal == null) throw new ArgumentNullException(nameof(signal));
            if (ReferenceEquals(owner, null)) throw new ArgumentNullException(nameof(owner));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (owner == null) return EmptyDisposable.Instance;

            var guardedHandler = new UnityObjectHandler(owner, handler);
            IDisposable subscription = signal.Subscribe(guardedHandler.Invoke, priority);
            guardedHandler.Attach(subscription);
            return subscription;
        }

        /// <inheritdoc cref="SubscribeUnity(Signal,Object,Action,int)"/>
        public static IDisposable SubscribeUnity<T>(this Signal<T> signal, Object owner, Action<T> handler, int priority = 0) {
            if (signal == null) throw new ArgumentNullException(nameof(signal));
            if (ReferenceEquals(owner, null)) throw new ArgumentNullException(nameof(owner));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (owner == null) return EmptyDisposable.Instance;

            var guardedHandler = new UnityObjectHandler<T>(owner, handler);
            IDisposable subscription = signal.Subscribe(guardedHandler.Invoke, priority);
            guardedHandler.Attach(subscription);
            return subscription;
        }

        private sealed class UnityObjectHandler {
            private readonly object _gate = new object();
            private readonly Object _owner;
            private readonly Action _handler;
            private IDisposable? _subscription;
            private bool _ownerDestroyed;

            internal UnityObjectHandler(Object owner, Action handler) {
                _owner = owner;
                _handler = handler;
            }

            internal void Attach(IDisposable subscription) {
                bool dispose;
                lock (_gate) {
                    dispose = _ownerDestroyed;
                    if (!dispose) _subscription = subscription;
                }

                if (dispose) subscription.Dispose();
            }

            internal void Invoke() {
                if (_owner != null) {
                    _handler();
                    return;
                }

                RemoveDestroyedOwner();
            }

            private void RemoveDestroyedOwner() {
                IDisposable? subscription;
                lock (_gate) {
                    _ownerDestroyed = true;
                    subscription = _subscription;
                    _subscription = null;
                }

                subscription?.Dispose();
            }
        }

        private sealed class UnityObjectHandler<T> {
            private readonly object _gate = new object();
            private readonly Object _owner;
            private readonly Action<T> _handler;
            private IDisposable? _subscription;
            private bool _ownerDestroyed;

            internal UnityObjectHandler(Object owner, Action<T> handler) {
                _owner = owner;
                _handler = handler;
            }

            internal void Attach(IDisposable subscription) {
                bool dispose;
                lock (_gate) {
                    dispose = _ownerDestroyed;
                    if (!dispose) _subscription = subscription;
                }

                if (dispose) subscription.Dispose();
            }

            internal void Invoke(T payload) {
                if (_owner != null) {
                    _handler(payload);
                    return;
                }

                RemoveDestroyedOwner();
            }

            private void RemoveDestroyedOwner() {
                IDisposable? subscription;
                lock (_gate) {
                    _ownerDestroyed = true;
                    subscription = _subscription;
                    _subscription = null;
                }

                subscription?.Dispose();
            }
        }

        private sealed class EmptyDisposable : IDisposable {
            internal static readonly EmptyDisposable Instance = new EmptyDisposable();
            public void Dispose() { }
        }
    }
}
