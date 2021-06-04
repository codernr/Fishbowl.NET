using System.Threading.Tasks;
using Fishbowl.Net.Client.Shared.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Fishbowl.Net.Client.Shared
{
    public static class SharedExtensions
    {
        public static WebAssemblyHostBuilder AddInteropServices(this WebAssemblyHostBuilder builder)
        {
            builder.Services.AddSingleton<IStorageService, StorageService>();

            if (builder.HostEnvironment.IsDevelopment())
            {
                builder.Services.AddSingleton<IScreenService, DevScreenService>();
            }
            else
            {
                builder.Services.AddSingleton<IScreenService, ScreenService>();
            }

            return builder;
        }

        public static Task InitializeInteropServicesAsync(this WebAssemblyHost host) =>
            Task.WhenAll(
                host.Services.GetRequiredService<IStorageService>().InitializeAsync(),
                host.Services.GetRequiredService<IScreenService>().InitializeAsync());
    }
}