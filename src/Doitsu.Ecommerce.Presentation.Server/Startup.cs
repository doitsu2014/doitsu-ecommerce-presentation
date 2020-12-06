using Doitsu.Ecommerce.Presentation.Server.Data;
using Doitsu.Ecommerce.Presentation.Server.Extensions;
using Doitsu.Ecommerce.Presentation.Server.Models;
using Doitsu.Ecommerce.Presentation.Server.Services;
using Doitsu.Ecommerce.Presentation.Server.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Doitsu.Ecommerce.Presentation.Shared.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Doitsu.Ecommerce.Presentation.Server
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Configure Settings
            services.Configure<SmtpClientSettings>(Configuration.GetSection(nameof(SmtpClientSettings)));

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                // Configure the context to use Microsoft SQL Server.
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));

                // Register the entity sets needed by OpenIddict.
                // Note: use the generic overload if you need
                // to replace the default OpenIddict entities.
                options.UseOpenIddict();
            });

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    // If this mode is true, system should implement the email sender service
                    // Note: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm
                    options.SignIn.RequireConfirmedAccount = true;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();
            services.AddTransient<IEmailSender, AuthMessageService>();

            // Configure Identity to use the same JWT claims as OpenIddict instead
            // of the legacy WS-Federation claims it uses by default (ClaimTypes),
            // which saves you from doing the mapping in your authorization controller.
            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = Claims.Role;
            });

            services.AddOpenIddict()

                // Register the OpenIddict core components.
                .AddCore(options =>
                 {
                     // Configure OpenIddict to use the Entity Framework Core stores and models.
                     // Note: call ReplaceDefaultEntities() to replace the default OpenIddict entities.
                     options.UseEntityFrameworkCore()
                            .UseDbContext<ApplicationDbContext>();
                 })

                // Register the OpenIddict server components.
                .AddServer(options =>
                {
                    // Enable the authorization, logout, token and userinfo endpoints.
                    options.SetAuthorizationEndpointUris("/connect/authorize")
                           .SetLogoutEndpointUris("/connect/logout")
                           .SetTokenEndpointUris("/connect/token")
                           .SetUserinfoEndpointUris("/connect/userinfo");

                    // Mark the "email", "profile" and "roles" scopes as supported scopes.
                    options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

                    // Note: the sample uses the code and refresh token flows but you can enable
                    // the other flows if you need to support implicit, password or client credentials.
                    options.AllowAuthorizationCodeFlow()
                           .AllowRefreshTokenFlow();

                    if (!Environment.IsDevelopment())
                    {
                        // Register the signing and encryption credentials.
                        options.AddDevelopmentEncryptionCertificate()
                            .AddDevelopmentSigningCertificate();
                    }
                    else
                    {
                        var certSection = Configuration.GetSection("OpenIdServerCertificate");
                        var x509 = new X509Certificate2(File.ReadAllBytes(certSection["FileName"]), certSection["Password"]);
                        options.AddSigningCertificate(x509)
                            .AddEncryptionCertificate(x509);
                    }

                    if (IsCluster())
                    {
                        services.Configure<ForwardedHeadersOptions>(options =>
                        {
                            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                        });
                    }

                    // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                    options.UseAspNetCore()
                           .EnableAuthorizationEndpointPassthrough()
                           .EnableLogoutEndpointPassthrough()
                           .EnableStatusCodePagesIntegration()
                           .EnableTokenEndpointPassthrough();
                })

                // Register the OpenIddict validation components.
                .AddValidation(options =>
                {
                    // Import the configuration from the local OpenIddict server instance.
                    options.UseLocalServer();

                    // Register the ASP.NET Core host.
                    options.UseAspNetCore();
                });

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    IConfigurationSection googleAuthNSection = Configuration.GetSection("Authentication:Google");
                    options.ClientId = googleAuthNSection["ClientId"];
                    options.ClientSecret = googleAuthNSection["ClientSecret"];
                });

            services.AddControllersWithViews();
            services.AddRazorPages();
            // Added by me
            services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
            
            // services.AddRemoteAuthentication<RemoteAuthenticationState, RemoteUserAccount, OidcProviderOptions>();
            // services.AddScoped<AuthenticationStateProvider, RemoteAuthenticationService>()
            //     .AddScoped<SignOutSessionStateManager>()
            //     .AddTransient<IAccessTokenProvider, AccessTokenProvider>()
            //     .AddTransient<Microsoft.JSInterop.IJSRuntime, JSRuntime>();
            
            // Added by me
            services.AddScoped<SignOutSessionStateManager>();
            services.AddSingleton<IWeatherForecastService, WeatherForecastService>();

            // Register the worker responsible of seeding the database with the sample clients.
            // Note: in a real world application, this step should be part of a setup script.
            services.AddHostedService<Worker>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            if (IsCluster())
            {
                app.Use((context, next) =>
                {
                    context.Request.Scheme = "https";
                    return next();
                });
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(options =>
            {
                options.MapRazorPages();
                options.MapControllers();
                options.MapFallbackToPage("/_ClientHost");
            });
        }

        private bool IsCluster()
        {
            return Configuration.GetValue<bool>("Operation:IsCluster");
        }
    }
}
