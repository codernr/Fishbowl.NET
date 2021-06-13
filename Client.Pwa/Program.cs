using System.Globalization;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Pwa.Common;
using Fishbowl.Net.Client.Shared;
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
                    .AddJsonSerializationOptions()
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
