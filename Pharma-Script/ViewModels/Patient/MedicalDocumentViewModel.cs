using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.Patient
{
    public class MedicalDocumentViewModel
    {
        public int DocumentID { get; set; }

        [Required(ErrorMessage = "Patient is required.")]
        public int PatientID { get; set; }

        [Required(ErrorMessage = "Document Title is required.")]
        [StringLength(200, ErrorMessage = "Document Title cannot exceed 200 characters.")]
        [Display(Name = "Document Title")]
        public string DocumentTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Document Type is required.")]
        [Display(Name = "Document Type")]
        public string DocumentType { get; set; } = string.Empty; // Prescription, Blood Report, etc.

        [Display(Name = "Upload File")]
        public IFormFile? File { get; set; }
    }
}
