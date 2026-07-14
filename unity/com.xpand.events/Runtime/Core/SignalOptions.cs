#nullable enable

using System;

namespace Xpand.Events {
    /// <summary>
    /// Immutable per-instance options that define how a signal handles listener exceptions.
    /// </summary>
    public sealed class SignalOptions {
        private static readonly SignalOptions FailFastOptions = new SignalOptions(false, null);

        private SignalOptions(bool reportAndContinue, Action<Exception>? exceptionSink) {
            ReportAndContinueEnabled = reportAndContinue;
            ExceptionSink = exceptionSink;
        }

        /// <summary>
        /// Gets the default policy, which immediately propagates the first listener exception.
        /// </summary>
        public static SignalOptions FailFast => FailFastOptions;

        /// <summary>
        /// Creates a policy that reports each listener exception and continues with the next listener.
        /// </summary>
        /// <param name="exceptionSink">The callback that receives listener exceptions.</param>
        /// <returns>An immutable report-and-continue policy.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exceptionSink"/> is <see langword="null"/>.</exception>
        public static SignalOptions ReportAndContinue(Action<Exception> exceptionSink) {
            if (exceptionSink == null) throw new ArgumentNullException(nameof(exceptionSink));
            return new SignalOptions(true, exceptionSink);
        }

        internal bool ReportAndContinueEnabled { get; }

        internal Action<Exception>? ExceptionSink { get; }
    }

    internal static class SignalExceptionDispatcher {
        internal static void Report(SignalOptions options, Exception handlerException) {
            try {
                options.ExceptionSink!(handlerException);
            }
            catch (Exception sinkException) {
                throw new AggregateException(
                    "A signal listener failed and the configured exception sink also failed.",
                    handlerException,
                    sinkException);
            }
        }
    }
}
