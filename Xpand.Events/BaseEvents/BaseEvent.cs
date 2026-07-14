using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xpand.Events.Interfaces;

namespace Xpand.Events {
    /// <summary>
    /// Base event class
    /// </summary>
    /// <typeparam name="T">event delegate type</typeparam>
    public abstract class BaseEvent<T> : ISuspendable where T : Delegate {

        private List<T> _subscriptions;
        private HashSet<T> _subscriptionsCache;
        private bool _isSuspended;
        
        
        /// <summary>
        /// Gets the number of registered listeners.
        /// </summary>
        public int Count => _subscriptionsCache.Count;

        public bool IsSuspended => _isSuspended;

        
        public BaseEvent() {
            _subscriptions = new List<T>(XpandEventsConfig.DefaultSubscriptionsBuffer);
            _subscriptionsCache = new HashSet<T>();
            _isSuspended = false;
        }

        public bool AddListener(T listener) {
            if (listener == null) throw new ArgumentNullException(nameof(listener));
            if (_subscriptionsCache.Contains(listener)) return false;
            _subscriptions.Add(listener);
            _subscriptionsCache.Add(listener);
            return true;
        }

        public bool RemoveListener(T listener) {
            if (!_subscriptionsCache.Contains(listener)) return false;
            _subscriptionsCache.Remove(listener);
            return _subscriptions.Remove(listener);
        }

        public bool Contains(T listener) {
            return _subscriptionsCache.Contains(listener);
        }

        /// <summary>
        /// Removes all registered listeners.
        /// </summary>
        public void Clear() {
            _subscriptions.Clear();
            _subscriptionsCache.Clear();
        }

        public void Suspend() {
            _isSuspended = true;
        }

        public void Unsuspend() {
            _isSuspended = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] GetImmutableSubscriptionArray() {
            return _subscriptions.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void PrepareInvoke() {
        }

    }
}
