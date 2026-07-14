# Xpand Events roadmap

## Product direction

Xpand Events will become a small, deterministic, AOT-friendly in-process signal
library for Unity and general-purpose C#/.NET applications.

The library should be suitable for gameplay systems, UI/view models, plug-in
callbacks, modular monoliths, desktop applications, workers, and local domain
notifications. It will provide adapters for other ecosystems without turning the
Core package into a framework.

### Non-goals

- Distributed messaging or service-to-service delivery.
- Durable storage, event sourcing, retries, or guaranteed delivery.
- A dependency injection container or application framework.
- A replacement for Channels, Reactive Extensions, MediatR, Kafka, or RabbitMQ.
- Implicit thread switching or hidden asynchronous execution.

## Baseline assessment

The repository currently contains four synchronous event families: normal,
ordered, exception-isolating, and ordered exception-isolating. The prototype has
useful ideas, but its public claims and production guarantees exceed its verified
behavior.

Known baseline problems:

- strong delegates are presented as weak events;
- no thread-safety contract;
- allocation on every invocation;
- priority overflow for negative values;
- inconsistent null bookkeeping;
- global mutable logger and configuration;
- generated arity explosion from T4 templates;
- tests concentrated on zero-argument happy paths;
- invalid state management in subscription benchmarks;
- outdated target frameworks, test dependencies, CI actions, and release scripts;
- no UPM package, Unity assembly definitions, or Unity runtime test matrix;
- incomplete README and package metadata.

The 30 existing NUnit tests pass on the audited checkout. That is a regression
baseline, not evidence of enterprise or Unity readiness.

## Contract decisions

These decisions define the intended v1 contract. Change them only through a short
architecture decision record in `docs/decisions/`.

| Area | Decision |
|---|---|
| Primary abstraction | `Signal<T>` with a single payload |
| Default ownership | Strong subscription with `IDisposable` token |
| Weak ownership | Explicit separate API/package; never implicit |
| Duplicate handler | Allowed; each registration is independent |
| Null handler | `ArgumentNullException` |
| Ordering | Higher `int` priority first; stable insertion order for ties |
| Mutation during publish | Stable snapshot; visible on the next publish |
| Reentrancy | Supported |
| Threading | Core operations are thread-safe; handlers run on publisher thread |
| Dispatch allocation | Zero steady-state heap allocation for synchronous publish |
| Default exception behavior | Fail-fast |
| Isolated exception behavior | Explicit report-and-continue policy |
| Async behavior | Separate `AsyncSignal<T>` using `ValueTask` and cancellation |
| Global state | None in Core |
| Core dependencies | BCL only |

## Proposed package layout

```text
src/
  Xpand.Events.Core/
  Xpand.Events.Async/
  Xpand.Events.Extensions.Microsoft/
  Xpand.Events.Testing/
  Xpand.Events.Analyzers/
tests/
  Xpand.Events.Core.Tests/
  Xpand.Events.Core.StressTests/
  Xpand.Events.AotSmokeTests/
benchmarks/
  Xpand.Events.Benchmarks/
unity/
  com.xpand.events/
    package.json
    Runtime/
    Tests/Editor/
    Tests/Runtime/
    Samples~/
    Documentation~/
docs/
  decisions/
```

The exact migration may be incremental, but dependency direction must remain:

```text
Unity / Microsoft extensions / Async / Testing / Analyzers
                         -> Core
```

Core must never reference an integration package.

## M0: contract and build baseline

Goal: make the existing repository honest, reproducible, and safe to evolve.

- [x] Correct README claims about weak references, threading, async, and Unity.
- [x] Document current semantics for duplicate handlers, ordering, mutation during
      dispatch, suspension, and exceptions.
- [x] Fix ordered priority overflow across the full `int` range.
- [x] Reject null listeners consistently and repair list/cache invariants.
- [x] Add `Clear` and `Count` to the legacy implementation if it remains public.
- [x] Add regression tests for all known correctness defects.
- [x] Update the test project to a supported .NET target and current test packages.
- [x] Remove `TransformOnBuild` and T4 from the normal build/pack path.
- [ ] Make `dotnet restore`, `build`, `test`, and `pack` complete on Windows, macOS,
      and Linux CI. The matrix workflow is implemented; its first GitHub-hosted
      run is still pending. As of 2026-07-14, the remote repository exposes only
      an obsolete failed `.NET` run from 2025; the current matrix is not pushed.
