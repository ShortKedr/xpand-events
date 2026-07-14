using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Xpand.Events.Interfaces;

namespace Xpand.Events {
    public abstract class BaseOrderedEvent<T> : ISuspendable where T : Delegate {

        private sealed class DescendingPriorityComparer : IComparer<int> {
            public static readonly DescendingPriorityComparer Instance = new DescendingPriorityComparer();

            public int Compare(int left, int right) {
                return right.CompareTo(left);
            }
        }
        
        private SortedList<int, List<T>> _subscriptions;
        private Dictionary<T, int> _orderBySubscriptionDict;
        private bool _isSuspended;

        
        /// <summary>
        /// Gets the number of registered listeners.
        /// </summary>
        public int Count => _orderBySubscriptionDict.Count;

        public bool IsSuspended => _isSuspended;

        
        public BaseOrderedEvent() {
            _subscriptions = new SortedList<int, List<T>>(DescendingPriorityComparer.Instance) {
                Capacity = XpandEventsConfig.DefaultSubscriptionsBuffer
            };
            _orderBySubscriptionDict = new Dictionary<T, int>(XpandEventsConfig.DefaultSubscriptionsBuffer);
            _isSuspended = false;
        }

        /// <summary>
        /// Adds new event listener is specified priority.
        /// Priority is optional param, so default priority equals to 0.
        /// Higher the priority, closer the listener to invoke in the invocation queue.
        /// Listeners with same priority value invokes in order they was added to the event.
        /// Null listeners are rejected with <see cref="ArgumentNullException"/>.
        /// </summary>
        /// <param name="listener">listener</param>
        /// <param name="priority">invocation priority</param>
        /// <returns>true if listener was added</returns>
        public bool AddListener(T listener, int priority = 0) {
            if (listener == null) throw new ArgumentNullException(nameof(listener));
            if (_orderBySubscriptionDict.ContainsKey(listener)) return false;
            if (!_subscriptions.ContainsKey(priority)) _subscriptions.Add(priority, new List<T>(XpandEventsConfig.DefaultSubscriptionsBuffer));
            _subscriptions[priority].Add(listener);
            _orderBySubscriptionDict.Add(listener, priority);
            return true;
        }

        public bool RemoveListener(T listener) {
            if (!_orderBySubscriptionDict.ContainsKey(listener)) return false;
            int order = _orderBySubscriptionDict[listener];
            _orderBySubscriptionDict.Remove(listener);
            return _subscriptions[order].Remove(listener);
        }
        
        public bool Contains(T listener) {
            return _orderBySubscriptionDict.ContainsKey(listener);
        }

        /// <summary>
        /// Removes all registered listeners.
        /// </summary>
        public void Clear() {
            _subscriptions.Clear();
            _orderBySubscriptionDict.Clear();
        }

        public void Suspend() {
            _isSuspended = true;
        }

        public void Unsuspend() {
            _isSuspended = false;
        }

        /// <summary>
        /// Return subscriptions array. Subscriptions are already ordered
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] GetImmutableSubscriptionArray() {
            IList<List<T>> orderLists = _subscriptions.Values;
            int totalCount = 0;
            for (int i = 0; i < orderLists.Count; i++) totalCount += orderLists[i].Count;
            List<T> result = new List<T>(totalCount);
            for (int i = 0; i < orderLists.Count; i++) {
                for (int j = 0; j < orderLists[i].Count; j++) {
                    result.Add(orderLists[i][j]);
                }
            }
            return result.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void PrepareInvoke() {
        }

    }
}
