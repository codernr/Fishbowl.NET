using System.Text.Json;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Shared.Services;
using Fishbowl.Net.Shared.Serialization;
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
                host.Services.GetRequiredService<IStorageService>().InitializeAsync(),
                host.Services.GetRequiredService<IScreenService>().InitializeAsync());
    }
}