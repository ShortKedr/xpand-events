# Supply-chain security

## Dependency controls

The `Security` workflow performs two independent checks:

- GitHub dependency review rejects pull requests that introduce a dependency
  with a known vulnerability of low severity or higher.
- NuGet restore audits direct and transitive packages with
  `NuGetAuditMode=all`; NU1901 through NU1904 are errors on pushes, pull requests,
  the weekly schedule, and manual runs.

The same NuGet audit command passed locally on 2026-07-14 with no reported
advisories. A clean local result is a snapshot; scheduled CI is required because
advisory data changes after a package is published.

## SBOM and provenance

Release builds install the pinned CycloneDX .NET tool 6.2.0 and generate a JSON
SBOM for each of the six package projects, excluding development-only packages.
This prevents dependency-bearing extension packages from contaminating the
dependency-free Core inventory. SBOMs are uploaded beside NuGet artifacts.

`actions/attest@v4` creates two keyless attestations for package artifacts:

- SLSA build provenance tied to each `.nupkg` and `.snupkg` digest;
- a package-specific CycloneDX SBOM attestation tied to each runtime `.nupkg`
  digest.

GitHub mints a short-lived OIDC/Sigstore certificate for the workflow. Consumers
can verify a downloaded package against this repository with `gh attestation
verify`. An attestation proves which workflow produced an exact artifact; it does
not prove the package is defect-free.

## Package signing decision

NuGet author signing is not enabled until the project has a documented
certificate owner, protected signing service, rotation/revocation process, and
continuity plan. Committing a `.pfx`, storing a long-lived certificate in an
ordinary repository secret, or creating an unmaintained self-signed identity
would weaken rather than improve the release process.

For the current public release plan, keyless GitHub attestations provide
verifiable build identity and NuGet.org supplies repository signing after
publication. If a consumer requires NuGet author signatures, that requirement
must be resolved before 1.0 publication and documented as an assembly/package
operations decision; signatures must never be added after publication by
rebuilding the same immutable version.

The first external workflow run and attestation verification remain pending
until a release tag is authorized and pushed.
