#nullable enable

using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using Xpand.Events.Async;
using Xpand.Events.Extensions.Microsoft;

internal static class Program {
    private static async Task Main() {
        var options = new BoundedChannelOptions(1) {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true
        };
        Channel<int> channel = Channel.CreateBounded<int>(options);
        var source = new AsyncSignal<int>();
        using IDisposable bridge = source.SubscribeTo(channel.Writer);

        await source.PublishAsync(1).ConfigureAwait(false);
        Task blockedPublish = source.PublishAsync(2).AsTask();
        await Task.Yield();

        if (blockedPublish.IsCompleted) {
            throw new InvalidOperationException("The second publish bypassed bounded-channel backpressure.");
        }

        int first = await channel.Reader.ReadAsync().ConfigureAwait(false);
        await blockedPublish.ConfigureAwait(false);
        int second = await channel.Reader.ReadAsync().ConfigureAwait(false);
        channel.Writer.Complete();

        if (first != 1 || second != 2) {
            throw new InvalidOperationException("The bounded channel changed signal payload order.");
        }

        Console.WriteLine("Channel sample passed: capacity-one backpressure was awaited and order was preserved.");
    }
}
