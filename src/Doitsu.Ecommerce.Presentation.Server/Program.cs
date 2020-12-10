using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Doitsu.Ecommerce.Presentation.Server
{
    public static class Program
    {
        public static void Main(string[] args) =>
            CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var b = Host.CreateDefaultBuilder(args)
                .ConfigureLogging(builder => builder.ClearProviders())
                .UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration))
                .ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>());
            
            return b;
        }
    }
}
