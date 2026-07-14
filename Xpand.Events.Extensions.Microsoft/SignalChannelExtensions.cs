#nullable enable

using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Xpand.Events.Async;

namespace Xpand.Events.Extensions.Microsoft {
    /// <summary>Explicit adapters between signals and in-process channels.</summary>
    public static class SignalChannelExtensions {
        /// <summary>
        /// Writes synchronous publishes with <see cref="ChannelWriter{T}.TryWrite"/>.
        /// Publish throws when a bounded wait-mode channel is full or the writer is complete.
        /// </summary>
        public static IDisposable SubscribeTo<T>(
            this Signal<T> signal,
            ChannelWriter<T> writer) {
            return SubscribeTo(signal, writer, 0);
        }

        /// <summary>
        /// Writes synchronous publishes with <see cref="ChannelWriter{T}.TryWrite"/>
        /// at the specified subscription priority.
        /// </summary>
        public static IDisposable SubscribeTo<T>(
            this Signal<T> signal,
            ChannelWriter<T> writer,
            int priority) {
            if (signal == null) throw new ArgumentNullException(nameof(signal));
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            return signal.Subscribe(payload => {
                if (!writer.TryWrite(payload)) {
                    throw new InvalidOperationException(
                        "The channel rejected the signal payload. It may be full or completed; use AsyncSignal<T> for awaited backpressure.");
                }
            }, priority);
        }

        /// <summary>
        /// Writes async publishes with <see cref="ChannelWriter{T}.WriteAsync(T,CancellationToken)"/>,
        /// so a bounded wait-mode channel applies asynchronous backpressure.
        /// </summary>
        public static IDisposable SubscribeTo<T>(
            this AsyncSignal<T> signal,
            ChannelWriter<T> writer) {
            return SubscribeTo(signal, writer, 0);
        }

        /// <summary>
        /// Writes async publishes with awaited backpressure at the specified
        /// subscription priority.
        /// </summary>
        public static IDisposable SubscribeTo<T>(
            this AsyncSignal<T> signal,
            ChannelWriter<T> writer,
            int priority) {
            if (signal == null) throw new ArgumentNullException(nameof(signal));
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            return signal.Subscribe((payload, cancellationToken) => writer.WriteAsync(payload, cancellationToken), priority);
        }

        /// <summary>Reads a channel until completion and publishes each item synchronously.</summary>
        public static Task PumpToAsync<T>(
            this ChannelReader<T> reader,
            Signal<T> signal) {
            return PumpToAsync(reader, signal, default);
        }

        /// <summary>Reads a channel until completion and publishes each item synchronously.</summary>
        public static async Task PumpToAsync<T>(
            this ChannelReader<T> reader,
            Signal<T> signal,
            CancellationToken cancellationToken) {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (signal == null) throw new ArgumentNullException(nameof(signal));

            while (await reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false)) {
                while (reader.TryRead(out T item)) {
                    cancellationToken.ThrowIfCancellationRequested();
                    signal.Publish(item);
                }
            }
        }

        /// <summary>Reads a channel until completion and sequentially awaits each async publish.</summary>
        public static Task PumpToAsync<T>(
            this ChannelReader<T> reader,
            AsyncSignal<T> signal) {
            return PumpToAsync(reader, signal, default);
        }

        /// <summary>Reads a channel until completion and sequentially awaits each async publish.</summary>
        public static async Task PumpToAsync<T>(
            this ChannelReader<T> reader,
            AsyncSignal<T> signal,
            CancellationToken cancellationToken) {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (signal == null) throw new ArgumentNullException(nameof(signal));

            while (await reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false)) {
                while (reader.TryRead(out T item)) {
                    cancellationToken.ThrowIfCancellationRequested();
                    await signal.PublishAsync(item, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
