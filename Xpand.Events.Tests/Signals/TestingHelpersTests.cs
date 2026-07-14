using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xpand.Events.Async;
using Xpand.Events.Testing;

namespace Xpand.Events.Tests.Signals {
    public sealed class TestingHelpersTests {
        [Test]
        public void PayloadProbeRecordsStableSnapshotAndHonorsTokenLifetime() {
            var signal = new Signal<int>();
            var probe = new SignalProbe<int>();
            using (probe.SubscribeTo(signal)) {
                signal.Publish(1);
                signal.Publish(2);
            }
            signal.Publish(3);

            Assert.That(probe.Snapshot(), Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public async Task PayloadProbeIsSafeForConcurrentPublish() {
            var signal = new Signal<int>();
            var probe = new SignalProbe<int>();
            using var subscription = probe.SubscribeTo(signal);

            Task[] workers = Enumerable.Range(0, 4)
                .Select(worker => Task.Run(() => {
                    for (int i = 0; i < 250; i++) signal.Publish(worker);
                }))
                .ToArray();
            await Task.WhenAll(workers);

            Assert.That(probe.Count, Is.EqualTo(1_000));
            Assert.That(probe.Snapshot(), Has.Length.EqualTo(1_000));
        }

        [Test]
        public async Task AsyncProbeRecordsPayloadsWithoutExtraScheduling() {
            var signal = new AsyncSignal<string>();
            var probe = new AsyncSignalProbe<string>();
            using var subscription = probe.SubscribeTo(signal);

            await signal.PublishAsync("first");
            await signal.PublishAsync("second");

            Assert.That(probe.Snapshot(), Is.EqualTo(new[] { "first", "second" }));
        }

        [Test]
        public void InvocationLogMakesPriorityOrderExplicit() {
            var signal = new Signal();
            var log = new InvocationLog();
            signal.Subscribe(log.Handler("low"), priority: -1);
            signal.Subscribe(log.Handler("high"), priority: 1);

            signal.Publish();

            Assert.That(log.Snapshot(), Is.EqualTo(new[] { "high", "low" }));
        }
    }
}
