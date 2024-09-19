using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MqttBrokerWithDashboard
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
              .ConfigureWebHostDefaults(webBuilder =>
              {
                  // Config log
                  webBuilder.ConfigureLogging((ctx, builder) =>
                  {
                      builder.AddConfiguration(ctx.Configuration.GetSection("Logging"));
                      builder.AddFile(o => o.RootPath = ctx.HostingEnvironment.ContentRootPath);
                  });

                  webBuilder.UseStartup<Startup>();
              });
        }
    }
}