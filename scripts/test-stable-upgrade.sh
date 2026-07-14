#!/usr/bin/env bash
set -euo pipefail

repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
fixture="$(mktemp -d)"
trap 'rm -rf "$fixture"' EXIT

feed="$fixture/feed"
consumer="$fixture/consumer"
packages="$fixture/packages"
mkdir -p "$feed" "$consumer" "$packages"

cd "$repository_root"

for version in 1.0.0-rc.1 1.0.0; do
  dotnet pack Xpand-Events.sln \
    --configuration Release \
    -p:Version="$version" \
    -p:PackageVersion="$version" \
    --output "$feed" \
    -m:1 \
    --disable-build-servers
done

package_ids=(
  Xpand.Events
  Xpand.Events.Core
  Xpand.Events.Async
  Xpand.Events.Extensions.Microsoft
  Xpand.Events.Testing
  Xpand.Events.Analyzers
)

for package_id in "${package_ids[@]}"; do
  test -f "$feed/$package_id.1.0.0-rc.1.nupkg"
  test -f "$feed/$package_id.1.0.0.nupkg"
done

cat > "$consumer/UpgradeSmoke.csproj" <<'EOF'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Xpand.Events" Version="1.0.0-rc.1" />
    <PackageReference Include="Xpand.Events.Core" Version="1.0.0-rc.1" />
    <PackageReference Include="Xpand.Events.Async" Version="1.0.0-rc.1" />
    <PackageReference Include="Xpand.Events.Extensions.Microsoft" Version="1.0.0-rc.1" />
    <PackageReference Include="Xpand.Events.Testing" Version="1.0.0-rc.1" />
    <PackageReference Include="Xpand.Events.Analyzers" Version="1.0.0-rc.1">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>
</Project>
EOF

cat > "$consumer/Program.cs" <<'EOF'
using Microsoft.Extensions.DependencyInjection;
using Xpand.Events;
using Xpand.Events.Async;
using Xpand.Events.Extensions.Microsoft;
using Xpand.Events.Testing;

var signal = new Signal<int>();
var probe = new SignalProbe<int>();
using (probe.SubscribeTo(signal)) {
    signal.Publish(42);
}

if (probe.Count != 1 || probe.Snapshot()[0] != 42) {
    throw new InvalidOperationException("Synchronous signal package failed.");
}

var asyncSignal = new AsyncSignal<int>();
var asyncValue = 0;
using (asyncSignal.Subscribe((value, _) => {
    asyncValue = value;
    return ValueTask.CompletedTask;
})) {
    await asyncSignal.PublishAsync(42);
}

if (asyncValue != 42) {
    throw new InvalidOperationException("Async signal package failed.");
}

var services = new ServiceCollection();
services.AddSignal<int>();
services.AddAsyncSignal<int>();
if (!services.Any(descriptor => descriptor.ServiceType == typeof(Signal<int>)) ||
    !services.Any(descriptor => descriptor.ServiceType == typeof(AsyncSignal<int>))) {
    throw new InvalidOperationException("Microsoft integration package failed.");
}

var legacy = new XEvent<int>();
var legacyValue = 0;
Event<int> legacyHandler = value => legacyValue = value;
legacy.AddListener(legacyHandler);
legacy.Invoke(42);
if (legacyValue != 42) {
    throw new InvalidOperationException("Compatibility package failed.");
}

Console.WriteLine("XPAND_EVENTS_STABLE_UPGRADE_PASSED");
EOF

restore_and_run() {
  local expected_version="$1"
  dotnet restore "$consumer/UpgradeSmoke.csproj" \
    --packages "$packages" \
    --source "$feed" \
    --source https://api.nuget.org/v3/index.json \
    --force \
    --no-http-cache
  dotnet build "$consumer/UpgradeSmoke.csproj" \
    --configuration Release \
    --no-restore \
    --disable-build-servers
  output="$(dotnet run \
    --project "$consumer/UpgradeSmoke.csproj" \
    --configuration Release \
    --no-build \
    --no-restore)"
  grep -F "XPAND_EVENTS_STABLE_UPGRADE_PASSED" <<< "$output" >/dev/null
  grep -F "Xpand.Events.Core/$expected_version" \
    "$consumer/obj/project.assets.json" >/dev/null
}

restore_and_run 1.0.0-rc.1

perl -0pi -e 's/1\.0\.0-rc\.1/1.0.0/g' "$consumer/UpgradeSmoke.csproj"
restore_and_run 1.0.0

echo "stable upgrade regression test passed"
