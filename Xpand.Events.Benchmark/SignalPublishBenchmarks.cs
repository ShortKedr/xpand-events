using System;
using BenchmarkDotNet.Attributes;

namespace Xpand.Events.Benchmark {
    [MemoryDiagnoser]
    public class SignalPublishBenchmarks {
        private event Action<int> DotNetEvent;
        private Signal<int> _signal;
        private IDisposable[] _subscriptions;

        [Params(1, 10, 100)]
        public int ListenerCount { get; set; }

        [GlobalSetup]
        public void Setup() {
            DotNetEvent = null;
            _signal = new Signal<int>();
            _subscriptions = new IDisposable[ListenerCount];

            for (int i = 0; i < ListenerCount; i++) {
                DotNetEvent += Consume;
                _subscriptions[i] = _signal.Subscribe(Consume);
            }
        }

        [GlobalCleanup]
        public void Cleanup() {
            for (int i = 0; i < _subscriptions.Length; i++) {
                _subscriptions[i].Dispose();
            }
        }

        [Benchmark(Baseline = true)]
        public void DotNetEventPublish() {
            DotNetEvent?.Invoke(42);
        }

        [Benchmark]
        public void SignalPublish() {
            _signal.Publish(42);
        }

        private static void Consume(int value) {
        }
    }
}
