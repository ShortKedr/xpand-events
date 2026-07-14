#nullable enable

using System;

namespace Xpand.Events.Extensions.Microsoft {
    /// <summary>Explicit synchronous observable adapters.</summary>
    public static class SignalObservableExtensions {
        /// <summary>
        /// Exposes signal payloads through <see cref="IObservable{T}"/>. The signal
        /// does not have a completion concept, so observers receive only OnNext.
        /// </summary>
        public static IObservable<T> AsObservable<T>(this Signal<T> signal, int priority = 0) {
            if (signal == null) throw new ArgumentNullException(nameof(signal));
            return new SignalObservable<T>(signal, priority);
        }

        private sealed class SignalObservable<T> : IObservable<T> {
            private readonly Signal<T> _signal;
            private readonly int _priority;

            internal SignalObservable(Signal<T> signal, int priority) {
                _signal = signal;
                _priority = priority;
            }

            public IDisposable Subscribe(IObserver<T> observer) {
                if (observer == null) throw new ArgumentNullException(nameof(observer));
                return _signal.Subscribe(observer.OnNext, _priority);
            }
        }
    }
}
