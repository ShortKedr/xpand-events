#!/usr/bin/env bash
set -euo pipefail

version="${1:?usage: prepare-release.sh <semver>}"
if [[ ! "$version" =~ ^[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z.-]+)?$ ]]; then
  echo "Version must be SemVer, for example 1.0.0-rc.1 or 1.0.0." >&2
  exit 1
fi

prefix="${version%%-*}"
suffix=""
if [[ "$version" == *-* ]]; then
  suffix="${version#*-}"
fi

projects=(
  Xpand.Events/Xpand.Events.csproj
  Xpand.Events.Core/Xpand.Events.Core.csproj
  Xpand.Events.Async/Xpand.Events.Async.csproj
  Xpand.Events.Extensions.Microsoft/Xpand.Events.Extensions.Microsoft.csproj
  Xpand.Events.Testing/Xpand.Events.Testing.csproj
  Xpand.Events.Analyzers/Xpand.Events.Analyzers.csproj
)

for project in "${projects[@]}"; do
  VERSION_PREFIX="$prefix" VERSION_SUFFIX="$suffix" perl -0pi -e '
    s{<VersionPrefix>.*?</VersionPrefix>}{<VersionPrefix>$ENV{VERSION_PREFIX}</VersionPrefix>};
    s{<VersionSuffix>.*?</VersionSuffix>}{<VersionSuffix>$ENV{VERSION_SUFFIX}</VersionSuffix>};
  ' "$project"
done

manifest="unity/com.xpand.events/package.json"
manifest_tmp="$(mktemp)"
jq --arg version "$version" '.version = $version' "$manifest" > "$manifest_tmp"
mv "$manifest_tmp" "$manifest"

root_changelog="CHANGELOG.md"
unreleased_notes="$(awk '
  /^## \[Unreleased\]/ { in_unreleased = 1; next }
  in_unreleased && /^## \[/ { exit }
  in_unreleased { print }
' "$root_changelog")"

if printf '%s\n' "$unreleased_notes" | grep -q '[^[:space:]]'; then
  notes="$(printf '%s\n' "$unreleased_notes" | awk '
    NF { found = 1 }
    found { lines[++count] = $0 }
    END {
      while (count > 0 && lines[count] ~ /^[[:space:]]*$/) count--
      for (line_number = 1; line_number <= count; line_number++) {
        print lines[line_number]
      }
    }
  ')"
else
  previous_tag="$(git describe --tags --abbrev=0 2>/dev/null || true)"
  range="HEAD"
  if [[ -n "$previous_tag" ]]; then
    range="$previous_tag..HEAD"
  fi
  notes="$(git log --no-merges --pretty=format:'- %s (%h)' "$range")"
  if [[ -z "$notes" ]]; then
    notes="- Maintenance release."
  fi
fi

notes_for_awk="${notes//$'\n'/\\n}"
release_date="$(date -u +%Y-%m-%d)"

root_tmp="$(mktemp)"
awk -v version="$version" -v date="$release_date" -v notes="$notes_for_awk" '
  BEGIN { in_unreleased = 0; inserted = 0 }
  /^## \[Unreleased\]/ {
    print "## [Unreleased]"
    print ""
    print "## [" version "] - " date
    print ""
    gsub(/\\n/, "\n", notes)
    print notes
    print ""
    in_unreleased = 1
    inserted = 1
    next
  }
  in_unreleased && /^## \[/ { in_unreleased = 0 }
  in_unreleased { next }
  { print }
  END { if (!inserted) exit 2 }
' "$root_changelog" > "$root_tmp"
mv "$root_tmp" "$root_changelog"

unity_changelog="unity/com.xpand.events/CHANGELOG.md"
unity_tmp="$(mktemp)"
{
  echo "# Changelog"
  echo
  echo "## [$version] - $release_date"
  echo
  echo "$notes"
  echo
  tail -n +2 "$unity_changelog"
} > "$unity_tmp"
mv "$unity_tmp" "$unity_changelog"

scripts/verify-release-version.sh "$version"
