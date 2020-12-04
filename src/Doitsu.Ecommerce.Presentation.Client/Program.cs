using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Doitsu.Ecommerce.Presentation.Client
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddHttpClient("Doitsu.Ecommerce.Presentation.ServerAPI")
                .ConfigureHttpClient(client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            // Supply HttpClient instances that include access tokens when making requests to the server project.
            builder.Services.AddScoped(provider =>
            {
                var factory = provider.GetRequiredService<IHttpClientFactory>();
                return factory.CreateClient("Doitsu.Ecommerce.Presentation.ServerAPI");
            });

            builder.Services.AddOidcAuthentication(options =>
            {
                options.ProviderOptions.ClientId = "balosar-blazor-client";
                options.ProviderOptions.Authority = "https://localhost:5001/";
                options.ProviderOptions.ResponseType = "code";
                options.ProviderOptions.ResponseMode = "fragment";
                options.AuthenticationPaths.RemoteRegisterPath = "https://localhost:5001/Account/Register";
                options.AuthenticationPaths.RemoteProfilePath = "https://localhost:5001/Account/Manage";
            });

            var host = builder.Build();
            return host.RunAsync();
        }
    }
}
