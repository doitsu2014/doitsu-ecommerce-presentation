using Doitsu.Ecosystem.ApplicationCore.Constants;
using Doitsu.Ecosystem.ApplicationCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Doitsu.Ecommerce.Presentation.Server.Extensions
{
    public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        public ApplicationUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor
        ) : base(userManager, roleManager, optionsAccessor)
        {
        }

        public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
        {
            var principal = await base.CreateAsync(user);
            var listAdditionalClaims = new (string key, string value)[] {
                (ClaimTypeConstants.AVATAR, user.Avatar),
                (ClaimTypeConstants.ADDRESS_STREET, user.Street),
                (ClaimTypeConstants.ADDRESS_COUNTRY, user.Country),
                (ClaimTypeConstants.ADDRESS_STATE, user.State),
                (ClaimTypeConstants.ADDRESS_CITY, user.City),
                (ClaimTypeConstants.ADDRESS_ZIP_CODE, user.ZipCode),
            }
            .Where(kv => !string.IsNullOrEmpty(kv.value))
            .Select(kv => new Claim(kv.key, kv.value))
            .ToArray();
            
            ((ClaimsIdentity)principal.Identity)?.AddClaims(listAdditionalClaims);
            
            return principal;
        }

    }
}

