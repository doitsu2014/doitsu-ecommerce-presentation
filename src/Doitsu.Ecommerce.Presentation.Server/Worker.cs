using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Doitsu.Ecosystem.ApplicationCore.Constants;
using Doitsu.Ecosystem.Infrastructure.Data;
using Doitsu.Ecosystem.ApplicationCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Doitsu.Ecommerce.Presentation.Server
{
    public class Worker : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public Worker(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync(cancellationToken);

            await RegisterDefaultScopesAsync(scope.ServiceProvider);
            await RegisterDefaultClientsAsync(scope.ServiceProvider);
            await RegisterDefaultUsersAsync(scope.ServiceProvider);
        }

        private async Task RegisterDefaultClientsAsync(IServiceProvider serviceProvider)
        {
            var applicationManager = serviceProvider
                .GetRequiredService<OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var clientSection = configuration.GetSection("Initial:OpenIdClient");

            if (await applicationManager.FindByClientIdAsync(clientSection["Admin:Id"]) is null)
            {
                await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = clientSection["Admin:Id"],
                    ConsentType = ConsentTypes.Explicit,
                    DisplayName = clientSection["Admin:DisplayName"],
                    Type = ClientTypes.Public,
                    PostLogoutRedirectUris =
                    {
                        new Uri($"{clientSection["Admin:Uri"]}/authentication/logout-callback")
                    },
                    RedirectUris =
                    {
                        new Uri($"{clientSection["Admin:Uri"]}/authentication/login-callback")
                    },
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Logout,
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.ResponseTypes.Code,
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,
                        $"{Permissions.Prefixes.Scope}{ScopeNameConstants.DoitsuEcommerceServicesAll}"
                    },
                    Requirements =
                    {
                        Requirements.Features.ProofKeyForCodeExchange
                    }
                });
            }

            if (await applicationManager.FindByClientIdAsync(clientSection["Customer:Id"]) is null)
            {
                await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = clientSection["Customer:Id"],
                    ConsentType = ConsentTypes.Explicit,
                    DisplayName = clientSection["Customer:DisplayName"],
                    Type = ClientTypes.Public,
                    PostLogoutRedirectUris =
                    {
                        new Uri($"{clientSection["Customer:Uri"]}/authentication/logout-callback")
                    },
                    RedirectUris =
                    {
                        new Uri($"{clientSection["Customer:Uri"]}/authentication/login-callback")
                    },
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Logout,
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.ResponseTypes.Code,
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,
                        $"{Permissions.Prefixes.Scope}{ScopeNameConstants.DoitsuEcommerceServicesAll}"
                    },
                    Requirements =
                    {
                        Requirements.Features.ProofKeyForCodeExchange
                    }
                });
            }
        }

        private async Task RegisterDefaultScopesAsync(IServiceProvider serviceProvider)
        {
            var scopeManager = serviceProvider.GetRequiredService<IOpenIddictScopeManager>();
            if (await scopeManager.FindByNameAsync(ScopeNameConstants.DoitsuEcommerceServicesAll) is null)
            {
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = ScopeNameConstants.DoitsuEcommerceServicesAll,
                    DisplayName = "Doitsu Ecommerce All Services",
                    DisplayNames =
                    {
                        [CultureInfo.GetCultureInfo("vn-VN")] = "Thương mại điện tử của Doitsu tất cả dịch vụ."
                    },
                    Resources =
                    {
                        ResourceNameConstants.ResourceBlogpost,
                        ResourceNameConstants.ResourceProduct,
                        ResourceNameConstants.ResourceDefault,
                        ResourceNameConstants.ResourceUser,
                        ResourceNameConstants.ResourceOrder
                    }
                });
            }
        }

        private async Task RegisterDefaultUsersAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var adminUserSection = configuration.GetSection("Initial:AdminUser");

            if (!userManager.Users.Any())
            {
                var adminUser = new ApplicationUser()
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = adminUserSection["EmailAddress"],
                    NormalizedEmail = adminUserSection["EmailAddress"].ToUpper(),
                    UserName = adminUserSection["EmailAddress"],
                    NormalizedUserName = adminUserSection["EmailAddress"].ToUpper(),
                    City = "HCM",
                    State = "HCM",
                    Country = "VN",
                    Name = "TRAN HUU DUC",
                    PhoneNumber = "0946680600"
                };
                await userManager.CreateAsync(adminUser, adminUserSection["Password"]);

                var listRoleNames = (new string[]
                {
                    IdentityRoleConstants.Admin,
                    IdentityRoleConstants.Customer
                });

                if (!roleManager.Roles.Any())
                {
                    var roles = listRoleNames.Select(r => new IdentityRole()
                        {Id = Guid.NewGuid().ToString(), Name = r, NormalizedName = r.ToUpper()});
                    foreach (var role in roles) await roleManager.CreateAsync(role);
                }

                await userManager.AddToRolesAsync(adminUser, listRoleNames);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}