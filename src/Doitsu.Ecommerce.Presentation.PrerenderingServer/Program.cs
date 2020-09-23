using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Doitsu.Ecommerce.Presentation.PrerenderingServer
{
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
                    webBuilder
                        .UseConfiguration(new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddIniFile("appsettings.ini", true)
                            .Build())
                        .ConfigureAppConfiguration((context, commonBuilder) => commonBuilder.AddJsonFile($"Metadata.json", true, true))
                        .UseStartup<Startup>();
                });
    }
}