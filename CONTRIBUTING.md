# Contributing

Xpand Events is intentionally a small in-process signal library. Proposals for
distributed transport, persistence, retries, event sourcing, or a general
mediator framework are outside Core's scope; adapters should preserve the
semantics of the specialized primitive they connect.

## Before changing code

1. Read `AGENTS.md`, `ROADMAP.md`, the affected contract document, and existing
   tests.
2. Open an issue for a new public API, behavioral contract change, target
   framework change, or package-boundary change before investing in a large PR.
3. Use an ADR under `docs/decisions/` when a decision changes the contract table
   or creates a long-lived compatibility constraint.

## Pull requests

- Keep each PR focused on one coherent behavior or roadmap item.
- Add a regression test that fails before a bug fix and passes afterward.
- Preserve thread safety, stable snapshots, reentrancy, all `int` priorities,
  idempotent tokens, and zero steady-state synchronous publish allocation.
- Do not add Unity or `Microsoft.Extensions.*` dependencies to Core.
- Do not add T4, runtime code generation, reflection-dependent dispatch, global
  mutable configuration, or `async void` handlers.
- Update XML docs, user documentation, migration notes, public API baselines,
  package changelog, and roadmap status in the same PR where applicable.
- State which .NET, Unity, AOT, trimming, and platform validations were passed,
  failed, or not run.

Run the local .NET checks from the repository root:

```shell
dotnet restore Xpand-Events.sln
dotnet build Xpand-Events.sln --configuration Release --no-restore
dotnet test Xpand-Events.sln --configuration Release --no-build --no-restore
dotnet pack Xpand-Events.sln --configuration Release --no-build --no-restore --output artifacts
```

Unity changes also require the relevant Edit Mode, Play Mode, domain-reload, and
player validation described in `unity/ValidationProject/README.md`. Do not claim
a platform based only on Editor compilation.

By submitting a contribution, you agree that it is licensed under this
repository's BSD-2-Clause license.
