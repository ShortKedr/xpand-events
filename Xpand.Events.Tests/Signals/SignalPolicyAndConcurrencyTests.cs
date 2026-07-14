using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xpand.Events.Tests.Signals {
    [TestFixture]
    public class SignalPolicyAndConcurrencyTests {
        [Test]
        public void DefaultPolicyFailsFast() {
            Signal signal = new Signal();
            InvalidOperationException failure = new InvalidOperationException("handler failed");
            bool laterHandlerCalled = false;
            using (signal.Subscribe(() => throw failure, priority: 1))
            using (signal.Subscribe(() => laterHandlerCalled = true)) {
                InvalidOperationException actual = Assert.Throws<InvalidOperationException>(new Action(signal.Publish));

                Assert.That(actual, Is.SameAs(failure));
                Assert.That(laterHandlerCalled, Is.False);
            }
        }

        [Test]
        public void ReportAndContinueReportsAndInvokesLaterHandlers() {
            InvalidOperationException failure = new InvalidOperationException("handler failed");
            Exception reported = null;
            bool laterHandlerCalled = false;
            Signal signal = new Signal(SignalOptions.ReportAndContinue(exception => reported = exception));
            using (signal.Subscribe(() => throw failure, priority: 1))
            using (signal.Subscribe(() => laterHandlerCalled = true)) {
                signal.Publish();
            }

            Assert.That(reported, Is.SameAs(failure));
            Assert.That(laterHandlerCalled, Is.True);
        }

        [Test]
        public void ExceptionSinkFailurePreservesBothExceptionsAndState() {
            InvalidOperationException handlerFailure = new InvalidOperationException("handler failed");
            ApplicationException sinkFailure = new ApplicationException("sink failed");
            Signal signal = new Signal(SignalOptions.ReportAndContinue(_ => throw sinkFailure));
            IDisposable subscription = signal.Subscribe(() => throw handlerFailure);

            AggregateException actual = Assert.Throws<AggregateException>(new Action(signal.Publish));

            Assert.That(actual.InnerExceptions, Has.Count.EqualTo(2));
            Assert.That(actual.InnerExceptions[0], Is.SameAs(handlerFailure));
            Assert.That(actual.InnerExceptions[1], Is.SameAs(sinkFailure));
            Assert.That(signal.Count, Is.EqualTo(1));

            subscription.Dispose();
            subscription.Dispose();
            Assert.That(signal.Count, Is.Zero);
        }

        [Test]
        public void ReportAndContinueRejectsNullSink() {
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                new Action(() => SignalOptions.ReportAndContinue(null)));

            Assert.That(exception.ParamName, Is.EqualTo("exceptionSink"));
        }

        [Test]
        public void SuspensionScopesAreNestedAndIdempotent() {
            Signal signal = new Signal();
            int calls = 0;
            using (signal.Subscribe(() => calls++)) {
                IDisposable outer = signal.Suspend();
                IDisposable inner = signal.Suspend();

                signal.Publish();
                outer.Dispose();
                outer.Dispose();
                signal.Publish();
                Assert.That(signal.IsSuspended, Is.True);

                inner.Dispose();
                signal.Publish();
                Assert.That(signal.IsSuspended, Is.False);
            }

            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void SuspensionDuringPublishDoesNotStopCurrentSnapshotButBlocksReentrancy() {
            Signal signal = new Signal();
            int calls = 0;
            IDisposable suspension = null;
            using (signal.Subscribe(() => {
                calls++;
                suspension = signal.Suspend();
                signal.Publish();
            }, priority: 1))
            using (signal.Subscribe(() => calls++)) {
                signal.Publish();
            }

            Assert.That(calls, Is.EqualTo(2));
            Assert.That(signal.IsSuspended, Is.True);
            suspension.Dispose();
        }

        [Test]
        public void HandlerExecutionDoesNotHoldMutationLock() {
            Signal signal = new Signal();
            using (ManualResetEventSlim entered = new ManualResetEventSlim())
            using (ManualResetEventSlim release = new ManualResetEventSlim())
            using (signal.Subscribe(() => {
                entered.Set();
                release.Wait(TimeSpan.FromSeconds(10));
            })) {
                Task publish = Task.Run(new Action(signal.Publish));
                Assert.That(entered.Wait(TimeSpan.FromSeconds(5)), Is.True);

                Task<IDisposable> subscribe = Task.Run(() => signal.Subscribe(() => { }));
                try {
                    Assert.That(subscribe.Wait(TimeSpan.FromSeconds(5)), Is.True);
                }
                finally {
                    release.Set();
                    publish.Wait(TimeSpan.FromSeconds(5));
                }

                subscribe.Result.Dispose();
            }
        }

        [Test]
        [Repeat(5)]
        public void ConcurrentPublishSubscribeAndDisposeDoNotCorruptState() {
            Signal<int> signal = new Signal<int>();
            int baseCalls = 0;
            using (signal.Subscribe(_ => Interlocked.Increment(ref baseCalls))) {
                Task publisherOne = Task.Run(() => PublishMany(signal, 4_000));
                Task publisherTwo = Task.Run(() => PublishMany(signal, 4_000));
                Task mutatorOne = Task.Run(() => MutateMany(signal, 2_000));
                Task mutatorTwo = Task.Run(() => MutateMany(signal, 2_000));

                Task.WaitAll(publisherOne, publisherTwo, mutatorOne, mutatorTwo);

                Assert.That(signal.Count, Is.EqualTo(1));
                Assert.That(baseCalls, Is.EqualTo(8_000));
            }
        }

        [Test]
        public void GenericPublishAllocatesZeroBytesAfterWarmup() {
            Signal<int> signal = new Signal<int>();
            IDisposable subscription = signal.Subscribe(IgnorePayload);
            for (int i = 0; i < 1_000; i++) signal.Publish(i);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 10_000; i++) signal.Publish(i);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            GC.KeepAlive(subscription);
            subscription.Dispose();
            Assert.That(allocated, Is.Zero);
        }

        [Test]
        public void NoPayloadPublishAllocatesZeroBytesAfterWarmup() {
            Signal signal = new Signal();
            IDisposable subscription = signal.Subscribe(Ignore);
            for (int i = 0; i < 1_000; i++) signal.Publish();

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 10_000; i++) signal.Publish();
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            GC.KeepAlive(subscription);
            subscription.Dispose();
            Assert.That(allocated, Is.Zero);
        }

        private static void PublishMany(Signal<int> signal, int count) {
            for (int i = 0; i < count; i++) signal.Publish(i);
        }

        private static void MutateMany(Signal<int> signal, int count) {
            for (int i = 0; i < count; i++) {
                IDisposable subscription = signal.Subscribe(IgnorePayload, i);
                subscription.Dispose();
                subscription.Dispose();
            }
        }

        private static void IgnorePayload(int value) {
        }

        private static void Ignore() {
        }
    }
}
