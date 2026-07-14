using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using NUnit.Framework;
using Xpand.Events.Async;
using Xpand.Events.Extensions.Microsoft;

namespace Xpand.Events.Tests.Signals {
    public sealed class SignalAdapterTests {
        [Test]
        public void ObservableForwardsOnNextAndUsesSubscriptionLifetime() {
            var signal = new Signal<int>();
            var observer = new RecordingObserver<int>();
            IDisposable subscription = signal.AsObservable(priority: 10).Subscribe(observer);

            signal.Publish(42);
            subscription.Dispose();
            signal.Publish(100);

            Assert.That(observer.Values, Is.EqualTo(new[] { 42 }));
            Assert.That(observer.Completed, Is.False);
        }

        [Test]
        public void SynchronousChannelAdapterRejectsFullWaitModeChannel() {
            Channel<int> channel = Channel.CreateBounded<int>(new BoundedChannelOptions(1) {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = true
            });
            var signal = new Signal<int>();
            using IDisposable subscription = signal.SubscribeTo(channel.Writer);
            signal.Publish(1);

            Assert.Throws<InvalidOperationException>((Action)(() => signal.Publish(2)));
        }

        [Test]
        public async Task AsyncChannelAdapterAwaitsBoundedBackpressure() {
            Channel<int> channel = Channel.CreateBounded<int>(new BoundedChannelOptions(1) {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = true
            });
            var signal = new AsyncSignal<int>();
            using IDisposable subscription = signal.SubscribeTo(channel.Writer);
            await signal.PublishAsync(1);

            Task blockedPublish = signal.PublishAsync(2).AsTask();
            Assert.That(blockedPublish.IsCompleted, Is.False);

            Assert.That(await channel.Reader.ReadAsync(), Is.EqualTo(1));
            await blockedPublish;
            Assert.That(await channel.Reader.ReadAsync(), Is.EqualTo(2));
        }

        [Test]
        public async Task ChannelPumpPublishesEveryItemInOrder() {
            Channel<int> channel = Channel.CreateUnbounded<int>();
            var signal = new Signal<int>();
            var observed = new List<int>();
            signal.Subscribe(observed.Add);

            await channel.Writer.WriteAsync(1);
            await channel.Writer.WriteAsync(2);
            channel.Writer.Complete();
            await channel.Reader.PumpToAsync(signal);

            Assert.That(observed, Is.EqualTo(new[] { 1, 2 }));
        }

        private sealed class RecordingObserver<T> : IObserver<T> {
            internal List<T> Values { get; } = new List<T>();
            internal bool Completed { get; private set; }

            public void OnCompleted() {
                Completed = true;
            }

            public void OnError(Exception error) {
                throw error;
            }

            public void OnNext(T value) {
                Values.Add(value);
            }
        }
    }
}
