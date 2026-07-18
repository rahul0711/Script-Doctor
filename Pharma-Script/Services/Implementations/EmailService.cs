using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Pharma_Script.Services.Interfaces;

namespace Pharma_Script.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendVideoConsultationLinkEmailAsync(
            string patientEmail,
            string patientName,
            int? patientAge,
            string doctorName,
            string meetingUrl,
            string meetingProvider,
            DateTime appointmentDate,
            TimeSpan startTime)
        {
            if (string.IsNullOrWhiteSpace(patientEmail)) return;

            var smtpSection = _config.GetSection("Smtp");
            var host = smtpSection["Host"] ?? "smtp.gmail.com";
            var port = int.TryParse(smtpSection["Port"], out var p) ? p : 587;
            var enableSsl = bool.TryParse(smtpSection["EnableSsl"], out var ssl) ? ssl : true;
            var user = smtpSection["User"];
            var password = smtpSection["Password"];
            var fromName = smtpSection["FromName"] ?? "Cloud Doctor";

            using var smtpClient = new SmtpClient(host)
            {
                Port = port,
                EnableSsl = enableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(user, password)
            };

            var fromAddress = new MailAddress(user!, fromName);
            using var message = new MailMessage
            {
                From = fromAddress,
                Subject = $"Your Video Consultation Link with Dr. {doctorName}",
                IsBodyHtml = true,
                Body = BuildEmailBody(patientName, patientAge, doctorName, meetingUrl, meetingProvider, appointmentDate, startTime)
            };
            message.To.Add(patientEmail);

            await smtpClient.SendMailAsync(message);
        }

        private static string BuildEmailBody(
            string patientName,
            int? patientAge,
            string doctorName,
            string meetingUrl,
            string meetingProvider,
            DateTime appointmentDate,
            TimeSpan startTime)
        {
            var ageText = patientAge.HasValue ? $"{patientAge.Value} years" : "N/A";
            var slotText = DateTime.Today.Add(startTime).ToString("hh:mm tt");
            var dateText = appointmentDate.ToString("dd MMM, yyyy");

            return @"<!DOCTYPE html>
<html>
<head>
<meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
<meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
<title>Cloud Doctor</title>
<style type=""text/css"">
#templateContainer { width: 100%; max-width: 600px; padding: 0; margin: 20px auto 0; background-color: #fff; box-sizing: border-box; }
.Content_block { display: block; box-sizing: border-box; width: 100%; margin-top: 0; line-height: 1.6; padding: 20px; border-top: 5px solid #2b6cb0; }
a.btn { display: inline-block; background: #2b6cb0; color: #fff !important; text-decoration: none; padding: 12px 24px; border-radius: 6px; font-weight: 600; margin-top: 10px; }
a.btn:hover { opacity: .85; }
</style>
</head>
<body style=""font-family: calibri,helvetica,arial; font-size: 14px; background-color: #efefef; padding: 0; margin: 0; box-sizing: border-box"">
<div id=""templateContainer"">
    <div class=""Content_block"">
        <p style=""margin: 0 0 10px; font-size: 18px; font-family: calibri,arial,helvetica;"">Your Video Consultation is Confirmed</p>
        <p style=""color:#555;"">Dear " + patientName + @",</p>
        <p style=""color:#555;"">Your video consultation with <strong>Dr. " + doctorName + @"</strong> has been scheduled. Please find your appointment details below and use the link at the time of your consultation to join.</p>

        <table style=""display: table; width: 100%; border-collapse: collapse; margin-top: 15px;"">
            <tr>
                <td style=""color:#222; width:140px; text-transform:uppercase; font-weight:600; border:1px solid #efefef; padding:8px 10px;"">Doctor</td>
                <td style=""color:#777; border:1px solid #efefef; padding:8px 10px;"">Dr. " + doctorName + @"</td>
            </tr>
            <tr>
                <td style=""color:#222; width:140px; text-transform:uppercase; font-weight:600; border:1px solid #efefef; padding:8px 10px;"">Patient</td>
                <td style=""color:#777; border:1px solid #efefef; padding:8px 10px;"">" + patientName + @"</td>
            </tr>
            <tr>
                <td style=""color:#222; width:140px; text-transform:uppercase; font-weight:600; border:1px solid #efefef; padding:8px 10px;"">Age</td>
                <td style=""color:#777; border:1px solid #efefef; padding:8px 10px;"">" + ageText + @"</td>
            </tr>
            <tr>
                <td style=""color:#222; width:140px; text-transform:uppercase; font-weight:600; border:1px solid #efefef; padding:8px 10px;"">Date</td>
                <td style=""color:#777; border:1px solid #efefef; padding:8px 10px;"">" + dateText + @"</td>
            </tr>
            <tr>
                <td style=""color:#222; width:140px; text-transform:uppercase; font-weight:600; border:1px solid #efefef; padding:8px 10px;"">Time</td>
                <td style=""color:#777; border:1px solid #efefef; padding:8px 10px;"">" + slotText + @"</td>
            </tr>
            <tr>
                <td style=""color:#222; width:140px; text-transform:uppercase; font-weight:600; border:1px solid #efefef; padding:8px 10px;"">Platform</td>
                <td style=""color:#777; border:1px solid #efefef; padding:8px 10px;"">" + meetingProvider + @"</td>
            </tr>
        </table>

        <div style=""text-align:center; margin: 25px 0 10px;"">
            <a class=""btn"" href=""" + meetingUrl + @""" target=""_blank"">Join Video Consultation</a>
        </div>
        <p style=""color:#999; font-size:12px;"">If the button above doesn't work, copy and paste this link into your browser:<br/>" + meetingUrl + @"</p>
    </div>
    <div style=""float: left; width: 100%; color: #fff; font-size: 13px; text-align: center; margin: 0; box-sizing: border-box; padding:10px; background: #1F2E67;"">
        <span>&copy; Cloud Doctor</span>
    </div>
</div>
</body>
</html>";
        }
    }
}
