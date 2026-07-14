# Xpand Events

Xpand Events is an experimental in-process signal library for .NET and Unity.
The prerelease packages are not yet production-ready; the legacy `XEvent`
implementation remains a compatibility prototype.

## Current limitations

- Subscriptions keep strong references to delegates. This is not a weak-event
  implementation, and subscribers must be removed explicitly when appropriate.
- The legacy `XEvent` families are not thread-safe and retain global logging and
  configuration. The new `Signal` API is thread-safe and uses per-instance
  options.
- Synchronous signals do not support `async void` listeners. Sequential
  `ValueTask` handlers are available only through the optional prerelease
  `Xpand.Events.Async` package.
- The legacy NuGet DLL is not a Unity integration. The prerelease UPM package has
  local Unity 6000.3 Edit/Play Mode, Mono, IL2CPP, Android-build, and WebGL-build
  validation; clean CI, device/browser runtime, and Git-tag installation are
  still pending.

The planned production contract and the work required to reach it are tracked in
[ROADMAP.md](ROADMAP.md).

The exact behavior of the existing `XEvent` API is documented in
[Current legacy event semantics](docs/legacy-semantics.md).

The new prerelease `Signal<T>` API is documented in
[Signal contract](docs/signal-contract.md). It provides strong disposable
subscriptions, stable priorities, thread-safe snapshot publishing, explicit
per-instance exception behavior, and scoped suspension. These guarantees apply
to `Signal` and `Signal<T>`, not to the legacy `XEvent` families.

Sequential `ValueTask` handlers are available in the separate prerelease
`Xpand.Events.Async` package and documented in the
[async signal contract](docs/async-signal-contract.md).

Optional DI, logging, activities, and metrics are documented in
[Microsoft extensions](docs/microsoft-extensions.md).

Framework-neutral probes are documented in
[Testing helpers](docs/testing-helpers.md).

Reproducible .NET and Unity Player snapshots are recorded in the
[Performance and allocation baseline](docs/performance-baseline.md).

The exact tested matrix and servicing rules are defined in the
[Support policy](docs/support-policy.md).

Optional ownership and lifecycle diagnostics are documented in
[Analyzers](docs/analyzers.md). Runnable worker, desktop, and bounded-channel
examples are listed in [Integration samples](docs/integration-samples.md).

Signals and their adapters are process-local. See
[Delivery boundaries](docs/delivery-boundaries.md) before integrating with a
durable queue, broker, or transactional outbox.

See [Migrating from XEvent to Signal](docs/migration-xevent-to-signal.md) and the
[versioning and deprecation policy](docs/versioning.md) when adopting the new
package. The [strong-name decision](docs/decisions/0001-strong-name-policy.md)
documents why prerelease assemblies are currently unsigned.

# Xpand-Project links
- [Xpand-Events](https://github.com/ShortKedr/xpand-events)

# Summary

# Table of contents
 * [Requirements](#requirements)
 * [Current legacy semantics](docs/legacy-semantics.md)
 * [Hot to use](#how-to-use)
 * [Event usage patterns](#event-usage-patterns)
   * [Implicit pattern](#implicit-pattern)
   * [Class arguments pattern](#class-arguments-pattern)
   * [Tuple arguments pattern](#tuple-arguments-pattern)
   * [Record arguments pattern](#record-arguments-pattern)
 * [Install](#install)
   * [Assembly DLL](#install-assembly)
   * [Nuget package](#install-nuget)
 * [Performance](#performance)
 * [Migrating from default events](#migrating)
 * [Async use](#async-use)
 * [Multithreading](#multithreading)
 * [Use with Unity Engine](#use-with-unity)


# <a id="requirements"></a>Requirements
* **Min language version:** C# 8.0;  
* **Recomended language version**: C# 9.0;  
* **Target Framework versions:** .NET Standard 2.0 and .NET 8.0.

With C# 9.0 you will able to use `record` events. Record events moved to different project, that requires C# 9.0 support

# <a id="how-to-use"></a>How to use
There will be some useful info soon

# <a id="event-usage-patterns"></a>Event usage patterns

## <a id="implicit-pattern"></a>Implicit pattern
There will be implicit pattern soon

## <a id="class-arguments-pattern"></a>Class arguments pattern
There will be class arguments pattern soon

## <a id="tuple-arguments-pattern"></a>Tuple arguments pattern
There will be tuple arguments pattern soon

## <a id="record-arguments-pattern"></a>Record arguments pattern
There will be record arguments pattern soon
 
 
# <a id="install"></a>Install
There will be some useful info soon

## <a id="install-assembly"></a>Assembly DLL
There will be some useful info soon

## <a id="install-nuget"></a>Nuget package
There will be some useful info soon
 
 
# <a id="performance"></a>Performance

`Xpand.Events.Benchmark` contains a BenchmarkDotNet dispatch benchmark with
`MemoryDiagnoser`, static no-op handlers, fixed setup state, and a standard .NET
event baseline. No benchmark result is published here until the supported
platform matrix has been measured reproducibly.

Run it with:

```shell
dotnet run --project Xpand.Events.Benchmark -c Release
```

# <a id="migrating"></a>Migrating from legacy events

Use the step-by-step [XEvent to Signal migration guide](docs/migration-xevent-to-signal.md).

# <a id="async-use"></a>Async usage

`Signal` and `Signal<T>` are synchronous and invoke delegates on the publishing
thread. Do not register `async void` listeners. The separate prerelease
`Xpand.Events.Async` package provides sequential `ValueTask` dispatch,
`CancellationToken`, and explicit fail-fast or report-and-continue policies; see
the [async signal contract](docs/async-signal-contract.md).

# <a id="multithreading"></a>Multithreading

`Signal` and `Signal<T>` subscribe, dispose, publish, clear, and suspend operations
are thread-safe. Handlers execute synchronously on the publishing thread. The
legacy `XEvent` API is not thread-safe; external synchronization is required when
an instance is accessed by more than one thread.

# <a id="use-with-unity"></a>Use with Unity Engine

The prerelease package is under `unity/com.xpand.events` and contains Core,
lifecycle helpers, a main-thread dispatcher, ScriptableObject channels, samples,
and Edit/Play Mode tests. See its
[Unity documentation](unity/com.xpand.events/Documentation~/index.md) for the
exact verified matrix and remaining limitations. Do not infer support for an
unlisted Unity version or platform.
