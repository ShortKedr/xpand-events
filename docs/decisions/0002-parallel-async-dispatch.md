# ADR 0002: parallel async dispatch remains a separate future primitive

- Status: Accepted
- Date: 2026-07-14

## Context

Parallel handler execution changes stable ordering from an execution guarantee to
only a start-order hint. It also requires new choices for cancellation, partial
completion, exception aggregation, concurrency limits, and whether one handler
may observe another handler's effects. Hiding those choices behind a boolean on
`AsyncSignal<T>` would make its contract ambiguous.

## Decision

`AsyncSignal<T>` dispatches sequentially and awaits each `ValueTask` before
starting the next handler. Parallel dispatch is not included in v0.8.

If a verified use case requires it, it will be designed as a separately named
primitive or policy with explicit maximum concurrency, exception aggregation,
cancellation, and ordering documentation. Applications can use Channels or their
own task orchestration when those semantics are already appropriate.

## Consequences

- Priority and equal-priority ordering remain observable and deterministic.
- Cancellation can stop before the next handler starts.
- A slow handler delays later handlers by design.
- The current API does not imply backpressure, queueing, retries, or durable work.
