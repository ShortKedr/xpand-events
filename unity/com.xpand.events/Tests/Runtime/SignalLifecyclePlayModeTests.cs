using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Xpand.Events.Unity.Tests {
    public sealed class SignalLifecyclePlayModeTests {
        [UnityTest]
        public IEnumerator EnableDisableControlsSubscriptionLifetime() {
            var signal = new Signal();
            var gameObject = new GameObject("signal-listener");
            gameObject.SetActive(false);
            var listener = gameObject.AddComponent<LifecycleProbe>();
            listener.Signal = signal;

            gameObject.SetActive(true);
            signal.Publish();
            Assert.That(listener.Calls, Is.EqualTo(1));

            gameObject.SetActive(false);
            signal.Publish();
            Assert.That(listener.Calls, Is.EqualTo(1));
            Assert.That(signal.Count, Is.Zero);

            gameObject.SetActive(true);
            signal.Publish();
            Assert.That(listener.Calls, Is.EqualTo(2));

            Object.Destroy(gameObject);
            yield return null;
            Assert.That(signal.Count, Is.Zero);
        }

        private sealed class LifecycleProbe : SignalListenerBehaviour {
            internal Signal Signal { get; set; }
            internal int Calls { get; private set; }

            protected override void SubscribeOnEnable(CompositeSubscription subscriptions) {
                subscriptions.Add(Signal.Subscribe(() => Calls++));
            }
        }
    }
}
