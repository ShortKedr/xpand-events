# Release-candidate adoption

## 2026-07-14: local `1.0.0-rc.1` candidate

The candidate passed adoption in one real .NET application and one real Unity
project. Both consumers were copied to a temporary directory before integration;
their original working trees were not modified.

After all package metadata was prepared as `1.0.0-rc.1`, the exact NuGet
candidate was rebuilt with:

```bash
dotnet pack Xpand-Events.sln -c Release --no-build --no-restore \
  -o /tmp/xpand-rc-final-20260714/packages \
  -m:1 --disable-build-servers
```

The Core package used by the .NET consumer had SHA-256
`22697a5545bddf5a113057401a0534c9cc3b0a943f0705562572fa531d878839`.
The sorted file-hash manifest for the UPM snapshot had SHA-256
`8c66ebf5b2ef73fa5da9cffc57a487b1d150e197410382c2c2015302bc03a81c`.

This candidate was built from the current uncommitted worktree based on commit
`12d81f33737f292a59f34f28d349d3cb6bb9706a`. These local hashes identify the
tested artifacts, but do not replace the signed/tagged provenance required for a
public release.

### Non-Unity .NET application

- Consumer: `spl-pl`, a real `net8.0` compiler console application, at commit
  `0cf174de7dd03cf931241a043d3607ffd17941b1`.
- Integration: referenced `Xpand.Events.Core` `1.0.0-rc.1` from the temporary
  local feed through an isolated NuGet package cache and published the
  application's lexing and compilation stages.
- Contract exercised: payload delivery, full priority ordering, and idempotent
  subscription disposal.
- Result: restore passed, Release build passed with zero warnings and zero
  errors, and the real application run exited with code 0 after printing
  `XPAND_EVENTS_RC_ADOPTION_PASSED`.

### Unity project

- Consumer: `UniXMerge`, a real Unity editor extension project. The source
  snapshot was not yet committed in its own repository, so no consumer commit
  identifier is available.
- Environment: Unity `6000.3.19f1` on macOS arm64.
- Integration: installed the temporary `com.xpand.events` `1.0.0-rc.1` package
  through a local UPM dependency.
- Contract exercised: `int.MinValue`/`int.MaxValue` priority ordering,
  subscription identity and idempotent disposal, plus automatic removal of a
  subscription whose `UnityEngine.Object` owner was destroyed.
- Result: the complete consumer Edit Mode suite passed 11/11 tests. The two
  candidate-specific adoption tests passed 2/2.

## Remaining release gates

Adoption does not authorize publication. Before `1.0.0`, the exact committed and
tagged candidate must pass the GitHub-hosted Windows/macOS/Linux matrix, produce
traceable provenance and SBOM attestations, and be verified from its published
NuGet and Git-tag UPM locations. Upgrade from that exact prerelease to the stable
package must also be documented and tested.
