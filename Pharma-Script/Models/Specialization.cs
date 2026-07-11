namespace Pharma_Script.Models
{
    public class Specialization
    {
        public int SpecializationID { get; set; }
        public string SpecializationName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
