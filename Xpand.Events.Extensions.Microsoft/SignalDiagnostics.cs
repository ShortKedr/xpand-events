#nullable enable

using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Xpand.Events.Extensions.Microsoft {
    /// <summary>
    /// Per-instance ActivitySource and metrics instruments for opt-in signal publishing.
    /// </summary>
    public sealed class SignalDiagnostics : IDisposable {
        private readonly bool _metricsEnabled;
        private readonly Meter _meter;
        private readonly Counter<long> _published;
        private readonly Counter<long> _failed;
        private readonly Histogram<double> _duration;

        /// <summary>Creates diagnostics with dynamic activities and explicitly enabled or disabled metrics.</summary>
        public SignalDiagnostics(string sourceName, bool metricsEnabled = false, string? version = null) {
            if (sourceName == null) throw new ArgumentNullException(nameof(sourceName));
            if (sourceName.Length == 0) throw new ArgumentException("A diagnostics source name is required.", nameof(sourceName));

            _metricsEnabled = metricsEnabled;
            ActivitySource = new ActivitySource(sourceName, version);
            _meter = new Meter(sourceName, version);
            _published = _meter.CreateCounter<long>("xpand.events.publish.count");
            _failed = _meter.CreateCounter<long>("xpand.events.publish.errors");
            _duration = _meter.CreateHistogram<double>("xpand.events.publish.duration", "ms");
        }

        /// <summary>Gets the activity source owned by this instance.</summary>
        public ActivitySource ActivitySource { get; }

        internal bool IsEnabled => _metricsEnabled || ActivitySource.HasListeners();

        internal bool MetricsEnabled => _metricsEnabled;

        internal void RecordPublished(double elapsedMilliseconds) {
            _published.Add(1);
            _duration.Record(elapsedMilliseconds);
        }

        internal void RecordFailure(double elapsedMilliseconds) {
            _failed.Add(1);
            _duration.Record(elapsedMilliseconds);
        }

        /// <summary>Disposes the activity source and meter.</summary>
        public void Dispose() {
            ActivitySource.Dispose();
            _meter.Dispose();
        }
    }

    /// <summary>Explicit instrumented publish adapters.</summary>
    public static class SignalDiagnosticsExtensions {
        /// <summary>
        /// Publishes with opt-in activities and metrics. When diagnostics have no
        /// activity listeners and metrics are disabled, this delegates directly to Core.
        /// </summary>
        public static void PublishWithDiagnostics<T>(
            this Signal<T> signal,
            T payload,
            SignalDiagnostics diagnostics) {
            if (signal == null) throw new ArgumentNullException(nameof(signal));
            if (diagnostics == null) throw new ArgumentNullException(nameof(diagnostics));
            if (!diagnostics.IsEnabled) {
                signal.Publish(payload);
                return;
            }

            long started = Stopwatch.GetTimestamp();
            using (Activity? activity = diagnostics.ActivitySource.StartActivity("signal.publish", ActivityKind.Internal)) {
                try {
                    signal.Publish(payload);
                    if (diagnostics.MetricsEnabled) diagnostics.RecordPublished(GetElapsedMilliseconds(started));
                }
                catch (Exception exception) {
                    activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
                    if (diagnostics.MetricsEnabled) diagnostics.RecordFailure(GetElapsedMilliseconds(started));
                    throw;
                }
            }
        }

        private static double GetElapsedMilliseconds(long started) {
            return (Stopwatch.GetTimestamp() - started) * (1000d / Stopwatch.Frequency);
        }
    }
}
