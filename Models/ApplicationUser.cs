using Microsoft.AspNetCore.Identity;

namespace TaskManager.Models
{
    public enum UserRole
    {
        Administrator,
        Manager,
        User
    }
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.User;
    }
}
