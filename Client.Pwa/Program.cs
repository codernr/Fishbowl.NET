using System.Globalization;
using Fishbowl.Net.Client.Pwa.Common;
using Fishbowl.Net.Client.Shared;
using Fishbowl.Net.Client.Shared.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Fishbowl.Net.Client.Pwa;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Services;
using Fluxor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder
    .AddSharedServices()
    .Services
        .AddLocalization()
        .AddJsonSerializationOptions()
        .AddSingleton<GameProperty>()
        .AddMudServices()
        .AddFluxor(options =>
            options.ScanAssemblies(typeof(Program).Assembly, typeof(StateManagerService).Assembly));

var culture = new CultureInfo("hu");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var host = builder.Build();

await host.InitializeInteropServicesAsync();

await host.RunAsync();
