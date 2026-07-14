#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xpand.Events;
using Xpand.Events.Extensions.Microsoft;

internal static class Program {
    private static async Task Main(string[] args) {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddSignal<WorkCompleted>();
        builder.Services.AddHostedService<SampleWorker>();

        using IHost host = builder.Build();
        await host.RunAsync().ConfigureAwait(false);
    }
}

internal readonly struct WorkCompleted {
    public WorkCompleted(int sequence) {
        Sequence = sequence;
    }

    public int Sequence { get; }
}

internal sealed class SampleWorker : BackgroundService {
    private readonly Signal<WorkCompleted> _completed;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<SampleWorker> _logger;

    public SampleWorker(
        Signal<WorkCompleted> completed,
        IHostApplicationLifetime lifetime,
        ILogger<SampleWorker> logger) {
        _completed = completed;
        _lifetime = lifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using CancellationTokenRegistration startedRegistration =
            _lifetime.ApplicationStarted.Register(() => started.TrySetResult(true));
        using CancellationTokenRegistration stoppingRegistration =
            stoppingToken.Register(() => started.TrySetCanceled(stoppingToken));
        await started.Task.ConfigureAwait(false);

        int observed = 0;
        using IDisposable subscription = _completed.Subscribe(message => {
            stoppingToken.ThrowIfCancellationRequested();
            observed = message.Sequence;
            _logger.LogInformation("Observed local work item {Sequence}", message.Sequence);
        });

        for (int sequence = 1; sequence <= 3; sequence++) {
            _completed.Publish(new WorkCompleted(sequence));
        }

        if (observed != 3) {
            throw new InvalidOperationException("The worker did not observe all in-process notifications.");
        }

        Console.WriteLine("Worker sample passed: 3 in-process notifications observed.");
        _lifetime.StopApplication();
    }
}
