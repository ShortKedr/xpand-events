#nullable enable

using System;
using UnityEngine;

namespace Xpand.Events.Unity.Channels {
    /// <summary>A no-payload ScriptableObject signal channel.</summary>
    [CreateAssetMenu(menuName = "Xpand Events/Void Signal Channel", fileName = "VoidSignalChannel")]
    public sealed class VoidSignalChannel : ScriptableObject {
        [NonSerialized] private Signal? _signal;

        private Signal RuntimeSignal => _signal ?? (_signal = new Signal());

        /// <summary>Gets the number of current runtime listeners.</summary>
        public int Count => RuntimeSignal.Count;

        /// <summary>Registers a runtime listener.</summary>
        public IDisposable Subscribe(Action handler, int priority = 0) {
            return RuntimeSignal.Subscribe(handler, priority);
        }

        /// <summary>Raises this channel synchronously on the calling thread.</summary>
        public void Raise() {
            RuntimeSignal.Publish();
        }

        private void OnDisable() {
            _signal?.Clear();
        }
    }
}
