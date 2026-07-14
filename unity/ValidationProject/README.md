# Unity validation project

This minimal Unity 6000.3 project installs `com.xpand.events` from the adjacent
folder. It is committed so package tests and player builds run without creating a
consumer project by hand.

`ValidationCommands` exposes batch-mode entry points for domain-reload settings
and macOS Mono, macOS IL2CPP, Android ARM64 IL2CPP, and WebGL builds. The
`Unity validation` workflow targets a licensed self-hosted macOS arm64 runner with
the `unity-6000.3` label. A green local run is not recorded as a green GitHub run;
the roadmap keeps CI/tag installation pending until those external checks occur.
