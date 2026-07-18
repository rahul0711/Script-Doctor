using System;
using System.Threading.Tasks;

namespace Pharma_Script.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendVideoConsultationLinkEmailAsync(
            string patientEmail,
            string patientName,
            int? patientAge,
            string doctorName,
            string meetingUrl,
            string meetingProvider,
            DateTime appointmentDate,
            TimeSpan startTime);
    }
}
