# Release runbook

Only a maintainer with NuGet environment approval may publish. Published versions
are immutable; never rebuild or replace an existing version.

## Release candidate

1. Dispatch `Prepare release` with the candidate version. Review its PR, which
   updates every NuGet/UPM version and both changelogs from one input.
2. Confirm every intended change is documented and all PublicAPI/analyzer rule
   files are current.
3. Run clean restore, Release build, all .NET tests, pack, NuGet audit, trimming,
   Native AOT, and all supported integration samples.
4. Run Unity import, Edit Mode, both Play Mode domain-reload configurations,
   supported player builds/runtime smokes, and allocation validation. Follow the
   dedicated ephemeral runner contract in
   [`unity-ci-runner.md`](unity-ci-runner.md) for external CI.
5. Inspect every `.nupkg`, `.snupkg`, and package-specific SBOM. Confirm Core has
   no runtime dependency and analyzer compiler dependencies are private.
6. Run adoption in one real Unity project and one non-Unity application; record
   project/version, platform, result, and owner approval without committing
   proprietary consumer content.
7. Dispatch the `Release` workflow with the RC version from the reviewed branch.
   Confirm its non-publishing build, pack, SBOM, provenance, and artifact upload
   jobs pass; verify downloaded Core and UPM artifacts with `gh attestation
   verify` restricted to the release workflow and source ref.
8. Merge the preparation PR, let required CI finish, and create an annotated
   `v1.0.0-rc.N` tag only after all required checks are green.
9. The tag workflow builds NuGet/UPM artifacts once, publishes NuGet packages,
   and creates the GitHub release. Verify provenance and SBOM attestations with
   `gh attestation verify`,
   then approve the protected NuGet environment.

## Stable 1.0

Promote only an adopted release candidate with no unresolved critical issue.
Repeat the validation on the exact stable commit, update all version/changelog
metadata to `1.0.0`, and create the stable tag. The release workflow must build
once, publish those exact artifacts to NuGet, create the matching UPM tag/release,
and attach checksums/SBOMs. Do not publish first and tag later.

Run `scripts/test-stable-upgrade.sh` and follow
[`upgrading-1.0.md`](upgrading-1.0.md) first with local candidate artifacts, then
repeat the consumer check with the published RC and exact proposed stable
artifacts.

## Verification and rollback

- Install each NuGet package from NuGet.org into a clean consumer and install UPM
  from the release tag/subpath.
- Verify symbols resolve to the tagged commit and attestations match downloads.
- Verify the GitHub release contains the same checksums as published artifacts.
- NuGet and Git tags are immutable. If a release is defective, deprecate the
  package when appropriate, publish a new patch version, document the issue, and
  never overwrite or force-move the tag.

The workflow is not authorization to tag or publish by itself. Creating tags,
approving environments, and publishing externally require explicit maintainer
action.