- [x] Fix package versioning, license metadata, README inclusion, symbols, and the
      release artifact filename.
- [x] Replace obsolete GitHub Actions and remove hard-coded external package feed
      ownership.
- [x] Mark the next release as prerelease.

Exit criteria:

- clean cross-platform restore/build/test/pack;
- all known correctness bugs have regression tests;
- package metadata passes NuGet validation without unexplained warnings;
- documentation accurately describes only verified behavior.

## M1: Core v0.5

Goal: replace the prototype storage engine with a production-grade synchronous
signal core.

- [x] Introduce `Signal<T>` and a no-payload convenience abstraction.
- [x] Return an idempotent `IDisposable` from `Subscribe`.
- [x] Allow independent duplicate registrations.
- [x] Implement stable priority using direct priority comparison and a monotonic
      subscription sequence.
- [x] Implement thread-safe copy-on-write or equivalent snapshot storage.
- [x] Ensure handler execution occurs outside mutation locks.
- [x] Guarantee zero steady-state allocation during `Publish`.
- [x] Define and implement fail-fast and report-and-continue exception policies.
- [x] Handle failures in the exception sink without recursive logging or corrupted
      state.
- [x] Replace global configuration with immutable per-instance options for the
      primary `Signal` API; legacy `XEvent` static configuration remains only for
      compatibility.
- [x] Add scoped and nest-safe suspension behavior.
- [x] Add complete XML documentation and nullable annotations to the primary
      `Signal` API.
- [x] Add contract, concurrency, reentrancy, and allocation tests.
- [x] Add benchmarks with isolated event overhead, reset state, and
      `MemoryDiagnoser`.

Exit criteria:

- the behavioral contract table is covered by automated tests;
- concurrent mutation stress tests run repeatedly without corruption or deadlock;
- synchronous publish allocates zero bytes after warm-up;
- benchmark results are reproducible and include default .NET events as a baseline.

## M2: packaging and compatibility v0.6

Goal: produce a professional cross-industry .NET library package.

- [x] Target `netstandard2.0;net8.0` in Core.
- [x] Remove `net471` unless a documented consumer and CI job justify it.
- [x] Enable deterministic builds, repository metadata, Source Link, and symbol
      packages.
- [x] Add package README, SPDX license expression, icon, useful tags, and release
      notes.
- [x] Enable NuGet package validation and maintain a public API compatibility
      baseline.
- [x] Add trimming analysis and a trimmed application smoke test.
- [x] Add a Native AOT smoke-test application.
- [x] Decide and document strong-name policy before 1.0. Assemblies remain
      unsigned unless a verified consumer requirement and key-management policy
      justify changing the decision before 1.0.
- [x] Provide compatibility facades for legacy `XEvent` types where practical by
      retaining them in the separate `Xpand.Events` compatibility package.
- [x] Publish a migration guide from `AddListener`/`RemoveListener` to subscription
      tokens.
- [x] Establish SemVer and deprecation policy.
- [x] Add and run a modern .NET consumer through a `netstandard2.0` library.

Exit criteria:

- a consumer sample succeeds on modern .NET and through `netstandard2.0`;
- package validation and public API compatibility checks pass in CI;
- trimming and Native AOT smoke tests pass without unexplained warnings;
- symbols navigate to the exact repository commit.

## M3: Unity v0.7

Goal: ship a Unity-native package rather than an unverified generic DLL.

- [x] Create `com.xpand.events` with a valid UPM manifest and package layout.
- [x] Add runtime, Editor-test, and PlayMode-test assembly definitions.
- [x] Track required Unity `.meta` files.
- [x] Add `CompositeSubscription` and lifecycle binding helpers.
- [x] Add explicit `OnEnable`/`OnDisable` and `OnDestroy` usage patterns.
- [x] Handle destroyed `UnityEngine.Object` subscription targets predictably.
- [x] Add a main-thread dispatcher without changing Core handler-thread semantics.
- [x] Add optional ScriptableObject signal channels in a separate Unity namespace.
- [x] Test domain reload enabled and disabled on Unity 6000.3.19f1.
- [x] Add and run Edit Mode and Play Mode suites on Unity 6000.3.19f1.
- [x] Build and run the standalone smoke test with Mono and IL2CPP on macOS arm64.
- [x] Add representative macOS standalone, Android ARM64 IL2CPP, and WebGL IL2CPP
      player builds. The standalone players were run locally; Android/WebGL runtime
      execution and iOS remain for suitable device/browser/CI infrastructure.
