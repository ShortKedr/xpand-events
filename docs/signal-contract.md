# Signal contract

`Signal<T>` is the payload-oriented synchronous API. `Signal` is the equivalent
no-payload convenience type. Both APIs are prerelease and execute listeners on
the thread that calls `Publish`.

```csharp
var signal = new Signal<string>();
using var subscription = signal.Subscribe(
    message => Console.WriteLine(message),
    priority: 10);

signal.Publish("ready");
```

## Subscriptions and ordering

`Subscribe` rejects a null handler with `ArgumentNullException` and returns an
idempotent `IDisposable`. Equal delegates may be registered repeatedly. Every
token identifies and removes exactly one registration.

Higher numeric priority runs first across the complete `int` range. Equal
priority preserves the order in which subscriptions acquired the signal's
mutation lock.

## Publishing and mutation

Subscribe, dispose, `Clear`, `Suspend`, and `Publish` are thread-safe. A publish
reads one immutable snapshot and invokes its handlers without holding the
mutation lock. Consequently:

- mutations during a publish affect the next publish;
- a removed handler can still run in the current snapshot;
- reentrant publish reads the latest completed snapshot;
- synchronous publish performs no steady-state heap allocation after warm-up.

Handlers are never moved to another thread implicitly.

## Exceptions

The default `SignalOptions.FailFast` policy immediately propagates the first
listener exception and stops that publish.

`SignalOptions.ReportAndContinue(exceptionSink)` reports each listener failure
to the supplied per-instance sink and then invokes the next listener. If the
sink itself fails, publishing stops with an `AggregateException` containing the
listener failure followed by the sink failure. The sink failure is not reported
recursively, and subscription state is unchanged.

## Suspension

`Suspend()` returns an idempotent scope. Suspension scopes are counted, so every
active scope must be disposed before publishing resumes. Suspending from a
handler does not interrupt the current snapshot, but it blocks reentrant and
later publishes until the scope is disposed.

## Legacy API

The older `XEvent` families remain compatibility prototypes. Their documented
single-threaded behavior and static logging configuration have not changed.
See [Current legacy event semantics](legacy-semantics.md).
