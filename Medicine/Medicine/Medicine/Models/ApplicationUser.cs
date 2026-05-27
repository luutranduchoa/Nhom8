using Microsoft.AspNetCore.Identity;

namespace Medicine.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
