# Migrating from `XEvent` to `Signal`

The `Xpand.Events` package keeps the legacy `XEvent` families for incremental
migration. New code should reference `Xpand.Events.Core` and use `Signal` or
`Signal<T>`.

## Basic subscription

Legacy listeners are removed by delegate identity:

```csharp
var changed = new XEvent<string>();
changed.AddListener(HandleChanged);
changed.Invoke("ready");
changed.RemoveListener(HandleChanged);
```

The new API gives every registration its own idempotent lifetime token:

```csharp
var changed = new Signal<string>();
using IDisposable subscription = changed.Subscribe(HandleChanged);
changed.Publish("ready");
```

Keep the token for as long as the listener should be active and dispose it during
the owning component's shutdown. Disposing the same token more than once is safe.
Unlike `XEvent`, subscribing the same delegate more than once creates independent
registrations; each token removes exactly one registration.

## No payload and multiple arguments

Replace `XEvent` with `Signal`:

```csharp
var refreshed = new Signal();
using IDisposable subscription = refreshed.Subscribe(Refresh);
refreshed.Publish();
```

`Signal<T>` intentionally has one payload. Replace multi-argument events with a
named payload that can evolve without adding another generated event arity:

```csharp
public sealed class DamageMessage {
    public DamageMessage(int amount, string source) {
        Amount = amount;
        Source = source;
    }

    public int Amount { get; }
    public string Source { get; }
}

var damaged = new Signal<DamageMessage>();
damaged.Publish(new DamageMessage(10, "trap"));
```

## Ordered events

Priority is part of `Subscribe`; a separate `OrderedXEvent` type is unnecessary:

```csharp
using IDisposable first = signal.Subscribe(Validate, priority: int.MaxValue);
using IDisposable second = signal.Subscribe(Update, priority: 0);
using IDisposable last = signal.Subscribe(Audit, priority: int.MinValue);
```

Higher numeric priority runs first. Equal priority retains subscription order,
and the full `int` range is supported.

## Exception-isolating events

`Signal` is fail-fast by default. Replace `SafeXEvent` or `SafeOrderedXEvent` with
an explicit per-instance report-and-continue policy:

```csharp
var signal = new Signal<DamageMessage>(
    SignalOptions.ReportAndContinue(ReportListenerFailure));
```

The sink receives each listener exception and dispatch continues. If both a
listener and the sink fail, `Publish` throws an `AggregateException` containing
both failures. There is no new global logger or mutable exception policy.

## Suspension and clearing

Legacy suspension uses separate `Suspend` and `Unsuspend` calls. The new API uses
a nest-safe scope:

```csharp
using (signal.Suspend()) {
    // Publish calls are ignored while this scope is active.
}
```

`Clear()` removes all current registrations. An already-running publish keeps its
stable snapshot; mutations are observed by the next publish.

## Behavioral differences to account for

- `Signal` operations are thread-safe; handlers still execute synchronously on
  the publishing thread.
- Handler invocation occurs outside mutation locks, and reentrant publishing is
  supported.
- A null handler throws `ArgumentNullException`.
- `Publish` performs no steady-state heap allocation after warm-up.
- Subscriptions are strong references. Weak subscriptions and `async void`
  handlers are not supported.
- The compatibility package retains legacy behavior; adding it does not upgrade
  an `XEvent` instance to the `Signal` contract.
