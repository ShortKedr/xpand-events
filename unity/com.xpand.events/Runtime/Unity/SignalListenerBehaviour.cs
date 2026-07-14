#nullable enable

using System;
using UnityEngine;

namespace Xpand.Events.Unity {
    /// <summary>
    /// Defines an enable/disable subscription scope for a Unity component.
    /// </summary>
    public abstract class SignalListenerBehaviour : MonoBehaviour {
        private readonly CompositeSubscription _enabledSubscriptions = new CompositeSubscription();
        private readonly CompositeSubscription _destroySubscriptions = new CompositeSubscription();

        /// <summary>
        /// Called after enable. Add subscriptions to <paramref name="subscriptions"/>
        /// so they are disposed automatically on disable.
        /// </summary>
        protected abstract void SubscribeOnEnable(CompositeSubscription subscriptions);

        /// <summary>Tracks a subscription until this component is destroyed.</summary>
        protected IDisposable TrackUntilDestroy(IDisposable subscription) {
            return _destroySubscriptions.Add(subscription);
        }

        private void OnEnable() {
            _enabledSubscriptions.Clear();
            try {
                SubscribeOnEnable(_enabledSubscriptions);
            }
            catch {
                _enabledSubscriptions.Clear();
                throw;
            }
        }

        private void OnDisable() {
            _enabledSubscriptions.Clear();
        }

        private void OnDestroy() {
            try {
                _enabledSubscriptions.Dispose();
            }
            finally {
                _destroySubscriptions.Dispose();
            }
        }
    }
}
