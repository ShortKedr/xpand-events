# Async signal contract

`Xpand.Events.Async` provides `AsyncSignal<T>` for handlers that naturally return
`ValueTask`. It is a separate package over `Xpand.Events.Core`; synchronous code
should continue to use `Signal<T>`.

```csharp
var signal = new AsyncSignal<Message>();
using IDisposable subscription = signal.Subscribe(HandleAsync, priority: 10);
await signal.PublishAsync(message, cancellationToken);

static async ValueTask HandleAsync(Message message, CancellationToken cancellationToken) {
    await StoreAsync(message, cancellationToken);
}
```

## Dispatch

- Handlers are awaited sequentially. A later handler does not start until the
  earlier handler's `ValueTask` completes.
- Higher numeric priority runs first; equal priority retains subscription order.
- Publish uses a stable snapshot. Subscribe, disposal, and clear during publish
  affect the next publish.
- The same handler can be registered more than once, and each idempotent token
  owns exactly one registration.
- Operations are thread-safe, but handlers run on the caller's asynchronous
  continuation; the library does not switch to a main or worker thread.
- `async void` is unsupported. The delegate requires `ValueTask` and a
  `CancellationToken`.

## Cancellation

`PublishAsync` checks cancellation before each handler and passes the same token
to the handler and exception sink. Cancellation requested through that token is
control flow: it stops dispatch and is never converted into a reported handler
failure. A handler that ignores cancellation cannot be forcibly interrupted.

## Exceptions

Fail-fast is the default and immediately propagates the first handler failure.
`AsyncSignalOptions.ReportAndContinue` awaits an explicit per-instance exception
sink and then proceeds to the next handler. If both a handler and its sink fail,
an `AggregateException` preserves both failures.

Parallel dispatch is intentionally absent. Its ordering, cancellation,
aggregation, and concurrency-limit decisions require a separately named contract;
see [ADR 0002](decisions/0002-parallel-async-dispatch.md).
