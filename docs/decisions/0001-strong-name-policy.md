# ADR 0001: strong-name policy

- Status: Accepted
- Date: 2026-07-14

## Context

Strong naming provides an assembly identity for consumers that require signed
assemblies, but it is not a security boundary or a substitute for package signing.
Introducing, removing, or changing a strong-name key changes assembly identity and
can break binding for existing consumers. The project does not currently have a
verified consumer requirement for strong-named assemblies or an established
private-key custody and rotation process.

## Decision

`Xpand.Events.Core` and the `Xpand.Events` compatibility assembly remain unsigned
through the pre-1.0 releases. We will not delay Core or Unity compatibility by
adding a key without a concrete consumer requirement.

Before `1.0.0`, the compatibility review must either confirm this unsigned policy
for 1.x or replace this ADR with a signed-assembly policy. A change to signing
status after 1.0 is treated as an assembly-identity compatibility break and
therefore requires a new major version.

If strong naming is adopted, the replacement ADR must define:

- the consumer and runtime requirement;
- key custody, backup, access, and rotation;
- public-key verification in CI;
- package and assembly compatibility tests;
- the migration path for already published unsigned packages.

## Consequences

- Consumers that require strong-named dependencies cannot use the prerelease
  packages without an explicit downstream workaround.
- Package authenticity is handled separately through repository provenance,
  package signing, and release controls planned for M5.
- The project avoids committing an unmanaged signing key or implying that a
  strong name authenticates the publisher.
