using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Extensions.Logging;

namespace WebhookListener
{
    public class Program
    {
        static readonly LoggerProviderCollection Providers = new LoggerProviderCollection();

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
            try
            {
                Log.Information("Starting Webhook Listener Service");
                CreateWebHostBuilder(args).Build().Run();
                Log.Information($"Process ID - {Process.GetCurrentProcess().Id}");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Webhook Listener Service terminated unexpectedly");
                return;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
              .ConfigureKestrel((context, options) =>
              {
                  //options.ListenAnyIP(5000);
              }).UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
                  .ReadFrom.Configuration(hostingContext.Configuration)
                  .Enrich.FromLogContext().Enrich.WithExceptionDetails()
                  .WriteTo.Providers(Providers), writeToProviders: true)
          .UseStartup<Startup>();

        }
    }
}
