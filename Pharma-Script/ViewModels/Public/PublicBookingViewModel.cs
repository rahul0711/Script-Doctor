using Pharma_Script.Models;
using Pharma_Script.ViewModels.Appointment;

namespace Pharma_Script.ViewModels.Public
{
    public class PublicBookingViewModel
    {
        public PublicTenant Tenant { get; set; } = null!;
        public Pharma_Script.Models.Doctor Doctor { get; set; } = null!;
        public AppointmentBookingViewModel Booking { get; set; } = null!;
        public bool PaymentGatewayAvailable { get; set; }
    }
}
