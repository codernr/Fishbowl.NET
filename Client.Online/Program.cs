using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Fishbowl.Net.Client.Online.Services;
using Fishbowl.Net.Client.Shared;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace Fishbowl.Net.Client.Online
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

            builder.RootComponents.Add<App>("#app");

            builder
                .AddInteropServices()
                .Services
                    .AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
                    .AddTransient<IClientState, ClientState>()
                    .AddLocalization();

            var culture = new CultureInfo("hu");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            var host = builder.Build();

            await host.InitializeInteropServicesAsync();
            
            await host.RunAsync();
        }
    }
}
