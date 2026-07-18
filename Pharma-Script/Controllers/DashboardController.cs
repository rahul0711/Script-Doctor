using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharma_Script.Helpers;
using Pharma_Script.Repositories.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _uow;

        public DashboardController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IActionResult> Index()
        {
            var userId          = User.GetUserId();
            var isPatient       = User.GetRoleName().Equals("Patient", StringComparison.OrdinalIgnoreCase);

            // Patients don't get an internal dashboard — their history lives on the
            // hospital's own public website (/{slug}/my/appointments).
            if (isPatient)
            {
                var patientOrgId = User.GetOrganizationId();
                if (patientOrgId.HasValue)
                {
                    var org = await _uow.Organizations.GetByIdAsync(patientOrgId.Value);
                    if (org != null && !string.IsNullOrWhiteSpace(org.OrganizationSlug))
                    {
                        return Redirect($"/{org.OrganizationSlug}/my/appointments");
                    }
                }

                return RedirectToAction("Login", "Account");
            }

            var isPlatformOwner = User.IsPlatformOwner();
            var isOrgAdmin      = User.IsOrganizationAdmin();
            var isDoctor        = User.IsDoctor();
            var orgId           = User.GetOrganizationId();

            // ── Doctor Dashboard ──────────────────────────────────────────
            if (isDoctor)
            {
                var doctor = await _uow.Doctors.GetByUserIdAsync(userId);
                if (doctor == null)
                {
                    ViewBag.DoctorError = "Doctor profile not found.";
                    return View("DoctorDashboard");
                }

                var doctorId = doctor.DoctorID;
                var today    = DateTime.Today;

                // Today's appointments
                var todayAppointments = (await _uow.Appointments.SearchAndPaginateAsync(
                    orgId: null, branchId: null, doctorId: doctorId, patientId: null,
                    status: null, type: null,
                    startDate: today, endDate: today,
                    isPriority: null, searchTerm: null,
                    page: 1, pageSize: 50)).ToList();

                // Upcoming appointments (next 7 days, excluding today)
                var upcomingAppointments = await _uow.Appointments.GetSearchCountAsync(
                    orgId: null, branchId: null, doctorId: doctorId, patientId: null,
                    status: "Approved", type: null,
                    startDate: today.AddDays(1), endDate: today.AddDays(7),
                    isPriority: null, searchTerm: null);

                // Pending appointments
                var pendingCount = await _uow.Appointments.GetSearchCountAsync(
                    orgId: null, branchId: null, doctorId: doctorId, patientId: null,
                    status: "Pending", type: null,
                    startDate: null, endDate: null,
                    isPriority: null, searchTerm: null);

                // Total appointments ever (completed)
                var completedCount = await _uow.Appointments.GetSearchCountAsync(
                    orgId: null, branchId: null, doctorId: doctorId, patientId: null,
                    status: "Completed", type: null,
                    startDate: null, endDate: null,
                    isPriority: null, searchTerm: null);

                // Current calendar month's appointments - feeds the mini calendar (day highlighting +
                // click-to-filter) and the "next appointment" widget when it falls after today.
                var monthStart = new DateTime(today.Year, today.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                var monthAppointments = (await _uow.Appointments.SearchAndPaginateAsync(
                    orgId: null, branchId: null, doctorId: doctorId, patientId: null,
                    status: null, type: null,
                    startDate: monthStart, endDate: monthEnd,
                    isPriority: null, searchTerm: null,
                    page: 1, pageSize: 500)).ToList();

                ViewBag.Doctor               = doctor;
                ViewBag.TodayAppointments    = todayAppointments;
                ViewBag.TodayCount           = todayAppointments.Count;
                ViewBag.UpcomingCount        = upcomingAppointments;
                ViewBag.PendingCount         = pendingCount;
                ViewBag.CompletedCount       = completedCount;
                ViewBag.MonthAppointments    = monthAppointments;

                return View("DoctorDashboard");
            }

            // ── Admin / Platform Owner Dashboard ─────────────────────────
            int orgsCount    = 0;
            int branchesCount = 0;
            int deptsCount   = 0;
            int usersCount   = 0;
            int specsCount   = 0;
            int doctorsCount = 0;
            int patientsCount = 0;
            int appointmentsCount = 0;

            if (isPlatformOwner)
            {
                var orgList = await _uow.Organizations.GetAllAsync();
                orgsCount = orgList.Count();

                var branchList = await _uow.Branches.GetAllAsync();
                branchesCount = branchList.Count();

                var deptList = await _uow.Departments.GetAllAsync();
                deptsCount = deptList.Count();

                var userList = await _uow.Users.GetAllAsync();
                usersCount = userList.Count();

                doctorsCount = await _uow.Doctors.GetSearchCountAsync(null, null, null, null, null, string.Empty);
                patientsCount = await _uow.Patients.GetSearchCountAsync(null, null, string.Empty);
                appointmentsCount = await _uow.Appointments.GetSearchCountAsync(null, null, null, null, null, null, null, null, null, null);

                // Marketplace revenue widgets (platform-wide).
                ViewBag.TotalRevenue = await _uow.Payments.GetTotalRevenueAsync();
                ViewBag.TodayRevenue = await _uow.Payments.GetTodayRevenueAsync();
                ViewBag.PendingSettlementsCount = await _uow.Settlements.GetPendingCountAsync(null);
                ViewBag.CompletedSettlementsCount = await _uow.Settlements.GetCompletedCountAsync(null);
                ViewBag.RevenueSeries = await _uow.Payments.GetDailyRevenueAsync(14);
                ViewBag.RecentPayments = await _uow.Payments.SearchAndPaginateAsync(null, "Paid", null, 1, 5);
                ViewBag.RecentSettlements = await _uow.Settlements.GetRecentAsync(null, 5);
            }
            else if (orgId.HasValue)
            {
                orgsCount = 1;

                var branchList = await _uow.Branches.GetByOrganizationIdAsync(orgId.Value);
                branchesCount = branchList.Count();

                var deptList = await _uow.Departments.GetByOrganizationIdAsync(orgId.Value);
                deptsCount = deptList.Count();

                var userList = await _uow.Users.GetByOrganizationIdAsync(orgId.Value);
                usersCount = userList.Count();

                doctorsCount = await _uow.Doctors.GetSearchCountAsync(orgId.Value, null, null, null, null, string.Empty);
                patientsCount = await _uow.Patients.GetSearchCountAsync(orgId.Value, null, string.Empty);
                appointmentsCount = await _uow.Appointments.GetSearchCountAsync(orgId.Value, null, null, null, null, null, null, null, null, null);

                // Marketplace revenue widgets, scoped to this organization.
                ViewBag.TotalRevenue = await _uow.Payments.GetTotalOrganizationEarningsAsync(orgId.Value);
                ViewBag.TodayRevenue = await _uow.Payments.GetTodayOrganizationEarningsAsync(orgId.Value);
                ViewBag.PendingSettlementTotal = await _uow.Settlements.GetPendingTotalAsync(orgId.Value);
                ViewBag.LastSettlement = await _uow.Settlements.GetLastPaidAsync(orgId.Value);
                ViewBag.RecentPayments = await _uow.Payments.SearchAndPaginateAsync(orgId.Value, "Paid", null, 1, 5);
                ViewBag.RecentSettlements = await _uow.Settlements.GetRecentAsync(orgId.Value, 5);
            }

            var specList = await _uow.Specializations.GetAllAsync();
            specsCount = specList.Count();

            ViewBag.OrgsCount     = orgsCount;
            ViewBag.BranchesCount = branchesCount;
            ViewBag.DeptsCount    = deptsCount;
            ViewBag.UsersCount    = usersCount;
            ViewBag.SpecsCount    = specsCount;
            ViewBag.DoctorsCount  = doctorsCount;
            ViewBag.PatientsCount = patientsCount;
            ViewBag.AppointmentsCount = appointmentsCount;

            return View();
        }
    }
}
