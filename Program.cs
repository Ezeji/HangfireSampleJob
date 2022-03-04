using Hangfire;
using HangfireSample.Services.Message;
using HangfireSample.Services.Message.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using WellaHealthCoreData;
using WellaHealthCoreData.Models.Insurance;
using WellaHealthCoreRepository;
using WellaHealthCoreRepository.Interfaces;

namespace HangfireSample
{
    public class Program
    {
        private static void Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();
            Configure(host.Services);
            host.Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) => new HostBuilder()
            .ConfigureHostConfiguration(config =>
            {
                string environment = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
                string appSetting = string.IsNullOrEmpty(environment) ? $"appsettings.json" : $"appsettings.{environment}.json";

                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile(appSetting, optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureLogging((hostBuilder, logging) =>
            {
                logging.AddConfiguration(hostBuilder.Configuration.GetSection("Logging"));
                logging.AddNLog();
            })
            .ConfigureServices((hostBuilder, services) =>
            {
                ConfigureServices(services, hostBuilder.Configuration);
            });

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            //disable automatic retries inother to fetch job errors faster
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 0 });

            string connectionString = configuration.GetConnectionString("WellaHealthDb");
            services.AddDbContext<WellaHealthDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            //hangfire support
            services.AddHangfire(x => x.UseSqlServerStorage(connectionString));

            //add the processing server as IHostedService
            services.AddHangfireServer();

            //add configuration settings
            //services.Configure<DojahConfig>(configuration.GetSection("DojahConfig"));

            //add the main program to run
            services.AddSingleton<JobProcessor>();

            //services instantiations
            services.AddScoped<IMessageService, MessageService>();

            //repository instantiations
            services.AddScoped<IGenericRepository<InsuranceSubscription>, GenericRepository<InsuranceSubscription>>();
        }

        private static void Configure(IServiceProvider services)
        {
            IBackgroundJobClient backgroundJobClient = services.GetRequiredService<IBackgroundJobClient>();

            backgroundJobClient.Enqueue<JobProcessor>(job => job.ProcessJobsAsync());

            Console.WriteLine("Background job processing is complete");
        }
    }
}
