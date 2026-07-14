#nullable enable

using System;
using Xpand.Events.Signals.Internal;

namespace Xpand.Events {
    /// <summary>
    /// Represents a synchronous, thread-safe signal with a single payload.
    /// </summary>
    /// <typeparam name="T">The payload type.</typeparam>
    public sealed class Signal<T> {
        private readonly SignalOptions _options;
        private readonly SubscriptionStore<Action<T>> _subscriptions = new SubscriptionStore<Action<T>>();

        /// <summary>
        /// Initializes a signal with fail-fast exception behavior.
        /// </summary>
        public Signal() : this(SignalOptions.FailFast) {
        }

        /// <summary>
        /// Initializes a signal with the specified immutable options.
        /// </summary>
        /// <param name="options">Per-instance exception behavior.</param>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
        public Signal(SignalOptions options) {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Gets the current number of registrations.
        /// </summary>
        public int Count => _subscriptions.Count;

        /// <summary>
        /// Gets a value indicating whether publishing is currently suspended.
        /// </summary>
        public bool IsSuspended => _subscriptions.IsSuspended;

        /// <summary>
        /// Registers a listener and returns a token that removes only this registration.
        /// </summary>
        /// <param name="handler">The listener to invoke.</param>
        /// <param name="priority">The invocation priority. Higher values run first.</param>
        /// <returns>An idempotent subscription token.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/>.</exception>
        public IDisposable Subscribe(Action<T> handler, int priority = 0) {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            return _subscriptions.Subscribe(handler, priority);
        }

        /// <summary>
        /// Publishes a payload synchronously on the calling thread using a stable listener snapshot.
        /// </summary>
        /// <param name="payload">The payload delivered to every listener.</param>
        public void Publish(T payload) {
            if (_subscriptions.IsSuspended) return;

            SubscriptionEntry<Action<T>>[] snapshot = _subscriptions.GetSnapshot();
            if (!_options.ReportAndContinueEnabled) {
                for (int i = 0; i < snapshot.Length; i++) {
                    snapshot[i].Handler(payload);
                }

                return;
            }

            for (int i = 0; i < snapshot.Length; i++) {
                try {
                    snapshot[i].Handler(payload);
                }
                catch (Exception exception) {
                    SignalExceptionDispatcher.Report(_options, exception);
                }
            }
        }

        /// <summary>
        /// Removes every current registration. An in-progress publish keeps its existing snapshot.
        /// </summary>
        public void Clear() {
            _subscriptions.Clear();
        }

        /// <summary>
        /// Suspends new publishes until the returned idempotent scope is disposed.
        /// </summary>
        /// <returns>A nest-safe suspension scope.</returns>
        public IDisposable Suspend() {
            return _subscriptions.Suspend();
        }
    }
}
