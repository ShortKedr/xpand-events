# Current legacy event semantics

This document describes the behavior of the current `XEvent` prototype. It is
not the planned `Signal<T>` contract. The production contract and the work
required to implement it are tracked in [ROADMAP.md](../ROADMAP.md).

The rules below apply to all generated arities of the four public event
families:

- `XEvent` and `XEvent<T...>`;
- `OrderedXEvent` and `OrderedXEvent<T...>`;
- `SafeXEvent` and `SafeXEvent<T...>`;
- `SafeOrderedXEvent` and `SafeOrderedXEvent<T...>`.

## Subscriptions and duplicate handlers

`AddListener` stores a strong reference to the delegate. A subscriber remains
reachable until it is explicitly removed or the event itself becomes
unreachable.

Delegate equality identifies a subscription. Adding an equal delegate for a
second time returns `false` and does not create another registration. A
successful first registration returns `true`. `Contains` uses the same delegate
equality, and `RemoveListener` removes that one registration and returns whether
it was found. `Count` reports the current number of registrations. `Clear()`
removes every registration and may be called repeatedly.

This differs from the planned `Signal<T>` API, where duplicate registrations
will be independent and identified by disposable subscription tokens.

Passing `null` to `AddListener` throws `ArgumentNullException` in every event
family and does not change the subscription collection.

## Invocation order

Unordered events invoke listeners in registration order. Removing and then
adding a listener again places it at the end.

Ordered events invoke higher numeric priorities first. Listeners with the same
priority run in registration order. The full `int` range is supported, including
`int.MinValue` and `int.MaxValue`.

Every invocation allocates a new array containing the listeners in invocation
order.

## Mutation during invocation and reentrancy

`Invoke` copies the current listeners to an array before calling the first
listener. Adding or removing listeners from a handler does not change that
in-progress invocation:

- a listener added by a handler first runs on the next invocation;
- a listener removed by a handler can still run later in the current invocation;
- clearing the event from a handler does not stop listeners in the current
  snapshot;
- the next invocation observes these changes.

A reentrant `Invoke` creates a new snapshot. It therefore observes mutations
made before the nested call. Reentrancy is not comprehensively tested in the
prototype and must not be interpreted as a production guarantee.

These rules apply only to single-threaded use. The collections, suspension flag,
logger, and global configuration are not thread-safe.

## Suspension

`Suspend()` sets one Boolean flag, and `Unsuspend()` clears it. Calling `Invoke`
while the flag is set returns without creating a snapshot or calling listeners.

Suspension is not scoped, counted, or nest-safe. Multiple `Suspend()` calls are
cleared by one `Unsuspend()` call. Changing suspension from a handler does not
stop the current snapshot, but it affects a reentrant or later invocation.

## Exceptions

`XEvent` and `OrderedXEvent` use fail-fast behavior. A listener exception escapes
from `Invoke`, and listeners later in the snapshot are not called.

`SafeXEvent` and `SafeOrderedXEvent` catch each listener exception and pass it to
the static `XEventLogger`:

- `ImplicitException` is invoked for every caught exception regardless of
  `XpandEventsConfig.LogLevel`;
- `Exception` is invoked only when `LogLevel` equals `LogLevel.Exception`;
- after reporting succeeds, invocation continues with the next listener.

Exceptions thrown by either logger callback are not caught. They escape from
`Invoke` and prevent later listeners from running. The logger subscriptions and
`XpandEventsConfig.LogLevel` are global mutable state.