- [x] Profile allocations and dispatch cost in Unity Player, not only the Editor.
      Mono measured zero managed bytes over 1,000,000 steady-state publishes;
      IL2CPP timing was captured, but its per-thread managed allocation API is not
      implemented by Unity.
- [x] Add samples for gameplay, UI, lifecycle disposal, ordering, and error policy.
- [x] Publish Unity-specific documentation and support matrix.

Exit criteria:

- UPM installation from a Git tag works in a clean Unity project;
- Edit Mode and Play Mode tests pass in the minimum supported Unity version;
- representative IL2CPP player builds and runtime smoke tests pass;
- normal publish is allocation-free in a Unity Player after warm-up;
- destroyed-object behavior is documented and tested.

## M4: enterprise extensions v0.8

Goal: support server, desktop, plug-in, industrial, and test environments without
adding dependencies to Core.

- [x] Add `AsyncSignal<T>` with sequential dispatch, cancellation, and explicit
      exception policy.
- [x] Evaluate parallel async dispatch as a separate opt-in policy. ADR 0002
      keeps it out of v0.8 until concurrency, cancellation, ordering, and
      aggregation semantics are justified by a verified use case.
- [x] Add `Microsoft.Extensions.DependencyInjection` registration helpers.
- [x] Add `Microsoft.Extensions.Logging` exception/diagnostic sink.
- [x] Add `ActivitySource` and metrics integration without per-publish allocation
      when diagnostics are disabled.
- [x] Add adapters for `IObservable<T>` and `System.Threading.Channels` where their
      semantics can be represented honestly.
- [x] Add deterministic testing helpers and handler probes.
- [x] Add analyzers for undisposed subscriptions, `async void`, static long-lived
      publishers, and unsafe Unity lifecycle patterns.
- [x] Add representative worker service, desktop synchronization-context, and
      bounded-channel samples.
- [x] Document where durable or distributed messaging must replace this library.

Exit criteria:

- each extension is independently consumable and optional;
- Core dependency graph remains BCL-only;
- async cancellation and exception semantics have full contract tests;
- integrations include end-to-end samples and do not misrepresent delivery
  guarantees.

## M5: stable v1.0

Goal: freeze a supportable public contract and production release process.

- [x] Complete public API and binary compatibility review.
- [x] Resolve all planned pre-1.0 breaking changes.
- [x] Publish performance and allocation baselines for .NET and Unity Player.
- [x] Define supported .NET and Unity version policy.
- [x] Add dependency review, vulnerability scanning, SBOM, provenance, and package
      signing as appropriate for public distribution. NuGet audit passed locally;
      the first external SBOM/provenance attestation awaits an authorized tag.
- [x] Add contribution, security, support, and release documentation.
- [x] Automate changelog, package, UPM tag, NuGet, and GitHub release creation.
      Workflows are configured; their first authorized tag run remains pending.
- [x] Run release-candidate adoption in at least one real Unity project and one
      non-Unity .NET application. The local `1.0.0-rc.1` candidate passed in
      `UniXMerge` (Unity 6000.3.19f1, 11/11 Edit Mode tests) and `spl-pl`
      (`net8.0`, clean Release build and real application run); exact artifact
      identity and remaining tagged-release gates are recorded in
      `docs/release-candidate-adoption.md`.
- [ ] Publish `1.0.0` only after the release candidate meets every exit criterion.

Exit criteria:

- no unresolved critical correctness, lifecycle, threading, AOT, or packaging issue;
- documented support matrix is continuously verified by CI;
- upgrade from the latest prerelease is documented and tested;
- release artifacts are reproducible and traceable to a signed/tagged commit.

## Work priority

Unless a user directs otherwise, contributors and agents should work in this order:

1. correctness and contract tests;
2. build and package reproducibility;
3. Core architecture and allocation behavior;
4. Unity lifecycle and IL2CPP verification;
5. optional integrations;
6. additional features.

Do not skip an earlier milestone by adding an attractive integration to unstable
Core behavior. Update this file when a task is completed, deferred, or replaced by
an ADR.
