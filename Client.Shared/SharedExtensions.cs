using System;
using System.Reflection;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Shared.Components;
using Fishbowl.Net.Client.Shared.Services;
using Fishbowl.Net.Client.Shared.Store;
using Fluxor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Fishbowl.Net.Client.Shared
{
    public static class SharedExtensions
    {
        public static IServiceCollection AddSharedServices(this IServiceCollection services, Assembly projectAssembly) =>
            services
                .AddSingleton<IStorageService, StorageService>()
                .AddFluxor(options =>
                    options.ScanAssemblies(typeof(SharedExtensions).Assembly, projectAssembly)
                    .AddMiddleware<LoggingMiddleware>());

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