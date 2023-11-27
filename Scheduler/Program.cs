using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using ASPNETCoreScheduler.Extensions;
using ASPNETCoreScheduler.Scheduler;
using Microsoft.AspNetCore.Http;
using System;


namespace ASPNETCoreScheduler
{
    public class Program
    {
        public static IConfigurationRoot Configuration { get; set; }
        public static IConfiguration StaticConfig { get; private set; }
        public static async Task Main(string[] args)
        {
            try
            {
                var host = new HostBuilder()
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.AddEnvironmentVariables();
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);
                    config.AddCommandLine(args);

                    Configuration = config.Build();
                    StaticConfig = Configuration;

                })
                 .ConfigureLogging((hostingContext, logging) =>
                 {
                     logging.AddConsole();
                     logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));

                     //Set for log4net
                     logging.AddLog4Net(hostingContext.Configuration.GetValue<string>("Log4NetConfigFile:Name"));
                 })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging();

                    services.ConfigureMySqlContext(Configuration);
                    services.ConfigureRepositoryWrapper();
                    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                    services.AddSingleton<IHostedService, SchedulerTask>();
                })
                .UseConsoleLifetime()
                .Build();

                using (host)
                {
                    // Start the host
                    await host.StartAsync();

                    // Wait for the host to shutdown
                    await host.WaitForShutdownAsync();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Program.cs "+DateTime.Now+ex.Message);
            }
        }
    }
}
