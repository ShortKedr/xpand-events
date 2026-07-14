using NUnit.Framework;
using ClassicAssert = NUnit.Framework.Legacy.ClassicAssert;

namespace Xpand.Events.Tests {
    [TestFixture]
    public class XEventTests {

        [Test]
        public void AddRemoveContainsOps() {
            XEvent ev = new XEvent();
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
            XEvent ev = new XEvent();
            Event listener = () => {};
            bool a1 = ev.AddListener(listener);
            bool a2 = ev.AddListener(listener);
            ClassicAssert.IsTrue(a1 && !a2);
        }

        [Test]
        public void CountTracksSuccessfulAddsAndRemoves() {
            XEvent ev = new XEvent();
            Event first = () => { };
            Event second = () => { };

            ClassicAssert.AreEqual(0, ev.Count);
            ClassicAssert.IsTrue(ev.AddListener(first));
            ClassicAssert.IsTrue(ev.AddListener(second));
            ClassicAssert.IsFalse(ev.AddListener(first));
            ClassicAssert.AreEqual(2, ev.Count);

            ClassicAssert.IsTrue(ev.RemoveListener(first));
            ClassicAssert.IsFalse(ev.RemoveListener(first));
            ClassicAssert.AreEqual(1, ev.Count);
        }

        [Test]
        public void ClearRemovesAllListenersAndIsIdempotent() {
            XEvent ev = new XEvent();
            Event first = () => { };
            Event second = () => { };
            ev.AddListener(first);
            ev.AddListener(second);

            ev.Clear();
            ev.Clear();

            ClassicAssert.AreEqual(0, ev.Count);
            ClassicAssert.IsFalse(ev.Contains(first));
            ClassicAssert.IsFalse(ev.Contains(second));
            ClassicAssert.AreEqual(0, ev.GetImmutableSubscriptionArray().Length);
        }

        [Test]
        public void ClearDuringInvokeAffectsOnlyTheNextInvoke() {
            XEvent ev = new XEvent();
            int callCount = 0;
            ev.AddListener(() => {
                callCount++;
                ev.Clear();
            });
            ev.AddListener(() => callCount++);

            ev.Invoke();

            ClassicAssert.AreEqual(2, callCount);
            ClassicAssert.AreEqual(0, ev.Count);

            ev.Invoke();
            ClassicAssert.AreEqual(2, callCount);
        }

        [Test]
        public void AddListenerRejectsNullWithoutChangingSubscriptions() {
            XEvent ev = new XEvent();

            System.ArgumentNullException exception = Assert.Throws<System.ArgumentNullException>(new System.Action(() => ev.AddListener(null)));

            ClassicAssert.AreEqual("listener", exception.ParamName);
            ClassicAssert.AreEqual(0, ev.GetImmutableSubscriptionArray().Length);
        }

        [Test]
        public void GenericAddListenerRejectsNullWithoutChangingSubscriptions() {
            XEvent<int> ev = new XEvent<int>();

            System.ArgumentNullException exception = Assert.Throws<System.ArgumentNullException>(new System.Action(() => ev.AddListener(null)));

            ClassicAssert.AreEqual("listener", exception.ParamName);
            ClassicAssert.AreEqual(0, ev.Count);
        }

        [Test]
        public void SuspendWorks() {
            XEvent ev = new XEvent();
            bool wasCalled = false;
            Event listener = () => wasCalled = true;
            ev.AddListener(listener);
            ev.Suspend();
            ev.Invoke();
            ClassicAssert.IsTrue(!wasCalled);
        }
        
        [Test]
        public void UnsuspendWorks() {
            XEvent ev = new XEvent();
            bool wasCalled = false;
            Event listener = () => wasCalled = true;
            ev.AddListener(listener);
            ev.Unsuspend();
            ev.Invoke();
            ClassicAssert.IsTrue(wasCalled);
        }
    }
}
