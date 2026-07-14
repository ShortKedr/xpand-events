using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Xpand.Events.Async;

namespace Xpand.Events.Tests.Signals {
    public sealed class AsyncSignalTests {
        [Test]
        public async Task PublishAwaitsHandlersSequentiallyInStablePriorityOrder() {
            var signal = new AsyncSignal<int>();
            var observed = new List<int>();
            signal.Subscribe((value, _) => AddAfterYield(observed, value), priority: int.MinValue);
            signal.Subscribe((value, _) => AddAfterYield(observed, value * 10), priority: int.MaxValue);
            signal.Subscribe((value, _) => AddAfterYield(observed, value * 100), priority: int.MaxValue);

            await signal.PublishAsync(2);

            Assert.That(observed, Is.EqualTo(new[] { 20, 200, 2 }));
        }

        [Test]
        public void NullHandlerIsRejected() {
            var signal = new AsyncSignal<int>();
            Assert.Throws<ArgumentNullException>((Action)(() => signal.Subscribe(null)));
        }

        [Test]
        public async Task DuplicateTokensRemoveExactlyOneRegistration() {
            var signal = new AsyncSignal<int>();
            var calls = 0;
            AsyncSignalHandler<int> handler = (_, __) => {
                calls++;
                return default;
            };
            IDisposable first = signal.Subscribe(handler);
            IDisposable second = signal.Subscribe(handler);

            first.Dispose();
            first.Dispose();
            await signal.PublishAsync(0);
            Assert.That(calls, Is.EqualTo(1));

            second.Dispose();
            await signal.PublishAsync(0);
            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void CancellationStopsBeforeNextHandlerAndIsNotReported() {
            var cancellation = new CancellationTokenSource();
            var reported = 0;
            var calls = 0;
            var signal = new AsyncSignal<int>(AsyncSignalOptions.ReportAndContinue((_, __) => {
                reported++;
                return default;
            }));
            signal.Subscribe((_, __) => {
                calls++;
                cancellation.Cancel();
                return default;
            }, priority: 10);
            signal.Subscribe((_, __) => {
                calls++;
                return default;
            });

            Assert.ThrowsAsync<OperationCanceledException>(
                (Func<Task>)(async () => await signal.PublishAsync(0, cancellation.Token)));
            Assert.That(calls, Is.EqualTo(1));
            Assert.That(reported, Is.Zero);
        }

        [Test]
        public async Task ReportAndContinueAwaitsSinkThenContinues() {
            var events = new List<string>();
            var signal = new AsyncSignal<int>(AsyncSignalOptions.ReportAndContinue(async (exception, _) => {
                await Task.Yield();
                events.Add("reported:" + exception.Message);
            }));
            signal.Subscribe((_, __) => throw new InvalidOperationException("broken"));
            signal.Subscribe((_, __) => {
                events.Add("continued");
                return default;
            });

            await signal.PublishAsync(0);

            Assert.That(events, Is.EqualTo(new[] { "reported:broken", "continued" }));
        }

        [Test]
        public void SinkFailurePreservesBothExceptions() {
            var signal = new AsyncSignal<int>(AsyncSignalOptions.ReportAndContinue((_, __) =>
                new ValueTask(Task.FromException(new ArgumentException("sink")))));
            signal.Subscribe((_, __) => throw new InvalidOperationException("handler"));

            AggregateException exception = Assert.ThrowsAsync<AggregateException>(
                (Func<Task>)(async () => await signal.PublishAsync(0)));

            Assert.That(exception.InnerExceptions, Has.Count.EqualTo(2));
            Assert.That(exception.InnerExceptions[0], Is.TypeOf<InvalidOperationException>());
            Assert.That(exception.InnerExceptions[1], Is.TypeOf<ArgumentException>());
        }

        [Test]
        public async Task MutationDuringPublishAffectsNextSnapshot() {
            var signal = new AsyncSignal<int>();
            var observed = new List<string>();
            IDisposable second = null;
            signal.Subscribe((_, __) => {
                observed.Add("first");
                second.Dispose();
                signal.Subscribe((___, ____) => {
                    observed.Add("third");
                    return default;
                });
                return default;
            });
            second = signal.Subscribe((_, __) => {
                observed.Add("second");
                return default;
            });

            await signal.PublishAsync(0);
            Assert.That(observed, Is.EqualTo(new[] { "first", "second" }));

            observed.Clear();
            await signal.PublishAsync(0);
            Assert.That(observed, Is.EqualTo(new[] { "first", "third" }));
        }

        [Test]
        public async Task SuspensionIsNestedAndScoped() {
            var signal = new AsyncSignal<int>();
            var calls = 0;
            signal.Subscribe((_, __) => {
                calls++;
                return default;
            });

            using (signal.Suspend()) {
                using (signal.Suspend()) await signal.PublishAsync(0);
                await signal.PublishAsync(0);
            }
            await signal.PublishAsync(0);

            Assert.That(calls, Is.EqualTo(1));
        }

        private static async ValueTask AddAfterYield(List<int> observed, int value) {
            await Task.Yield();
            observed.Add(value);
        }
    }
}
