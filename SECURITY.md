# Security policy

## Supported versions

Security fixes target the supported release lines and platform matrix in
`docs/support-policy.md`. The repository is currently prerelease; no stable 1.x
line has been published yet.

## Reporting a vulnerability

Do not disclose a suspected vulnerability in a public issue. Use the repository's
[private security advisory form](https://github.com/ShortKedr/xpand-events/security/advisories/new)
and include:

- affected package and version;
- impact and realistic attack or failure scenario;
- minimal reproduction or proof of concept;
- affected runtimes/platforms;
- known mitigations.

If private reporting is unavailable, open a public issue containing no exploit
details and ask the maintainer to establish a private channel. Do not attach
secrets, credentials, private projects, or unpublished exploit code publicly.

The maintainer will acknowledge receipt when available, validate scope, and
coordinate disclosure and remediation. No guaranteed response SLA is offered.
Security releases may make an otherwise breaking change when compatibility
cannot be preserved safely; the advisory and migration path must explain it.

## Scope

Relevant reports include signal-state corruption, unexpected code execution,
unsafe deserialization introduced by this project, package/release tampering,
dependency vulnerabilities, and lifecycle defects that cross a documented trust
or ownership boundary. Ordinary application exceptions, undisposed user-owned
subscriptions, and lack of distributed/durable delivery are documented behavior,
not security vulnerabilities by themselves.
