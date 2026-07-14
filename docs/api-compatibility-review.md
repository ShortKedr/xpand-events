# Public API and binary compatibility review

This review defines the pre-1.0 freeze candidate. It covers the public surfaces
of the Core, Async, Microsoft Extensions, Testing, Analyzers, and legacy
compatibility packages on both `netstandard2.0` and `net8.0`.

## Decisions

- `Xpand.Events.Core` retains the small `Signal`, `Signal<T>`, and
  `SignalOptions` surface. Ownership, priority, exception, snapshot, reentrancy,
  suspension, and threading semantics are part of compatibility, not only method
  signatures.
- `Xpand.Events.Async` exposes only payload-oriented `AsyncSignal<T>`, its
  `ValueTask` delegate, and immutable options. Parallel dispatch is not reserved
  by an ambiguous flag or enum; it remains a future explicit API decision.
- Microsoft integration stays in extension methods and a per-instance
  `SignalDiagnostics` object. DI and Channel overload families use explicit
  convenience overloads instead of multiple optional-parameter overloads, so a
  future overload cannot silently change source binding.
- Testing helpers return stable copies and cached handlers. They do not expose a
  test-framework dependency.
- The Roslyn analyzer implementation is internal. The supported surface is its
  documented diagnostic IDs and messages, delivered from
  `analyzers/dotnet/cs`; no runtime reference API is promised.
- The legacy `Xpand.Events` package remains a separate nullable-oblivious
  compatibility facade. Its generated arity surface is frozen for migration,
  but new features belong in payload-oriented packages. RS0041 is suppressed
  only in that project because retrofitting nullable metadata would change the
  legacy consumer contract.
- Assemblies remain unsigned according to ADR 0001. Adding or changing a strong
  name after 1.0 would be a binary-breaking assembly identity change.

## Enforcement

`PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt` files are compiled with
RS0016 and RS0017 as errors for Core, Async, Microsoft Extensions, Testing, and
the legacy compatibility package. The analyzer package has no public runtime
API; its diagnostic release files track rule compatibility instead.

NuGet package validation compares reference and implementation assemblies across
target frameworks. The only API compatibility suppression is the documented
legacy CP0008 difference where `System.Enum` implements `ISpanFormattable` on
modern .NET but not .NET Standard 2.0.

This repository has no previously published stable 1.x binary to use as a
package-to-package baseline. The checked-in surfaces therefore establish the RC
baseline. After a release is published, package validation must also compare
against the latest stable package before any subsequent release.

## Compatibility rule after 1.0

Removing a public member, changing assembly identity or target frameworks,
tightening generic constraints, changing optional defaults, or changing the
documented behavioral contracts requires a major version. Additive APIs require
PublicAPI review and focused tests. The compatibility package follows the same
binary rule even though its runtime behavior remains explicitly legacy and is
not upgraded to Core guarantees.
