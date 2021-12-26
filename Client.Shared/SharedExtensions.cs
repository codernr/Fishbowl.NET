using System;
using System.Text.Json;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Shared.Components;
using Fishbowl.Net.Client.Shared.Services;
using Fishbowl.Net.Client.Shared.Store;
using Fishbowl.Net.Shared.Serialization;
using Fluxor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Fishbowl.Net.Client.Shared
{
    public static class SharedExtensions
    {
        public static WebAssemblyHostBuilder AddSharedServices(this WebAssemblyHostBuilder builder)
        {
            builder.Services
                .AddSingleton<IStorageService, StorageService>();

            return builder;
        }

        public static IServiceCollection AddJsonSerializationOptions(this IServiceCollection services) =>
            services.AddSingleton<JsonSerializerOptions>(services =>
            {
                JsonSerializerOptions options = new(JsonSerializerDefaults.Web);
                options.Converters.Add(new GameConverter());
                options.Converters.Add(new TeamConverter());
                options.Converters.Add(new RoundConverter());
                options.Converters.Add(new ShuffleEnumeratorConverter());
                options.Converters.Add(new TimeSpanConverter());

                return options;
            });

        public static Task InitializeInteropServicesAsync(this WebAssemblyHost host) =>
            Task.WhenAll(
                host.Services.GetRequiredService<IStorageService>().InitializeAsync());

        public static Task Dispatch<T>(this IDispatcher dispatcher) where T : new()
        {
            dispatcher.Dispatch(new T());
            return Task.CompletedTask;
        }

        public static Task Dispatch<T>(this IDispatcher dispatcher, T action)
        {
            dispatcher.Dispatch(action);
            return Task.CompletedTask;
        }

        public static Task DispatchTransition<TScreen>(this IDispatcher dispatcher, TimeSpan delay = default) where TScreen : Screen
        {
            dispatcher.Dispatch(new ScreenManagerTransitionAction(typeof(TScreen), Delay: delay));
            return Task.CompletedTask;
        }

        public static Task DispatchTransition<TScreen, TAction>(
            this IDispatcher dispatcher, TAction action, TimeSpan delay = default)
            where TScreen : Screen
            {
                dispatcher.Dispatch(new ScreenManagerTransitionAction(typeof(TScreen), action, delay));
                return Task.CompletedTask;
            }
    }
}