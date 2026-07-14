# Versioning and deprecation policy

Xpand Events uses Semantic Versioning for its NuGet and future UPM packages.

## Before 1.0

Versions below `1.0.0` are prereleases while the contract and platform matrix are
being established. A minor-version change may contain a breaking API or behavior
change. Every such change must be called out in release notes and include a
migration note or compatibility facade where practical.

Prerelease suffixes identify maturity, for example `0.6.0-alpha.1`,
`0.7.0-beta.1`, and `1.0.0-rc.1`. Published package versions are immutable and are
never rebuilt under the same version.

## From 1.0 onward

- Patch releases contain backward-compatible bug fixes and documentation or
  packaging corrections.
- Minor releases add backward-compatible functionality and may deprecate APIs.
- Major releases may remove deprecated APIs or make documented breaking changes.

Public API compatibility includes source-visible API, binary assembly identity,
documented behavior, exception policy, ordering, threading, and package target
frameworks. A target-framework removal or signing-identity change is treated as a
breaking change.

## Deprecation

Deprecated public APIs use `ObsoleteAttribute` with a replacement or migration
link. Except for critical security or correctness issues, an API remains available
for at least one minor release after deprecation and is removed only in a major
release after 1.0.

The legacy `Xpand.Events` package is a compatibility package. It remains separate
from `Xpand.Events.Core`; its eventual deprecation and removal will be announced
in release notes and the migration guide. No removal date is claimed yet.

Security fixes may require an accelerated change. When compatibility cannot be
preserved safely, the advisory and release notes must explain the break and the
supported upgrade path.
