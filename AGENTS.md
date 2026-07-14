# Xpand Events: agent instructions

This file applies to the entire repository.

## Mission

Turn Xpand Events from an experimental 2022 event helper into a small,
predictable, production-ready in-process signal library for Unity and the wider
C#/.NET ecosystem.

The product is an in-process signal primitive. It is not a distributed message
broker, durable event store, queue, mediator framework, or replacement for
Kafka/RabbitMQ.

Read [ROADMAP.md](ROADMAP.md) before making architectural or public API changes.
The roadmap is part of these instructions. Work on the earliest incomplete
milestone unless the user explicitly requests another area.

## Current state: do not overclaim

The existing code is a prototype, not an enterprise-ready package.

- Subscriptions are strong references. The library is not currently a weak-event
  implementation, despite the old README description.
- Collections and global configuration are not thread-safe.
- Every publish allocates a new subscription array.
- Ordered events overflow for some negative priorities.
- Unity lifecycle, Mono, IL2CPP, AOT, and managed stripping are not tested.
- The current tests cover mostly zero-argument happy paths.
- The current T4 and NuGet/release pipeline is obsolete and unreliable on modern
  cross-platform .NET SDKs.

Do not describe any of these capabilities as supported until the corresponding
roadmap exit criteria are implemented and verified.

## Target product shape

Keep the core platform-neutral and dependency-free. Platform integrations belong
in separate packages/projects.

Planned package boundaries:

- `Xpand.Events.Core`: synchronous signal implementation and contracts.
- `Xpand.Events.Async`: `ValueTask`-based asynchronous signals.
- `Xpand.Events.Unity`: Unity lifecycle and main-thread integration, distributed
  as a Unity Package Manager package.
- `Xpand.Events.Extensions.Microsoft`: optional DI, logging, and diagnostics
  integration.
- `Xpand.Events.Testing`: deterministic test helpers.
- `Xpand.Events.Analyzers`: misuse diagnostics and migration assistance.

The target public API is payload-oriented, for example:

```csharp
using var subscription = signal.Subscribe(HandleMessage, priority: 10);
signal.Publish(message);
```

Prefer one `Signal<T>` abstraction over generating public types for every arity
from zero through sixteen. Preserve old `XEvent<...>` types only as documented
compatibility facades when that is inexpensive.

## Required behavioral contracts

All implementations and reviews must preserve these contracts unless an ADR and
the roadmap explicitly change them:

- Strong subscriptions are the default and return an idempotent `IDisposable`
  token.
- Weak subscriptions are explicit opt-in and never silently replace strong
  subscriptions.
- A null handler is rejected with `ArgumentNullException`.
- The same handler may be subscribed more than once; each subscription token
  identifies and removes exactly one registration.
- Higher numeric priority runs first. Equal priority preserves subscription order.
- All `int` priority values, including `int.MinValue` and `int.MaxValue`, work.
- Publish uses a stable snapshot. Mutations during publish affect the next publish.
- Reentrant publish is supported and must not corrupt subscription state.
- Core subscribe, unsubscribe, and publish operations are thread-safe.
- Synchronous publish performs zero steady-state heap allocations.
- Exception behavior is explicit: fail-fast and report-and-continue are separate
  policies, not ambiguous type names or global mutable configuration.
- Logging, diagnostics, dispatchers, and exception sinks are injected per instance
  or through optional integration packages. Do not add new global mutable state.
- `async void` handlers are not supported. Async signals use `ValueTask` and
  `CancellationToken`.

## Architecture constraints

- Keep `Xpand.Events.Core` free of Unity and `Microsoft.Extensions.*`
  dependencies.
- Avoid reflection and runtime code generation in Core. Unity IL2CPP and Native
  AOT must remain first-class constraints.
- Do not add a new T4-based generation step. Prefer ordinary source, an incremental
  source generator where generation is genuinely needed, or checked-in generated
  source with deterministic verification.
- Do not optimize from the legacy benchmark results. Establish correct benchmarks
  with reset state, `MemoryDiagnoser`, realistic no-op handlers, and platform
  coverage first.
- Do not put persistence, retries, backpressure, transport protocols, or distributed
  delivery in Core. Add adapters to specialized primitives such as Channels or
  external brokers instead.
- Avoid breaking public APIs after `1.0.0`. Before 1.0, document every breaking
  change and provide a migration note or compatibility facade where practical.

## Target frameworks and distribution

- Core baseline: `netstandard2.0;net8.0` unless a roadmap/ADR changes it.
- Add a direct .NET Framework target only when a real consumer requirement and CI
  test justify it.
- Unity distribution must use a valid UPM layout with `package.json`, runtime and
  test `.asmdef` files, `Runtime`, `Tests`, `Samples~`, `Documentation~`, license,
  and changelog.
- NuGet packages must include license metadata, README, symbols, Source Link, and
  deterministic repository information.
- Use Semantic Versioning. Experimental releases remain pre-1.0 and use prerelease
  suffixes when behavior or API is not stable.

## Testing rules

Every behavior change requires focused tests. Do not rely only on aggregate test
counts.

Core coverage must include:

- zero and generic payloads;
- duplicate and null handlers;
- all priority boundaries and stable ordering;
- disposal idempotency;
- subscribe/unsubscribe during publish;
- reentrancy;
- exception policies and failures inside the exception sink;
- suspension and nested suspension scopes;
- concurrency stress tests;
- weak-reference GC behavior, when weak signals are introduced;
- allocation assertions for publish;
- trimming and Native AOT smoke tests for modern .NET targets.

Unity coverage must include Edit Mode and Play Mode tests, destroyed
`UnityEngine.Object` targets, domain reload on/off, Mono, IL2CPP, and representative
player builds. A feature is not Unity-ready merely because its DLL compiles in the
Editor.

## Working method

1. Read this file, the roadmap, the affected source, and existing tests.
2. State the contract being changed before changing implementation details.
3. Add a regression test that fails for the old behavior.
4. Make the smallest coherent implementation change.
5. Run the narrow tests, then the relevant solution/package/Unity validation.
6. Update documentation, migration notes, and roadmap checkboxes in the same change.
7. Report commands run, results, and any platform matrix not executed locally.

Do not hide a failing or unavailable validation step. Distinguish clearly between
"not run", "failed", and "passed".

## Roadmap summary for agents

### M0: contract and build baseline

Fix correctness defects, specify behavior, replace obsolete build generation,
modernize tests/CI, and produce a clean reproducible pre-release package.

### M1: Core v0.5

Introduce `Signal<T>`, disposable subscriptions, stable priority, thread-safe
snapshot mutation, explicit exception policy, zero-allocation publish, and complete
contract/concurrency tests.

### M2: packaging and compatibility v0.6

Ship validated NuGet packages for `netstandard2.0;net8.0`, Source Link, symbols,
API compatibility checks, trimming/AOT smoke tests, documentation, and migration
facades for legacy `XEvent` APIs.

### M3: Unity v0.7

Ship the UPM package, lifecycle helpers, main-thread dispatcher, Unity-specific
subscription handling, samples, Unity Test Framework suites, and Mono/IL2CPP CI.

### M4: enterprise extensions v0.8

Add async signals, Microsoft logging/DI/diagnostics adapters, testing helpers,
analyzers, and representative server/desktop/IoT integration tests without bloating
Core.

### M5: stable v1.0

Freeze the public contract after compatibility review, performance baselines,
security/supply-chain checks, platform support documentation, release automation,
and successful release-candidate adoption.

Detailed tasks and exit criteria are maintained in [ROADMAP.md](ROADMAP.md).
