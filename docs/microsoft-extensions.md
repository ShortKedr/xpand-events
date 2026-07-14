# Microsoft extensions

`Xpand.Events.Extensions.Microsoft` is optional. It references Core and Async;
neither Core nor the Unity package references `Microsoft.Extensions.*`.

## Dependency injection

```csharp
services.AddSignal<OrderChanged>();
services.AddAsyncSignal<WorkItem>(ServiceLifetime.Transient);
```

The default lifetime is singleton because a signal normally represents one
publisher boundary. Pass `Scoped` or `Transient` explicitly when ownership is
different. Overloads accept immutable `SignalOptions` or `AsyncSignalOptions`;
the registration does not use global configuration.

## Logging

```csharp
var signal = new Signal<OrderChanged>(logger.ToSignalOptions());
```

`ToSignalOptions` and `ToAsyncSignalOptions` create explicit
report-and-continue policies. Logging happens only on a handler failure. If the
logger itself throws, the Core/Async sink-failure contract still preserves both
exceptions.

## Activities and metrics

Instrumentation is explicit so the normal `Publish` path remains unchanged:

```csharp
using var diagnostics = new SignalDiagnostics("MyCompany.Orders", metricsEnabled: true);
signal.PublishWithDiagnostics(message, diagnostics);
```

The adapter emits a `signal.publish` activity and, when metrics are explicitly
enabled, the following instruments:

- `xpand.events.publish.count`;
- `xpand.events.publish.errors`;
- `xpand.events.publish.duration` in milliseconds.

When metrics are disabled and `ActivitySource` has no listeners,
`PublishWithDiagnostics` delegates directly to Core. The regression suite asserts
zero steady-state managed allocation for that disabled path.

## Observable and Channel adapters

`signal.AsObservable()` forwards payloads to `IObserver<T>.OnNext` and returns the
underlying signal token from `Subscribe`. Signals have no completion state, so the
adapter never invents `OnCompleted` or `OnError` notifications.

`Signal<T>.SubscribeTo(ChannelWriter<T>)` uses non-blocking `TryWrite` and throws
when a wait-mode bounded channel is full or completed. It never blocks the
publishing thread. `AsyncSignal<T>.SubscribeTo` awaits `WriteAsync`, so normal
Channel backpressure and cancellation apply. `ChannelReader<T>.PumpToAsync`
provides the inverse bridge for synchronous or asynchronous signals.

Channels are process-local queues. These adapters do not add persistence,
delivery acknowledgement, retries, or distributed transport.
