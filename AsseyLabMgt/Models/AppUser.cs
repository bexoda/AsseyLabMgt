using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel;  // Needed for the DisplayName attribute

namespace AsseyLabMgt.Models
{
    public class AppUser : IdentityUser
    {
        [DisplayName("First Name")]
        public string? FirstName { get; set; }

        [DisplayName("Other Name")]
        public string? OtherName { get; set; }

        [DisplayName("Last Name")]
        public string? LastName { get; set; }

        [DisplayName("Created By")]
        public string? CreatdBy { get; set; }

        [DisplayName("Creation Date")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [DisplayName("Full Name")]
        public string? FullName
        {
            get
            {
                return $"{FirstName} {OtherName} {LastName}".Trim();
            }
        }

        [DisplayName("Last Login Date")]
        public DateTime? LoginDate { get; set; }

        [DisplayName("Modified By")]
        public string? ModifiedBy { get; }
    }
}
