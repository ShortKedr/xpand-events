# Upgrading from `1.0.0-rc.1` to `1.0.0`

The stable release is intended to preserve the public API and behavioral
contract of `1.0.0-rc.1`. Applications should update all Xpand Events packages
together so that Core and optional packages resolve to the same version.

## NuGet

Update every direct Xpand Events package reference from `1.0.0-rc.1` to
`1.0.0`, restore with a clean package graph, rebuild, and run the application's
signal contract tests. Do not mix RC and stable optional packages in one
application.

```xml
<PackageReference Include="Xpand.Events.Core" Version="1.0.0" />
<PackageReference Include="Xpand.Events.Async" Version="1.0.0" />
<PackageReference Include="Xpand.Events.Extensions.Microsoft" Version="1.0.0" />
```

Projects that still use `XEvent` should update the separate `Xpand.Events`
compatibility package to the same version. Moving from `XEvent` to `Signal<T>`
remains an explicit migration; installing stable does not silently change legacy
semantics.

## Unity Package Manager

Change the Git tag in the package reference while preserving the package
subpath:

```json
{
  "com.xpand.events": "https://github.com/ShortKedr/xpand-events.git?path=/unity/com.xpand.events#v1.0.0"
}
```

Remove an old package-lock entry only if Unity does not resolve the new tag, then
reopen the project and run its Edit Mode, Play Mode, and representative player
smoke tests.

## Automated validation

`scripts/test-stable-upgrade.sh` builds local RC and stable package sets, restores
all six NuGet packages into a clean `net8.0` consumer, builds and runs against the
RC, updates every reference to stable, then restores, rebuilds, and runs again.
The test uses an isolated package cache so a previously cached version cannot
hide a packaging error.

This local regression test proves the package graph and source-level transition.
Before publishing stable, repeat the consumer validation with the exact
`1.0.0-rc.1` packages downloaded from NuGet.org and the exact stable artifacts
produced from the proposed tag.
