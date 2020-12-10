using System;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Doitsu.Ecommerce.Presentation.Shared.Interfaces;
using Doitsu.Ecommerce.Presentation.Shared.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Doitsu.Ecommerce.Presentation.Client
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            // builder.RootComponents.Add<App>("#app");

            builder.Services.AddHttpClient("Doitsu.Ecommerce.Presentation.ServerAPI")
                .ConfigureHttpClient(client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();

            // Supply HttpClient instances that include access tokens when making requests to the server project.
            builder.Services.AddScoped(provider =>
            {
                var factory = provider.GetRequiredService<IHttpClientFactory>();
                return factory.CreateClient("Doitsu.Ecommerce.Presentation.ServerAPI");
            });

            builder.Services.AddOidcAuthentication(options =>
            {
                builder.Configuration.Bind("Oidc:ProviderOptions", options.ProviderOptions);
                builder.Configuration.Bind("Oidc:AuthenticationPaths", options.AuthenticationPaths);
            });

            var host = builder.Build();
            return host.RunAsync();
        }
    }
}