# Integration samples

The M4 samples are executable smoke tests as well as usage examples:

- `samples/Xpand.Events.WorkerSample` registers a singleton `Signal<T>` in the
  Microsoft Generic Host, injects it into a `BackgroundService`, owns the
  subscription with a scope, and stops deterministically after three publishes.
- `samples/Xpand.Events.DesktopSample` publishes from a worker thread and posts
  only the UI update to a single-thread `SynchronizationContext`. Core never
  switches threads implicitly.
- `samples/Xpand.Events.ChannelSample` connects `AsyncSignal<T>` to a capacity-one
  bounded Channel and proves that the second publish waits for capacity without
  changing payload order.

Run them from the repository root:

```shell
dotnet run --project samples/Xpand.Events.WorkerSample
dotnet run --project samples/Xpand.Events.DesktopSample
dotnet run --project samples/Xpand.Events.ChannelSample
```

All three examples are process-local. A successful run does not imply durable,
distributed, retry, or guaranteed-delivery behavior.
