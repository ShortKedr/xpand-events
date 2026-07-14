using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Xpand.Events;
using Debug = UnityEngine.Debug;

internal static class ValidationSmoke {
    internal const string SuccessMarker = "XPAND_EVENTS_PLAYER_SMOKE_PASSED";

#if !UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Run() {
        try {
            var signal = new Signal<int>();
            var observed = new List<int>();
            using (signal.Subscribe(value => observed.Add(value), priority: int.MinValue))
            using (signal.Subscribe(value => observed.Add(value * 10), priority: int.MaxValue)) {
                signal.Publish(2);
            }

            if (observed.Count != 2 || observed[0] != 20 || observed[1] != 2 || signal.Count != 0) {
                throw new InvalidOperationException("Signal ordering or disposal failed in the Unity Player.");
            }

            var reported = false;
            var isolated = new Signal(SignalOptions.ReportAndContinue(_ => reported = true));
            isolated.Subscribe(() => throw new InvalidOperationException("expected"));
            isolated.Publish();
            if (!reported) throw new InvalidOperationException("Exception policy failed in the Unity Player.");

            RunPublishProfile();
            Debug.Log(SuccessMarker);
            Application.Quit(0);
        }
        catch (Exception exception) {
            Debug.LogException(exception);
            Application.Quit(1);
        }
    }

    private static void RunPublishProfile() {
        const int dispatchCount = 1_000_000;
        var signal = new Signal();
        using IDisposable subscription = signal.Subscribe(NoOp);
        for (int i = 0; i < 100; i++) signal.Publish();

        long allocatedBytes = -1;
#if !ENABLE_IL2CPP
        long allocationStart = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < dispatchCount; i++) signal.Publish();
        allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocationStart;
        if (allocatedBytes != 0) {
            throw new InvalidOperationException($"Steady-state publish allocated {allocatedBytes} managed bytes in the Mono Player.");
        }
#endif

        long timestampStart = Stopwatch.GetTimestamp();
        for (int i = 0; i < dispatchCount; i++) signal.Publish();
        long elapsedTicks = Stopwatch.GetTimestamp() - timestampStart;
        double nanosecondsPerDispatch = elapsedTicks * (1_000_000_000d / Stopwatch.Frequency) / dispatchCount;
        Debug.Log($"XPAND_EVENTS_PLAYER_PERF dispatches={dispatchCount} nsPerDispatch={nanosecondsPerDispatch:F2} allocatedBytes={allocatedBytes}");
    }

    private static void NoOp() {
    }
#endif
}
