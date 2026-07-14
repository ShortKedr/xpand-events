# Support

Use GitHub Issues for reproducible bugs and focused feature proposals. Use
Discussions, if enabled, for design questions and usage help. Security issues
must follow `SECURITY.md` rather than a public issue.

Before reporting a bug, verify it on the latest patch of a supported runtime and
include:

- Xpand Events package/version and installation source;
- .NET SDK/runtime or exact Unity editor/backend/platform;
- minimal reproduction;
- expected and actual behavior plus complete exception/stack trace;
- whether trimming, Native AOT, IL2CPP, domain reload, or concurrency is involved.

Support is best-effort and has no guaranteed response or resolution SLA. The
tested matrix is defined in `docs/support-policy.md`. Environments marked “not
run” or “not supported” are welcome as useful reports, but are not treated as
verified regressions until reproduced on the supported matrix.

Xpand Events does not provide operational support for brokers, distributed
delivery, persistent queues, retries, database outboxes, or application-specific
thread dispatch. See `docs/delivery-boundaries.md` for the ownership boundary.
