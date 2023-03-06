using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Rinsen.Outback.Gui;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>()
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders();
                    var env = hostingContext.HostingEnvironment;
                    if (env.IsDevelopment())
                    {
                        logging.AddConsole().AddRinsenGelfLogger();
                    }
                    else
                    {
                        logging.AddRinsenGelfLogger();
                    }
                });
            });
}
