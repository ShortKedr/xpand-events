using System;
using System.Collections.Generic;
using Xpand.Events;

namespace Xpand.Events.Smoke {
    internal static class SignalSmokeProgram {
        public static int Main() {
            List<string> calls = new List<string>();
            Signal<string> signal = new Signal<string>();
            using (signal.Subscribe(value => calls.Add("low:" + value), int.MinValue))
            using (signal.Subscribe(value => calls.Add("high:" + value), int.MaxValue)) {
                signal.Publish("payload");
            }

            if (calls.Count != 2 || calls[0] != "high:payload" || calls[1] != "low:payload") {
                return 1;
            }

            Exception reported = null;
            bool continued = false;
            Signal isolated = new Signal(SignalOptions.ReportAndContinue(exception => reported = exception));
            InvalidOperationException expected = new InvalidOperationException("expected smoke-test failure");
            using (isolated.Subscribe(() => throw expected, priority: 1))
            using (isolated.Subscribe(() => continued = true)) {
                isolated.Publish();
            }

            if (!ReferenceEquals(reported, expected) || !continued) return 2;

            Console.WriteLine("Xpand.Events.Core smoke test passed.");
            return 0;
        }
    }
}
