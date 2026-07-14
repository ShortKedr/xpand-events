# Analyzers

`Xpand.Events.Analyzers` is an optional Roslyn analyzer package. Add it as a
development-only dependency; it does not add runtime assemblies or dependencies
to an application:

```xml
<PackageReference Include="Xpand.Events.Analyzers" Version="1.0.0-rc.1">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```

## Diagnostics

| ID | Default severity | Meaning |
|---|---|---|
| `XPAND001` | Warning | The `IDisposable` returned by `Subscribe` is discarded. Store it or put it in an explicit scope. |
| `XPAND002` | Warning | An asynchronous handler is passed to synchronous `Signal`/`Signal<T>`. Use `AsyncSignal<T>` so completion, cancellation, and failures are observable. |
| `XPAND003` | Warning | A signal publisher is stored in a static field and can retain strong subscribers for the process lifetime. Use an explicit process-lifetime ownership policy. |
| `XPAND004` | Warning | A `MonoBehaviour` subscribes from `Awake`, `Start`, or `OnEnable` without the lifecycle ownership supplied by `SignalListenerBehaviour`. |

These diagnostics flag ownership risks; they do not silently change signal
semantics. A diagnostic can be configured with standard `.editorconfig` severity
settings when the application has a deliberate, reviewed ownership policy.

The analyzer targets Roslyn 4.8 so it can load in the .NET 8 SDK and supported
Unity 6 editor toolchains. Its compiler dependencies are private and are not
shipped as application dependencies.
