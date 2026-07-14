#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Xpand.Events.Signals.Internal;

namespace Xpand.Events.Async {
    /// <summary>Handles one asynchronous signal payload.</summary>
    public delegate ValueTask AsyncSignalHandler<in T>(T payload, CancellationToken cancellationToken);

    /// <summary>
    /// A thread-safe asynchronous signal that awaits handlers sequentially in
    /// stable priority order.
    /// </summary>
    public sealed class AsyncSignal<T> {
        private readonly AsyncSignalOptions _options;
        private readonly SubscriptionStore<AsyncSignalHandler<T>> _subscriptions = new SubscriptionStore<AsyncSignalHandler<T>>();

        /// <summary>Initializes a signal with fail-fast exception behavior.</summary>
        public AsyncSignal() : this(AsyncSignalOptions.FailFast) {
        }

        /// <summary>Initializes a signal with immutable per-instance options.</summary>
        public AsyncSignal(AsyncSignalOptions options) {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>Gets the current registration count.</summary>
        public int Count => _subscriptions.Count;

        /// <summary>Gets whether publishing is currently suspended.</summary>
        public bool IsSuspended => _subscriptions.IsSuspended;

        /// <summary>Registers a handler and returns an idempotent token for this registration.</summary>
        public IDisposable Subscribe(AsyncSignalHandler<T> handler, int priority = 0) {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            return _subscriptions.Subscribe(handler, priority);
        }

        /// <summary>
        /// Publishes a payload and awaits handlers one at a time. Higher priority
        /// runs first; equal priority keeps subscription order.
        /// </summary>
        public async ValueTask PublishAsync(T payload, CancellationToken cancellationToken = default) {
            if (_subscriptions.IsSuspended) return;

            SubscriptionEntry<AsyncSignalHandler<T>>[] snapshot = _subscriptions.GetSnapshot();
            if (!_options.ReportAndContinueEnabled) {
                for (int i = 0; i < snapshot.Length; i++) {
                    cancellationToken.ThrowIfCancellationRequested();
                    await snapshot[i].Handler(payload, cancellationToken).ConfigureAwait(false);
                }

                return;
            }

            for (int i = 0; i < snapshot.Length; i++) {
                cancellationToken.ThrowIfCancellationRequested();
                try {
                    await snapshot[i].Handler(payload, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
                    throw;
                }
                catch (Exception handlerException) {
                    try {
                        await _options.ExceptionSink!(handlerException, cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
                        throw;
                    }
                    catch (Exception sinkException) {
                        throw new AggregateException(
                            "An async signal handler failed and the configured exception sink also failed.",
                            handlerException,
                            sinkException);
                    }
                }
            }
        }

        /// <summary>Removes all current registrations without changing an in-progress snapshot.</summary>
        public void Clear() {
            _subscriptions.Clear();
        }

        /// <summary>Suspends new publishes until the returned nest-safe scope is disposed.</summary>
        public IDisposable Suspend() {
            return _subscriptions.Suspend();
        }
    }
}
