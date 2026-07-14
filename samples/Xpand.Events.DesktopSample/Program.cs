#nullable enable

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Xpand.Events;

internal static class Program {
    private static void Main() {
        using var uiContext = new SingleThreadSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(uiContext);

        int uiThread = Environment.CurrentManagedThreadId;
        int handlerThread = -1;
        string? rendered = null;
        var signal = new Signal<string>();

        using IDisposable subscription = signal.Subscribe(message => {
            handlerThread = Environment.CurrentManagedThreadId;
            uiContext.Post(_ => {
                if (Environment.CurrentManagedThreadId != uiThread) {
                    throw new InvalidOperationException("The UI update did not run on the desktop synchronization context.");
                }

                rendered = message;
                uiContext.Complete();
            }, null);
        });

        Task publisher = Task.Run(() => signal.Publish("rendered on UI context"));
        uiContext.RunOnCurrentThread();
        publisher.GetAwaiter().GetResult();
        SynchronizationContext.SetSynchronizationContext(null);

        if (handlerThread == uiThread || rendered != "rendered on UI context") {
            throw new InvalidOperationException("Signals must run on the publisher thread; UI dispatch must stay explicit.");
        }

        Console.WriteLine("Desktop sample passed: publish stayed on the worker and rendering was posted to the UI context.");
    }
}

internal sealed class SingleThreadSynchronizationContext : SynchronizationContext, IDisposable {
    private readonly BlockingCollection<Action> _work = new BlockingCollection<Action>();

    public override void Post(SendOrPostCallback callback, object? state) {
        if (callback == null) throw new ArgumentNullException(nameof(callback));
        _work.Add(() => callback(state));
    }

    public void RunOnCurrentThread() {
        foreach (Action action in _work.GetConsumingEnumerable()) {
            action();
        }
    }

    public void Complete() {
        _work.CompleteAdding();
    }

    public void Dispose() {
        _work.Dispose();
    }
}
