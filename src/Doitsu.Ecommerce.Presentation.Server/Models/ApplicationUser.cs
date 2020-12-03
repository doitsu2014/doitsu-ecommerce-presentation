using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Doitsu.Ecommerce.Presentation.Server.Models
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(512)]
        public string Avatar { get; set; }
        [MaxLength(256)]
        public string Street { get; set; }
        [MaxLength(64)]
        public string City { get; set; }
        [MaxLength(32)]
        public string State { get; set; }
        [MaxLength(32)]
        public string Country { get; set; }
        [MaxLength(32)]
        public string ZipCode { get; set; }
        [MaxLength(256)]
        public string Name { get; set; }
        [MaxLength(256)]
        public string LastName { get; set; }
    } 
}
