using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Pwa.Common;
using Fishbowl.Net.Client.Shared;
using Fishbowl.Net.Shared.Serialization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Fishbowl.Net.Client.Pwa
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder
                .AddInteropServices()
                .Services
                    .AddLocalization()
                    .AddSingleton<JsonSerializerOptions>(services =>
                    {
                        JsonSerializerOptions options = new(JsonSerializerDefaults.Web);
                        options.Converters.Add(new GameConverter());
                        options.Converters.Add(new TeamConverter());
                        options.Converters.Add(new RoundConverter());
                        options.Converters.Add(new RandomEnumeratorConverter());
                        options.Converters.Add(new TimeSpanConverter());

                        return options;
                    })
                    .AddSingleton<GameProperty>();

            var culture = new CultureInfo("hu");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            var host = builder.Build();

            await host.InitializeInteropServicesAsync();
            
            await host.RunAsync();
        }
    }
}
