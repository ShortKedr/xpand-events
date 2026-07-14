#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xpand.Events.Async;

namespace Xpand.Events.Extensions.Microsoft {
    /// <summary>Creates explicit signal exception policies backed by an <see cref="ILogger"/>.</summary>
    public static class SignalLoggingExtensions {
        private static readonly EventId HandlerFailureEvent = new EventId(1, "SignalHandlerFailure");

        /// <summary>Creates a synchronous report-and-continue policy for this logger.</summary>
        public static SignalOptions ToSignalOptions(
            this ILogger logger,
            global::Microsoft.Extensions.Logging.LogLevel level = global::Microsoft.Extensions.Logging.LogLevel.Error) {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            return SignalOptions.ReportAndContinue(exception =>
                logger.Log(level, HandlerFailureEvent, exception, "Xpand Events signal handler failed."));
        }

        /// <summary>Creates an asynchronous report-and-continue policy for this logger.</summary>
        public static AsyncSignalOptions ToAsyncSignalOptions(
            this ILogger logger,
            global::Microsoft.Extensions.Logging.LogLevel level = global::Microsoft.Extensions.Logging.LogLevel.Error) {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            return AsyncSignalOptions.ReportAndContinue((exception, _) => {
                logger.Log(level, HandlerFailureEvent, exception, "Xpand Events async signal handler failed.");
                return default(ValueTask);
            });
        }
    }
}
