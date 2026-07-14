using NUnit.Framework;
using UnityEngine;

namespace Xpand.Events.Unity.Tests {
    public sealed class UnityObjectSignalTests {
        [Test]
        public void DestroyedOwnerIsSkippedAndRegistrationRemovesItself() {
            var owner = ScriptableObject.CreateInstance<TestOwner>();
            var signal = new Signal<int>();
            var calls = 0;
            signal.SubscribeUnity(owner, _ => calls++);

            Object.DestroyImmediate(owner);
            signal.Publish(42);

            Assert.That(calls, Is.Zero);
            Assert.That(signal.Count, Is.Zero);
        }

        [Test]
        public void NullOwnerIsRejected() {
            var signal = new Signal();
            Assert.Throws<System.ArgumentNullException>(() => signal.SubscribeUnity(null, () => { }));
        }

        private sealed class TestOwner : ScriptableObject {
        }
    }
}
