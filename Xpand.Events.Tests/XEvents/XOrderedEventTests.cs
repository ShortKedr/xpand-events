using System;
using System.Collections.Generic;
using NUnit.Framework;
using ClassicAssert = NUnit.Framework.Legacy.ClassicAssert;
using CollectionAssert = NUnit.Framework.Legacy.CollectionAssert;

namespace Xpand.Events.Tests {
    [TestFixture]
    public class OrderedXEventTests {
        
        [Test]
        public void AddRemoveContainsOps() {
            OrderedXEvent ev = new OrderedXEvent();
            bool wasCalled = false;
            Event listener = () => wasCalled = true;
            ev.AddListener(listener);
            ev.Invoke();
            bool con1 = ev.Contains(listener);
            bool rem = ev.RemoveListener(listener);
            bool con2 = ev.Contains(listener);
            ClassicAssert.IsTrue(wasCalled && con1 && rem && !con2);
        }
        
        [Test]
        public void NoDuplicateListeners() {
            OrderedXEvent ev = new OrderedXEvent();
            Event listener = () => {};
            bool a1 = ev.AddListener(listener);
            bool a2 = ev.AddListener(listener);
            ClassicAssert.IsTrue(a1 && !a2);
        }

        [Test]
        public void CountTracksSuccessfulAddsAndRemoves() {
            OrderedXEvent ev = new OrderedXEvent();
            Event first = () => { };
            Event second = () => { };

            ClassicAssert.AreEqual(0, ev.Count);
            ClassicAssert.IsTrue(ev.AddListener(first, int.MinValue));
            ClassicAssert.IsTrue(ev.AddListener(second, int.MaxValue));
            ClassicAssert.IsFalse(ev.AddListener(first, 0));
            ClassicAssert.AreEqual(2, ev.Count);

            ClassicAssert.IsTrue(ev.RemoveListener(first));
            ClassicAssert.IsFalse(ev.RemoveListener(first));
            ClassicAssert.AreEqual(1, ev.Count);
        }

        [Test]
        public void ClearRemovesAllListenersAndIsIdempotent() {
            OrderedXEvent ev = new OrderedXEvent();
            Event first = () => { };
            Event second = () => { };
            ev.AddListener(first, int.MinValue);
            ev.AddListener(second, int.MaxValue);

            ev.Clear();
            ev.Clear();

            ClassicAssert.AreEqual(0, ev.Count);
            ClassicAssert.IsFalse(ev.Contains(first));
            ClassicAssert.IsFalse(ev.Contains(second));
            ClassicAssert.AreEqual(0, ev.GetImmutableSubscriptionArray().Length);
        }

        [Test]
        public void ClearDuringInvokeAffectsOnlyTheNextInvoke() {
            OrderedXEvent ev = new OrderedXEvent();
            int callCount = 0;
            ev.AddListener(() => {
                callCount++;
                ev.Clear();
            }, 1);
            ev.AddListener(() => callCount++, 0);

            ev.Invoke();

            ClassicAssert.AreEqual(2, callCount);
            ClassicAssert.AreEqual(0, ev.Count);

            ev.Invoke();
            ClassicAssert.AreEqual(2, callCount);
        }

        [Test]
        public void AddListenerRejectsNullWithoutChangingSubscriptions() {
            OrderedXEvent ev = new OrderedXEvent();

            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(new Action(() => ev.AddListener(null)));

            ClassicAssert.AreEqual("listener", exception.ParamName);
            ClassicAssert.AreEqual(0, ev.GetImmutableSubscriptionArray().Length);
        }

        [Test]
        public void GenericAddListenerRejectsNullWithoutChangingSubscriptions() {
            OrderedXEvent<int> ev = new OrderedXEvent<int>();

            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(new Action(() => ev.AddListener(null)));

            ClassicAssert.AreEqual("listener", exception.ParamName);
            ClassicAssert.AreEqual(0, ev.Count);
        }

        [Test]
        public void SuspendWorks() {
            OrderedXEvent ev = new OrderedXEvent();
            bool wasCalled = false;
            Event listener = () => wasCalled = true;
            ev.AddListener(listener);
            ev.Suspend();
            ev.Invoke();
            ClassicAssert.IsTrue(!wasCalled);
        }
        
        [Test]
        public void UnsuspendWorks() {
            OrderedXEvent ev = new OrderedXEvent();
            bool wasCalled = false;
            Event listener = () => wasCalled = true;
            ev.AddListener(listener);
            ev.Unsuspend();
            ev.Invoke();
            ClassicAssert.IsTrue(wasCalled);
        }

