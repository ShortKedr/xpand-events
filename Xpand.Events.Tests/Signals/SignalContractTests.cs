using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Xpand.Events.Tests.Signals {
    [TestFixture]
    public class SignalContractTests {
        [Test]
        public void GenericSignalPublishesPayload() {
            Signal<string> signal = new Signal<string>();
            string received = null;
            using (signal.Subscribe(value => received = value)) {
                signal.Publish("payload");
            }

            Assert.That(received, Is.EqualTo("payload"));
        }

        [Test]
        public void NoPayloadSignalPublishes() {
            Signal signal = new Signal();
            int calls = 0;
            using (signal.Subscribe(() => calls++)) {
                signal.Publish();
            }

            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void SubscribeRejectsNull() {
            Signal<int> signal = new Signal<int>();

            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                new Action(() => signal.Subscribe(null)));

            Assert.That(exception.ParamName, Is.EqualTo("handler"));
            Assert.That(signal.Count, Is.Zero);
        }

        [Test]
        public void DuplicateRegistrationsAreIndependent() {
            Signal signal = new Signal();
            int calls = 0;
            Action handler = () => calls++;
            IDisposable first = signal.Subscribe(handler);
            IDisposable second = signal.Subscribe(handler);

            signal.Publish();
            first.Dispose();
            signal.Publish();
            first.Dispose();
            second.Dispose();
            signal.Publish();

            Assert.That(calls, Is.EqualTo(3));
            Assert.That(signal.Count, Is.Zero);
        }

        [Test]
        public void PriorityUsesFullIntRangeAndPreservesTieOrder() {
            Signal signal = new Signal();
            List<string> calls = new List<string>();
            using (signal.Subscribe(() => calls.Add("min"), int.MinValue))
            using (signal.Subscribe(() => calls.Add("zero-first"), 0))
            using (signal.Subscribe(() => calls.Add("max"), int.MaxValue))
            using (signal.Subscribe(() => calls.Add("zero-second"), 0)) {
                signal.Publish();
            }

            Assert.That(calls, Is.EqualTo(new[] { "max", "zero-first", "zero-second", "min" }));
        }

        [Test]
        public void MutationsDuringPublishAffectNextSnapshot() {
            Signal signal = new Signal();
            List<string> calls = new List<string>();
            IDisposable second = null;
            IDisposable added = null;

            using (signal.Subscribe(() => {
                calls.Add("first");
                second.Dispose();
                if (added == null) added = signal.Subscribe(() => calls.Add("added"));
            }, priority: 1)) {
                second = signal.Subscribe(() => calls.Add("second"));

                signal.Publish();
                signal.Publish();
            }

            second.Dispose();
            added.Dispose();
            Assert.That(calls, Is.EqualTo(new[] { "first", "second", "first", "added" }));
        }

        [Test]
        public void ReentrantPublishUsesLatestSnapshotWithoutCorruption() {
            Signal<int> signal = new Signal<int>();
            List<int> calls = new List<int>();
            IDisposable second = null;

            using (signal.Subscribe(value => {
                calls.Add(value * 10 + 1);
                if (value == 1) {
                    second.Dispose();
                    signal.Publish(2);
                }
            }, priority: 1)) {
                second = signal.Subscribe(value => calls.Add(value * 10 + 2));
                signal.Publish(1);
            }

            second.Dispose();
            Assert.That(calls, Is.EqualTo(new[] { 11, 21, 12 }));
            Assert.That(signal.Count, Is.Zero);
        }

        [Test]
        public void ClearDuringPublishAffectsNextSnapshot() {
            Signal signal = new Signal();
            int calls = 0;
            signal.Subscribe(() => {
                calls++;
                signal.Clear();
            }, priority: 1);
            signal.Subscribe(() => calls++);

            signal.Publish();
            signal.Publish();

            Assert.That(calls, Is.EqualTo(2));
            Assert.That(signal.Count, Is.Zero);
        }
    }
}
