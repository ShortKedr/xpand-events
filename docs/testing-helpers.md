# Testing helpers

`Xpand.Events.Testing` contains framework-neutral probes. It does not reference
NUnit, xUnit, MSTest, mocking libraries, or Unity.

```csharp
var probe = new SignalProbe<OrderChanged>();
using IDisposable subscription = probe.SubscribeTo(signal);

signal.Publish(message);

Assert.That(probe.Snapshot(), Is.EqualTo(new[] { message }));
```

`SignalProbe`, `SignalProbe<T>`, and `AsyncSignalProbe<T>` expose cached handlers,
thread-safe counts, reset operations, and stable array snapshots.
`InvocationLog` creates named or formatted handlers for explicit ordering
assertions. All helpers return the real signal subscription token, so tests cover
the same ownership behavior as production code.
