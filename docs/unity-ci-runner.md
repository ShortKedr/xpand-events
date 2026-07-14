# Unity self-hosted CI runner

Unity validation requires macOS ARM64, Unity `6000.3.19f1`, licensed build
modules, and the ability to run standalone players. The workflow therefore uses
a self-hosted runner with these labels:

- `self-hosted`;
- `macOS`;
- `ARM64`;
- `unity-6000.3`.

The custom label is assigned during runner registration. The Unity editor must
exist at the path configured in `.github/workflows/unity.yml`, with macOS Mono
and IL2CPP, Android ARM64 IL2CPP, and WebGL modules installed.

## Security contract

The repository is public. A self-hosted runner executes workflow code with the
permissions of its operating-system account, so it must not be a maintainer's
ordinary interactive environment.

- Use a dedicated machine or isolated, non-administrator OS account with no
  personal files, package-publishing credentials, SSH agent, cloud credentials,
  or unrelated repository access.
- Register the runner as ephemeral with `config.sh --ephemeral`; it must accept
  one reviewed job and then de-register automatically.
- Never commit or log the short-lived registration token.
- Preserve runner diagnostic logs outside the disposable work directory.
- Wipe the runner work directory after the job and install the next current
  runner version before later use.
- Start a runner only after reviewing the exact queued commit and workflow.

The workflow rejects pull requests whose head repository differs from this
repository and checks out without persisted GitHub credentials. External
contributors still receive the GitHub-hosted .NET and security checks; a
maintainer must bring trusted Unity changes onto a same-repository branch before
self-hosted validation.

GitHub's
[self-hosted runner reference](https://docs.github.com/en/actions/reference/runners/self-hosted-runners)
recommends ephemeral rather than persistent runners for autoscaling because each
ephemeral runner receives only one job. GitHub also documents passing custom
labels to `config.sh` with `--labels` in its
[runner label guide](https://docs.github.com/en/actions/how-tos/manage-runners/self-hosted-runners/apply-labels).
Follow the current setup instructions rather than copying a stale download URL
or registration token into this repository.

Registering a developer workstation as the runner requires explicit maintainer
authorization even when the workflow is queued and all local Unity tests pass.
