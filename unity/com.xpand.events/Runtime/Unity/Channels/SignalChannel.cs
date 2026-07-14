#nullable enable

using System;
using UnityEngine;

namespace Xpand.Events.Unity.Channels {
    /// <summary>Base class for strongly typed ScriptableObject signal channels.</summary>
    public abstract class SignalChannel<T> : ScriptableObject {
        [NonSerialized] private Signal<T>? _signal;

        private Signal<T> RuntimeSignal => _signal ?? (_signal = new Signal<T>());

        /// <summary>Gets the number of current runtime listeners.</summary>
        public int Count => RuntimeSignal.Count;

        /// <summary>Registers a runtime listener.</summary>
        public IDisposable Subscribe(Action<T> handler, int priority = 0) {
            return RuntimeSignal.Subscribe(handler, priority);
        }

        /// <summary>Raises this channel synchronously on the calling thread.</summary>
        public void Raise(T payload) {
            RuntimeSignal.Publish(payload);
        }

        /// <summary>Clears runtime-only subscriptions when Unity unloads the asset.</summary>
        protected virtual void OnDisable() {
            _signal?.Clear();
        }
    }
}
