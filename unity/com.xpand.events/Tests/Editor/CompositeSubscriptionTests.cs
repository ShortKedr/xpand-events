using System;
using NUnit.Framework;

namespace Xpand.Events.Unity.Tests {
    public sealed class CompositeSubscriptionTests {
        [Test]
        public void ClearDisposesEverySubscriptionAndRemainsReusable() {
            var composite = new CompositeSubscription();
            var signal = new Signal();
            var calls = 0;

            composite.Add(signal.Subscribe(() => calls++));
            composite.Clear();
            signal.Publish();
            Assert.That(calls, Is.Zero);

            composite.Add(signal.Subscribe(() => calls++));
            signal.Publish();
            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void DisposeIsIdempotentAndDisposesLateAdditionsImmediately() {
            var composite = new CompositeSubscription();
            var disposeCount = 0;

            composite.Dispose();
            composite.Dispose();
            composite.Add(new CallbackDisposable(() => disposeCount++));

            Assert.That(composite.IsDisposed, Is.True);
            Assert.That(disposeCount, Is.EqualTo(1));
        }

        [Test]
        public void DisposeContinuesAfterFailureAndReportsAllFailures() {
            var composite = new CompositeSubscription();
            var successfulDisposeCount = 0;
            composite.Add(new CallbackDisposable(() => successfulDisposeCount++));
            composite.Add(new CallbackDisposable(() => throw new InvalidOperationException("first")));
            composite.Add(new CallbackDisposable(() => throw new ArgumentException("second")));

            AggregateException exception = Assert.Throws<AggregateException>(() => composite.Dispose());

            Assert.That(successfulDisposeCount, Is.EqualTo(1));
            Assert.That(exception.InnerExceptions, Has.Count.EqualTo(2));
        }

        private sealed class CallbackDisposable : IDisposable {
            private readonly Action _callback;

            internal CallbackDisposable(Action callback) {
                _callback = callback;
            }

            public void Dispose() {
                _callback();
            }
        }
    }
}
