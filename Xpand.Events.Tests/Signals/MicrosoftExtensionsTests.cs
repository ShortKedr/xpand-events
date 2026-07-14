using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Xpand.Events.Async;
using Xpand.Events.Extensions.Microsoft;

namespace Xpand.Events.Tests.Signals {
    public sealed class MicrosoftExtensionsTests {
        [Test]
        public void DiRegistersConfiguredSynchronousSignalWithRequestedLifetime() {
            var failures = 0;
            var services = new ServiceCollection();
            services.AddSignal<int>(SignalOptions.ReportAndContinue(_ => failures++));

            using (ServiceProvider provider = services.BuildServiceProvider()) {
                Signal<int> first = provider.GetRequiredService<Signal<int>>();
                Signal<int> second = provider.GetRequiredService<Signal<int>>();
                first.Subscribe(_ => throw new InvalidOperationException("expected"));

                first.Publish(0);

                Assert.That(second, Is.SameAs(first));
                Assert.That(failures, Is.EqualTo(1));
            }
        }

        [Test]
        public void DiRegistersAsyncSignal() {
            var services = new ServiceCollection();
            services.AddAsyncSignal<string>(ServiceLifetime.Transient);

            using (ServiceProvider provider = services.BuildServiceProvider()) {
                Assert.That(provider.GetRequiredService<AsyncSignal<string>>(),
                    Is.Not.SameAs(provider.GetRequiredService<AsyncSignal<string>>()));
            }
        }

        [Test]
        public void LoggerPolicyRecordsFailureAndContinues() {
            var logger = new RecordingLogger();
            var signal = new Signal(logger.ToSignalOptions());
            var continued = false;
            signal.Subscribe(() => throw new InvalidOperationException("broken"));
            signal.Subscribe(() => continued = true);

            signal.Publish();

            Assert.That(continued, Is.True);
            Assert.That(logger.Exceptions, Has.Count.EqualTo(1));
            Assert.That(logger.Exceptions[0].Message, Is.EqualTo("broken"));
        }

        [Test]
        public async Task AsyncLoggerPolicyRecordsFailureAndContinues() {
            var logger = new RecordingLogger();
            var signal = new AsyncSignal<int>(logger.ToAsyncSignalOptions());
            var continued = false;
            signal.Subscribe((_, __) => throw new InvalidOperationException("broken async"));
            signal.Subscribe((_, __) => {
                continued = true;
                return default;
            });

            await signal.PublishAsync(0);

            Assert.That(continued, Is.True);
            Assert.That(logger.Exceptions, Has.Count.EqualTo(1));
        }

        [Test]
        public void DisabledDiagnosticsDoNotAllocateDuringPublish() {
            var signal = new Signal<int>();
            using IDisposable subscription = signal.Subscribe(NoOp);
            using var diagnostics = new SignalDiagnostics("Xpand.Events.Tests.Disabled");
            for (int i = 0; i < 100; i++) signal.PublishWithDiagnostics(i, diagnostics);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 1_000; i++) signal.PublishWithDiagnostics(i, diagnostics);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
        }

        [Test]
        public void ActivityListenerReceivesInstrumentedPublish() {
            const string sourceName = "Xpand.Events.Tests.Enabled";
            var stopped = 0;
            using var listener = new ActivityListener {
                ShouldListenTo = source => source.Name == sourceName,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
                ActivityStopped = _ => stopped++
            };
            ActivitySource.AddActivityListener(listener);
            using var diagnostics = new SignalDiagnostics(sourceName);
            var signal = new Signal<int>();

            signal.PublishWithDiagnostics(42, diagnostics);

            Assert.That(stopped, Is.EqualTo(1));
        }

        private static void NoOp(int _) {
        }

        private sealed class RecordingLogger : ILogger {
            internal List<Exception> Exceptions { get; } = new List<Exception>();

            public IDisposable BeginScope<TState>(TState state) where TState : notnull {
                return EmptyScope.Instance;
            }

            public bool IsEnabled(global::Microsoft.Extensions.Logging.LogLevel logLevel) {
                return true;
            }

            public void Log<TState>(
                global::Microsoft.Extensions.Logging.LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception exception,
                Func<TState, Exception, string> formatter) {
                if (exception != null) Exceptions.Add(exception);
            }
        }

        private sealed class EmptyScope : IDisposable {
            internal static readonly EmptyScope Instance = new EmptyScope();
            public void Dispose() { }
        }
    }
}
