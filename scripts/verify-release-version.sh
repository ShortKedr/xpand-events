#!/usr/bin/env bash
set -euo pipefail

expected="${1:?usage: verify-release-version.sh <semver>}"
projects=(
  Xpand.Events/Xpand.Events.csproj
  Xpand.Events.Core/Xpand.Events.Core.csproj
  Xpand.Events.Async/Xpand.Events.Async.csproj
  Xpand.Events.Extensions.Microsoft/Xpand.Events.Extensions.Microsoft.csproj
  Xpand.Events.Testing/Xpand.Events.Testing.csproj
  Xpand.Events.Analyzers/Xpand.Events.Analyzers.csproj
)

for project in "${projects[@]}"; do
  actual="$(dotnet msbuild "$project" -nologo -getProperty:Version | tail -n 1 | tr -d '\r')"
  if [[ "$actual" != "$expected" ]]; then
    echo "$project resolves Version '$actual', expected '$expected'." >&2
    exit 1
  fi
done

manifest_version="$(jq -r '.version' unity/com.xpand.events/package.json)"
if [[ "$manifest_version" != "$expected" ]]; then
  echo "UPM manifest version '$manifest_version', expected '$expected'." >&2
  exit 1
fi

grep -Fq "## [$expected]" CHANGELOG.md
grep -Fq "## [$expected]" unity/com.xpand.events/CHANGELOG.md
