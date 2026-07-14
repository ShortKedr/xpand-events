using System;
using NUnit.Framework;
using ClassicAssert = NUnit.Framework.Legacy.ClassicAssert;

namespace Xpand.Events.Tests {
    [TestFixture]
    public class SafeXEventTests {
        [Test]
        public void AddRemoveContainsOps() {
            SafeXEvent ev = new SafeXEvent();
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
            SafeXEvent ev = new SafeXEvent();
            Event listener = () => {};
            bool a1 = ev.AddListener(listener);
            bool a2 = ev.AddListener(listener);
            ClassicAssert.IsTrue(a1 && !a2);
        }

        [Test]
        public void AddListenerRejectsNullWithoutChangingSubscriptions() {
            SafeXEvent ev = new SafeXEvent();

            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(new Action(() => ev.AddListener(null)));

            ClassicAssert.AreEqual("listener", exception.ParamName);
            ClassicAssert.AreEqual(0, ev.GetImmutableSubscriptionArray().Length);
        }

        [Test]
        public void GenericAddListenerRejectsNullWithoutChangingSubscriptions() {
            SafeXEvent<int> ev = new SafeXEvent<int>();

            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(new Action(() => ev.AddListener(null)));

            ClassicAssert.AreEqual("listener", exception.ParamName);
            ClassicAssert.AreEqual(0, ev.Count);
        }

        [Test]
        public void SuspendWorks() {
            SafeXEvent ev = new SafeXEvent();
            bool wasCalled = false;
            Event listener = () => wasCalled = true;
            ev.AddListener(listener);
            ev.Suspend();
            ev.Invoke();
            ClassicAssert.IsTrue(!wasCalled);
        }
        
        [Test]
        public void UnsuspendWorks() {
            SafeXEvent ev = new SafeXEvent();
            bool wasCalled = false;
            Event listener = () => wasCalled = true;
            ev.AddListener(listener);
            ev.Unsuspend();
            ev.Invoke();
            ClassicAssert.IsTrue(wasCalled);
        }

        [Test]
        public void ExceptionCatch() {
            SafeXEvent ev = new SafeXEvent();
            Event listener = () => throw new Exception();
            ev.AddListener(listener);
            ev.Invoke();
            Assert.Pass();
        }

        [Test]
        public void Logging() {
            SafeXEvent ev = new SafeXEvent();
            Event listener = () => throw new Exception();
            ev.AddListener(listener);
            
            bool wasOnCalled = false;
            XpandEventsConfig.LogLevel = LogLevel.Exception;
            XEventLogger.ExceptionDelegate onExListener = exception => wasOnCalled = true;
            XEventLogger.Exception += onExListener;
            ev.Invoke();
            XEventLogger.Exception -= onExListener;
            
            bool wasOffCalled = false;
            XpandEventsConfig.LogLevel = LogLevel.None;
            XEventLogger.ExceptionDelegate offExListener = exception => wasOffCalled = true;
            XEventLogger.Exception += offExListener;
            ev.Invoke();
            XEventLogger.Exception -= offExListener;
            
            ClassicAssert.IsTrue(wasOnCalled && !wasOffCalled);
        }

        [Test]
        public void ImplicitLogging() {
            SafeXEvent ev = new SafeXEvent();
            Event listener = () => throw new Exception();
            ev.AddListener(listener);
            
            bool wasCalled = false;
            XpandEventsConfig.LogLevel = LogLevel.None;
            XEventLogger.ExceptionDelegate exListener = exception => wasCalled = true;
            XEventLogger.ImplicitException += exListener;
            ev.Invoke();
            XEventLogger.ImplicitException -= exListener;
            
            ClassicAssert.IsTrue(wasCalled);
        }
    }
}
