#nullable enable

using System;
using Microsoft.Extensions.DependencyInjection;
using Xpand.Events.Async;

namespace Xpand.Events.Extensions.Microsoft {
    /// <summary>Dependency injection registrations for signal instances.</summary>
    public static class SignalServiceCollectionExtensions {
        /// <summary>Registers a synchronous signal with fail-fast behavior.</summary>
        public static IServiceCollection AddSignal<T>(this IServiceCollection services) {
            return AddSignal<T>(services, SignalOptions.FailFast, ServiceLifetime.Singleton);
        }

        /// <summary>Registers a synchronous signal with fail-fast behavior and the specified lifetime.</summary>
        public static IServiceCollection AddSignal<T>(
            this IServiceCollection services,
            ServiceLifetime lifetime) {
            return AddSignal<T>(services, SignalOptions.FailFast, lifetime);
        }

        /// <summary>Registers a synchronous signal with immutable per-instance options.</summary>
        public static IServiceCollection AddSignal<T>(
            this IServiceCollection services,
            SignalOptions options) {
            return AddSignal<T>(services, options, ServiceLifetime.Singleton);
        }

        /// <summary>Registers a synchronous signal with immutable per-instance options and lifetime.</summary>
        public static IServiceCollection AddSignal<T>(
            this IServiceCollection services,
            SignalOptions options,
            ServiceLifetime lifetime) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (options == null) throw new ArgumentNullException(nameof(options));
            services.Add(new ServiceDescriptor(typeof(Signal<T>), _ => new Signal<T>(options), lifetime));
            return services;
        }

        /// <summary>Registers an asynchronous signal with fail-fast behavior.</summary>
        public static IServiceCollection AddAsyncSignal<T>(this IServiceCollection services) {
            return AddAsyncSignal<T>(services, AsyncSignalOptions.FailFast, ServiceLifetime.Singleton);
        }

        /// <summary>Registers an asynchronous signal with fail-fast behavior and the specified lifetime.</summary>
        public static IServiceCollection AddAsyncSignal<T>(
            this IServiceCollection services,
            ServiceLifetime lifetime) {
            return AddAsyncSignal<T>(services, AsyncSignalOptions.FailFast, lifetime);
        }

        /// <summary>Registers an asynchronous signal with immutable per-instance options.</summary>
        public static IServiceCollection AddAsyncSignal<T>(
            this IServiceCollection services,
            AsyncSignalOptions options) {
            return AddAsyncSignal<T>(services, options, ServiceLifetime.Singleton);
        }

        /// <summary>Registers an asynchronous signal with immutable per-instance options and lifetime.</summary>
        public static IServiceCollection AddAsyncSignal<T>(
            this IServiceCollection services,
            AsyncSignalOptions options,
            ServiceLifetime lifetime) {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (options == null) throw new ArgumentNullException(nameof(options));
            services.Add(new ServiceDescriptor(typeof(AsyncSignal<T>), _ => new AsyncSignal<T>(options), lifetime));
            return services;
        }
    }
}
