using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Doitsu.Ecommerce.Presentation.Server.Constants;
using Doitsu.Ecommerce.Presentation.Server.Data;
using Doitsu.Ecommerce.Presentation.Server.Models;
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
            await context.Database.EnsureCreatedAsync();

            await RegisterDefaultClientsAsync(scope.ServiceProvider);
            await RegisterDefaultUsersAsync(scope.ServiceProvider);
        }

        private async Task RegisterDefaultClientsAsync(IServiceProvider serviceProvider)
        {
            var manager = serviceProvider.GetRequiredService<OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication>>();
            if (await manager.FindByClientIdAsync("balosar-blazor-client") is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "balosar-blazor-client",
                    ConsentType = ConsentTypes.Explicit,
                    DisplayName = "Blazor client application",
                    Type = ClientTypes.Public,
                    PostLogoutRedirectUris =
                    {
                        new Uri("https://localhost:5001/authentication/logout-callback")
                    },
                    RedirectUris =
                    {
                        new Uri("https://localhost:5001/authentication/login-callback")
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
                        Permissions.Scopes.Roles
                    },
                    Requirements =
                    {
                        Requirements.Features.ProofKeyForCodeExchange
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

            if (userManager.Users.Count() == 0)
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
                    IdentityRoleConstants.ADMIN,
                    IdentityRoleConstants.CUSTOMER,
                    IdentityRoleConstants.BLOG_MANAGER,
                    IdentityRoleConstants.BLOG_PUBLISHER,
                    IdentityRoleConstants.BLOG_WRITER
                });
                if (roleManager.Roles.Count() == 0)
                {
                    var roles = listRoleNames.Select(r => new IdentityRole() { Id = Guid.NewGuid().ToString(), Name = r, NormalizedName = r.ToUpper() });
                    foreach (var role in roles) await roleManager.CreateAsync(role);
                }

                await userManager.AddToRolesAsync(adminUser, listRoleNames);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
