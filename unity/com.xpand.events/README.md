# Xpand Events for Unity

This prerelease UPM package contains the dependency-free synchronous Core API and
explicit Unity lifecycle helpers. Install it from a Git tag whose package path is
`unity/com.xpand.events`.

```csharp
var signal = new Signal<int>();
using IDisposable subscription = signal.Subscribe(HandleScore, priority: 10);
signal.Publish(42);
```

Handlers run synchronously on the publishing thread. Use
`UnityMainThreadDispatcher.Post` only when explicit main-thread transfer is
required. Strong subscriptions must be disposed; `SignalListenerBehaviour` and
`CompositeSubscription` make lifecycle ownership explicit.

See `Documentation~/index.md` for the verified support matrix and usage details.