        [Test]
        public void SubscriptionsArrayOrder() {
            OrderedXEvent ev = new OrderedXEvent();
            List<int> callStack = new List<int>();
            
            Event l1 = () => { callStack.Add(1); };
            Event l2 = () => { callStack.Add(2); };
            Event l3 = () => { callStack.Add(3); };
            Event l4 = () => { callStack.Add(4); };
            Event l5 = () => { callStack.Add(5);};
            Event l6 = () => { callStack.Add(6);};
            Event l7 = () => { callStack.Add(7);};
            Event l8 = () => { callStack.Add(8);};
            Event l9 = () => { callStack.Add(9);};

            ev.AddListener(l2, 1);
            ev.AddListener(l1, 4);
            ev.AddListener(l3, 0);
            ev.AddListener(l4,2);
            ev.AddListener(l5, 3);
            ev.AddListener(l6, 0);
            ev.AddListener(l7, 4);
            ev.AddListener(l8, 1);
            ev.AddListener(l9, 2);
            
            var subscriptions = ev.GetImmutableSubscriptionArray();
            if (subscriptions.Length != 9) Assert.Fail($"Expected count: 9; Real count:{subscriptions.Length}");
            for (int i = 0; i < subscriptions.Length; i++) subscriptions[i].Invoke();
            
            int expectedOrder = 175492836;
            int builtOrder = 0;
            for (int i = 0; i < callStack.Count; i++) builtOrder += callStack[i] * (int)Math.Pow(10, callStack.Count-i-1);
            ClassicAssert.IsTrue(builtOrder == expectedOrder, $"Expected order: {expectedOrder}; Built order: {builtOrder}");
        }

        [Test]
        public void InvokeOrder() {
            OrderedXEvent ev = new OrderedXEvent();
            List<int> callStack = new List<int>();
            List<Action> addActions = new List<Action>();

            
            Event l1 = () => { callStack.Add(1); };
            Event l2 = () => { callStack.Add(2); };
            Event l3 = () => { callStack.Add(3); };
            Event l4 = () => { callStack.Add(4); };
            Event l5 = () => { callStack.Add(5);};
            Event l6 = () => { callStack.Add(6);};
            Event l7 = () => { callStack.Add(7);};
            Event l8 = () => { callStack.Add(8);};
            Event l9 = () => { callStack.Add(9);};

            ev.AddListener(l2, 1);
            ev.AddListener(l1, 4);
            ev.AddListener(l3, 0);
            ev.AddListener(l4,2);
            ev.AddListener(l5, 3);
            ev.AddListener(l6, 0);
            ev.AddListener(l7, 4);
            ev.AddListener(l8, 1);
            ev.AddListener(l9, 2);

            ev.Invoke();
            
            int expectedOrder = 175492836;
            int builtOrder = 0;
            for (int i = 0; i < callStack.Count; i++) builtOrder += callStack[i] * (int)Math.Pow(10, callStack.Count-i-1);
            ClassicAssert.IsTrue(builtOrder == expectedOrder, $"Expected order: {expectedOrder}; Built order: {builtOrder}");
        }

        [Test]
        public void InvokeOrdersFullIntPriorityRangeFromHighestToLowest() {
            OrderedXEvent ev = new OrderedXEvent();
            List<int> callStack = new List<int>();

            ev.AddListener(() => callStack.Add(int.MinValue), int.MinValue);
            ev.AddListener(() => callStack.Add(0), 0);
            ev.AddListener(() => callStack.Add(int.MaxValue), int.MaxValue);
            ev.AddListener(() => callStack.Add(-1), -1);
            ev.AddListener(() => callStack.Add(1), 1);

            ev.Invoke();

            CollectionAssert.AreEqual(
                new[] { int.MaxValue, 1, 0, -1, int.MinValue },
                callStack);
        }

        [Test]
        public void GenericInvokeOrdersFullIntPriorityRangeAndPassesPayload() {
            OrderedXEvent<string> ev = new OrderedXEvent<string>();
            List<string> calls = new List<string>();

            ev.AddListener(value => calls.Add($"min:{value}"), int.MinValue);
            ev.AddListener(value => calls.Add($"zero:{value}"), 0);
            ev.AddListener(value => calls.Add($"max:{value}"), int.MaxValue);

            ev.Invoke("payload");

            CollectionAssert.AreEqual(
                new[] { "max:payload", "zero:payload", "min:payload" },
                calls);
        }
        
    }
}
