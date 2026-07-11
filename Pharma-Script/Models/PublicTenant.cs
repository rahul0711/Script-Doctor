namespace Pharma_Script.Models
{
    public class PublicTenant
    {
        public Organization Organization { get; set; } = null!;
        public CMSSetting? CMSSettings { get; set; }
    }
}
