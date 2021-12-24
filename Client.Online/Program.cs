using System;
using System.Net.Http;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Fishbowl.Net.Client.Shared;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Fishbowl.Net.Client.Online.Services;
using Microsoft.AspNetCore.Components.Web;
using Fishbowl.Net.Client.Online;
using MudBlazor.Services;
using MudBlazor;
using Fishbowl.Net.Client.Shared.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder
    .AddInteropServices()
    .Services
        .AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
        .AddTransient<IClientState, ClientState>()
        .AddLocalization()
        .AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
            config.SnackbarConfiguration.ShowCloseIcon = false;
            config.SnackbarConfiguration.VisibleStateDuration = 3000;
            config.SnackbarConfiguration.ShowTransitionDuration = 300;
            config.SnackbarConfiguration.HideTransitionDuration = 300;
        });

var culture = new CultureInfo("hu");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var host = builder.Build();

await host.InitializeInteropServicesAsync();

await host.RunAsync();