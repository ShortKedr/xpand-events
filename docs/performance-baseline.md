# Performance and allocation baseline

This document records reproducible validation snapshots, not latency SLAs. Compare
future runs on equivalent hardware/runtime and investigate material regressions;
do not compare nanoseconds directly across machines or runtime versions.

## .NET 8

Measured on 2026-07-14 with BenchmarkDotNet 0.15.8, .NET 8.0.19 Arm64 RyuJIT,
concurrent workstation GC, macOS Tahoe 26.2, and an Apple M3 Pro. The ShortRun job
used one launch, three warmups, and three measured iterations.

| Listeners | .NET event mean | `Signal<T>` mean | Signal allocated/op |
|---:|---:|---:|---:|
| 1 | 1.279 ns | 2.300 ns | 0 B |
| 10 | 27.043 ns | 18.712 ns | 0 B |
| 100 | 234.428 ns | 166.963 ns | 0 B |

The one-listener values are close to timer/overhead limits and the ShortRun
confidence intervals are wide. The useful baseline is the order of magnitude,
linear listener scaling, and zero managed allocation—not a promise that Signal
is universally faster than a .NET event.

Reproduce from the repository root:

```shell
dotnet run --project Xpand.Events.Benchmark -c Release -- \
  --filter '*SignalPublishBenchmarks*' --job short
```

The benchmark resets state in `GlobalSetup`, uses cached static no-op handlers,
keeps subscription tokens alive, includes a standard .NET event baseline, and
uses `MemoryDiagnoser`.

## Unity Player

Measured locally in development players built by Unity 6000.3.19f1 on macOS
arm64. Each snapshot warms the signal, then publishes 1,000,000 times to one
no-op handler.

| Backend | Mean snapshot | Managed allocation |
|---|---:|---:|
| Mono | 6.42 ns/publish | 0 B over 1,000,000 publishes |
| IL2CPP | 4.75 ns/publish | Not measurable with this Unity API |

Unity 6000.3 IL2CPP does not implement
`GC.GetAllocatedBytesForCurrentThread`, so the functional/timing smoke passed but
no IL2CPP allocation claim is made. Android and WebGL players were built but not
executed for this baseline. These gaps remain in the support matrix rather than
being inferred from the macOS result.

The Unity measurement source is
`unity/ValidationProject/Assets/ValidationSmoke.cs`; player build entry points
are in `unity/ValidationProject/Assets/Editor/ValidationCommands.cs`.
