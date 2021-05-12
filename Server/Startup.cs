using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using Fishbowl.Net.Server.Hubs;
using Fishbowl.Net.Server.Services;
using System;
using Microsoft.AspNetCore.SignalR;
using Fishbowl.Net.Client.Services;
using Fishbowl.Net.Shared.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Fishbowl.Net.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<GameService>();
            services.AddSingleton<Func<string, GameSetupViewModel, GameContext>>(provider =>
                (password, gameSetup) =>
                    {
                        var groupHubContext = new GroupHubContext(
                            provider.GetRequiredService<IHubContext<GameHub, IGameClient>>(), password);
                        return new GameContext(gameSetup, groupHubContext, provider.GetRequiredService<Func<Func<Task>, Timer>>());
                    });
            services.AddSingleton<Func<Func<Task>, Timer>>(provider =>
                action => new Timer(TimeSpan.FromMinutes(this.Configuration.GetValue<int>("GameContextTimeoutInMinutes")),
                    action, provider.GetRequiredService<ILogger<Timer>>()));
            services.AddSignalR();
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddResponseCompression(options =>
            {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHub<GameHub>("/game");
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
