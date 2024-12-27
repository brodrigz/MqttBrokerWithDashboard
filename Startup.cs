using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MqttBrokerWithDashboard.Services;
using MQTTnet.AspNetCore;
using MudBlazor.Services;
using System.Threading.Tasks;
using System;
using MqttBrokerWithDashboard.Options;
using Microsoft.Extensions.Options;

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
            // Bind and configure options
            services.Configure<MqttServerOptions>(
                Configuration.GetSection("MqttServerOptions"));
            services.Configure<MqttServiceOptions>(
                Configuration.GetSection("MqttServiceOptions"));

            services.AddRazorPages(options => options.RootDirectory = "/Pages");
            services.AddServerSideBlazor();
            services.AddMudServices();
            services.AddControllers();
            services.AddSingleton<MqttBrokerService>();
            services.AddHostedMqttServer(options =>
            {
                options
                .WithDefaultEndpoint()
                .WithDefaultEndpointPort(Configuration.GetValue<int>("MqttServerOptions:Port"));
            });

            services
                .AddMqttConnectionHandler()
                .AddConnections()
                .AddMqttTcpServerAdapter()
                .AddEndpointsApiExplorer()
                .AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Broker API");
                c.RoutePrefix = "swagger";
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
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