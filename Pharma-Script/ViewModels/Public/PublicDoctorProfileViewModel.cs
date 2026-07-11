using Pharma_Script.Models;
using System.Collections.Generic;
using System.Linq;

namespace Pharma_Script.ViewModels.Public
{
    public class PublicDoctorProfileViewModel
    {
        public PublicTenant Tenant { get; set; } = null!;
        public Pharma_Script.Models.Doctor Doctor { get; set; } = null!;
        public List<DoctorAvailability> Availability { get; set; } = new();

        private static readonly string[] WeekOrder =
        {
            "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
        };

        public IEnumerable<IGrouping<string, DoctorAvailability>> AvailabilityByDay =>
            Availability
                .Where(a => a.IsAvailable)
                .OrderBy(a => System.Array.IndexOf(WeekOrder, a.DayOfWeek))
                .ThenBy(a => a.StartTime)
                .GroupBy(a => a.DayOfWeek);
    }
}
