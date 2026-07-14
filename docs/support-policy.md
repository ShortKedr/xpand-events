# Supported versions and platforms

This policy separates package compatibility from environments the project has
actually validated. A runtime being able to consume `netstandard2.0` does not by
itself make that runtime a supported platform.

## .NET packages

| Area | Policy |
|---|---|
| Target frameworks | `netstandard2.0` and `net8.0` |
| Directly tested runtime | Latest servicing patch of .NET 8 on Windows, macOS, and Linux CI |
| Native AOT / trimming | Latest .NET 8 servicing SDK in the Linux smoke job; local macOS arm64 AOT snapshot |
| Language baseline | C# 8 for shipped libraries |
| .NET Framework | No direct target or support claim |
| Later .NET releases | May consume a compatible asset; support is claimed only after a dedicated CI consumer is added |

.NET 8 is an LTS release supported by Microsoft through 2026-11-10. The project
requires consumers to use a currently serviced patch. Before that date, the
project must add and validate a supported successor runtime; removing `net8.0`
or `netstandard2.0` from a stable package is a breaking change.

The authoritative lifecycle is the
[Microsoft .NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy).

## Unity package

The minimum supported editor family is Unity 6000.3 LTS, with the latest patch
recommended. Local validation currently uses 6000.3.19f1 on macOS arm64. Unity
states that Unity 6.3 LTS is supported through December 2027; the authoritative
schedule is the
[Unity 6 release support page](https://unity.com/releases/unity-6/support).

| Unity environment | Support status |
|---|---|
| Editor import, Edit Mode, Play Mode | Validated on 6000.3.19f1 macOS arm64 |
| Domain reload enabled and disabled | Validated |
| macOS arm64 Mono and IL2CPP player | Build and runtime smoke validated |
| Android ARM64 IL2CPP | Build validated; runtime not yet supported |
| WebGL IL2CPP | Build validated; runtime not yet supported |
| iOS, Windows player, Linux player | Not yet supported or validated |

An editor or platform becomes supported only after its package import, relevant
tests, representative player build, and runtime smoke pass in repeatable CI or
documented local validation. Patch releases within the minimum Unity LTS family
are expected to remain compatible, but regressions must be reproduced on the
latest patch before being classified as library defects.

## Support window and changes

- Security and critical correctness fixes target supported package lines.
- Preview SDKs, preview Unity editors, and unsupported runtimes are best-effort
  and must not block a stable patch release unless the issue also reproduces on
  the supported matrix.
- Adding a target framework or platform is backward-compatible. Removing one,
  raising the Unity minimum, changing assembly identity, or dropping a documented
  runtime guarantee requires the versioning process in `docs/versioning.md`.
- Every release records the exact CI and local matrix that passed; unexecuted
  platforms are reported as “not run,” never inferred.

The first GitHub-hosted cross-platform run and licensed Unity CI run are still
pending. Until those runs are green, this file is the intended 1.0 support policy,
not evidence that every listed CI row has already passed externally.
