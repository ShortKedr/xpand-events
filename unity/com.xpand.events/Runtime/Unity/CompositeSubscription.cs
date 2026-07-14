#nullable enable

using System;
using System.Collections.Generic;

namespace Xpand.Events.Unity {
    /// <summary>
    /// Owns a group of subscriptions and disposes every item even when one item fails.
    /// </summary>
    public sealed class CompositeSubscription : IDisposable {
        private readonly object _gate = new object();
        private List<IDisposable> _subscriptions = new List<IDisposable>();
        private bool _isDisposed;

        /// <summary>Gets the number of subscriptions currently owned by this instance.</summary>
        public int Count {
            get {
                lock (_gate) return _subscriptions.Count;
            }
        }

        /// <summary>Gets whether this instance has been permanently disposed.</summary>
        public bool IsDisposed {
            get {
                lock (_gate) return _isDisposed;
            }
        }

        /// <summary>
        /// Adds a subscription. If this composite is already disposed, the subscription
        /// is disposed immediately.
        /// </summary>
        public IDisposable Add(IDisposable subscription) {
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));

            lock (_gate) {
                if (!_isDisposed) {
                    _subscriptions.Add(subscription);
                    return subscription;
                }
            }

            subscription.Dispose();
            return subscription;
        }

        /// <summary>
        /// Disposes and removes current items while keeping the composite reusable.
        /// </summary>
        public void Clear() {
            List<IDisposable> subscriptions;
            lock (_gate) {
                if (_subscriptions.Count == 0) return;
                subscriptions = _subscriptions;
                _subscriptions = new List<IDisposable>();
            }

            DisposeAll(subscriptions);
        }

        /// <summary>
        /// Permanently disposes this composite and all current subscriptions.
        /// </summary>
        public void Dispose() {
            List<IDisposable> subscriptions;
            lock (_gate) {
                if (_isDisposed) return;
                _isDisposed = true;
                subscriptions = _subscriptions;
                _subscriptions = new List<IDisposable>();
            }

            DisposeAll(subscriptions);
        }

        private static void DisposeAll(List<IDisposable> subscriptions) {
            List<Exception>? failures = null;
            for (int i = subscriptions.Count - 1; i >= 0; i--) {
                try {
                    subscriptions[i].Dispose();
                }
                catch (Exception exception) {
                    if (failures == null) failures = new List<Exception>();
                    failures.Add(exception);
                }
            }

            if (failures != null) throw new AggregateException("One or more subscriptions failed during disposal.", failures);
        }
    }

    /// <summary>Fluent helpers for assigning subscription ownership.</summary>
    public static class CompositeSubscriptionExtensions {
        /// <summary>Adds a subscription to a composite and returns the original token.</summary>
        public static IDisposable AddTo(this IDisposable subscription, CompositeSubscription composite) {
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));
            if (composite == null) throw new ArgumentNullException(nameof(composite));
            return composite.Add(subscription);
        }
    }
}
