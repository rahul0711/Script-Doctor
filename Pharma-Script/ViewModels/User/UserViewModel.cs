using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.User
{
    public class UserViewModel
    {
        public int UserID { get; set; }

        [Display(Name = "Organization")]
        public int? OrganizationID { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        [Display(Name = "Role")]
        public int RoleID { get; set; }

        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(100, ErrorMessage = "First Name cannot exceed 100 characters.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Last Name cannot exceed 100 characters.")]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        public string Phone { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Is Active?")]
        public bool IsActive { get; set; } = true;
    }
}
