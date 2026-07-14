using System;
using Xpand.Events.NetStandardConsumer;

internal static class Program {
    private static void Main() {
        var counter = new Counter();
        var observed = 0;

        using (counter.Changed.Subscribe(value => observed = value)) {
            counter.Set(42);
        }

        if (observed != 42) {
            throw new InvalidOperationException("The netstandard2.0 consumer did not deliver the payload.");
        }

        counter.Set(100);
        if (observed != 42) {
            throw new InvalidOperationException("Disposing the consumer subscription did not unregister it.");
        }

        Console.WriteLine("Xpand.Events modern .NET/netstandard2.0 consumer passed.");
    }
}
