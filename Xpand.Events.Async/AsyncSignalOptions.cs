#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xpand.Events.Async {
    /// <summary>Immutable per-instance async exception behavior.</summary>
    public sealed class AsyncSignalOptions {
        private static readonly AsyncSignalOptions FailFastOptions = new AsyncSignalOptions(false, null);

        private AsyncSignalOptions(
            bool reportAndContinue,
            Func<Exception, CancellationToken, ValueTask>? exceptionSink) {
            ReportAndContinueEnabled = reportAndContinue;
            ExceptionSink = exceptionSink;
        }

        /// <summary>Gets the policy that immediately propagates the first handler failure.</summary>
        public static AsyncSignalOptions FailFast => FailFastOptions;

        /// <summary>Creates a policy that awaits the sink and continues to the next handler.</summary>
        public static AsyncSignalOptions ReportAndContinue(
            Func<Exception, CancellationToken, ValueTask> exceptionSink) {
            if (exceptionSink == null) throw new ArgumentNullException(nameof(exceptionSink));
            return new AsyncSignalOptions(true, exceptionSink);
        }

        internal bool ReportAndContinueEnabled { get; }

        internal Func<Exception, CancellationToken, ValueTask>? ExceptionSink { get; }
    }
}
