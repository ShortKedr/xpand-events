# Changelog

## [Unreleased]

## [1.0.0-rc.1] - 2026-07-14

- Rebuilt synchronous Core around thread-safe, allocation-free `Signal<T>`.
- Added Async, Microsoft Extensions, Testing, Analyzers, and Unity packages.
- Retained legacy `XEvent` APIs in a separate compatibility package and added a
  migration guide.
- Added deterministic NuGet/UPM packaging, API compatibility, trimming, Native
  AOT, Unity Player, security, SBOM, provenance, and release validation.
- Validated the candidate in real Unity and non-Unity .NET consumers.
