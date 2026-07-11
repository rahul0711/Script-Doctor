using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.Public
{
    public class PublicContactFormViewModel
    {
        [Required(ErrorMessage = "Please enter your name.")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(150)]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Enter a valid phone number.")]
        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(250)]
        public string? Subject { get; set; }

        [Required(ErrorMessage = "Please enter a message.")]
        public string Message { get; set; } = string.Empty;

        // Honeypot - real visitors never see or fill this field.
        public string? Website { get; set; }
    }
}
