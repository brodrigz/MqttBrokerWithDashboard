using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MqttBrokerWithDashboard.MqttBroker;
using MQTTnet.AspNetCore;
using MudBlazor.Services;

namespace MqttBrokerWithDashboard
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages(options => options.RootDirectory = "/Pages");
            services.AddServerSideBlazor();
            services.AddMudServices();

            services.AddSingleton<MqttBrokerService>();
            services.AddHostedMqttServer(options =>
            {
                options
                .WithDefaultEndpoint()
                .WithDefaultEndpointPort(1883);
            });

            services
                .AddMqttConnectionHandler()
                .AddConnections()
                .AddMqttTcpServerAdapter();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapMqtt("/mqtt");
            });

            app.UseMqttServer(server =>
            {
                var mqttBrokerService = app.ApplicationServices.GetRequiredService<MqttBrokerService>();

                // Sets MQTTNet's server on our singleton and binds events
                mqttBrokerService.BindServer(server);
            });
        }
    }
}