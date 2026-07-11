using System;

namespace Pharma_Script.Models
{
    public class MedicalDocument
    {
        public int DocumentID { get; set; }
        public int PatientID { get; set; }
        public int OrganizationID { get; set; }
        public int UploadedByUserID { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty; // Prescription, Blood Report, etc.
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long? FileSize { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.Now;

        // Joined fields for display
        public string? UploadedByUserName { get; set; }
    }
}
