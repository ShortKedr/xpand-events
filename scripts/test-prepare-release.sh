#!/usr/bin/env bash
set -euo pipefail

repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
fixture="$(mktemp -d)"
trap 'rm -rf "$fixture"' EXIT

cd "$repository_root"

for project in \
  Xpand.Events/Xpand.Events.csproj \
  Xpand.Events.Core/Xpand.Events.Core.csproj \
  Xpand.Events.Async/Xpand.Events.Async.csproj \
  Xpand.Events.Extensions.Microsoft/Xpand.Events.Extensions.Microsoft.csproj \
  Xpand.Events.Testing/Xpand.Events.Testing.csproj \
  Xpand.Events.Analyzers/Xpand.Events.Analyzers.csproj; do
  mkdir -p "$fixture/$(dirname "$project")"
  cp "$project" "$fixture/$project"
done

mkdir -p "$fixture/scripts" "$fixture/unity/com.xpand.events"
cp scripts/prepare-release.sh scripts/verify-release-version.sh "$fixture/scripts/"
cp unity/com.xpand.events/package.json "$fixture/unity/com.xpand.events/"

cat > "$fixture/CHANGELOG.md" <<'EOF'
# Changelog

## [Unreleased]

- Preserve this curated release note.
- Preserve this second release note.

## [0.9.0] - 2026-01-01

- Previous release.
EOF

cat > "$fixture/unity/com.xpand.events/CHANGELOG.md" <<'EOF'
# Changelog

## [0.9.0] - 2026-01-01

- Previous Unity release.
EOF

cd "$fixture"
./scripts/prepare-release.sh 1.2.3-rc.4

grep -F '## [1.2.3-rc.4]' CHANGELOG.md >/dev/null
grep -F -- '- Preserve this curated release note.' CHANGELOG.md >/dev/null
grep -F -- '- Preserve this second release note.' CHANGELOG.md >/dev/null
grep -F -- '- Preserve this curated release note.' \
  unity/com.xpand.events/CHANGELOG.md >/dev/null

echo "prepare-release regression test passed"
