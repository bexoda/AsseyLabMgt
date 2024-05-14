using Microsoft.AspNetCore.Identity;

namespace AsseyLabMgt.Models
{
    public class AppUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? OtherName { get; set; }
        public string? LastName { get; set; }
        public string? CreatdBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public string? FullName
        {
            get
            {
                return $"{FirstName} {OtherName} {LastName}".Trim();
            }
        }

        public DateTime? LoginDate { get; set; }
        public string? ModifiedBy { get; }
        public string? RoleId { get; set; }
        public IdentityRole? Role { get; set; }

    }
}
