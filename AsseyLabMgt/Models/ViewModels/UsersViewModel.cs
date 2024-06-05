using System.ComponentModel.DataAnnotations;

namespace AsseyLabMgt.Models.ViewModels
{
    public class UsersViewModel
    {
        public string? Id { get; set; }

        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Other Name")]
        public string OtherName { get; set; }

        [Display(Name = "Surname")]
        public string Surname { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Password")]
        public string Password { get; set; }

        public string? FullName
        {
            get
            {
                return $"{FirstName} {OtherName} {Surname}".Trim();
            }
        }

        [Display(Name = "Role")]
        public string? RoleId { get; set; }
    }
}
