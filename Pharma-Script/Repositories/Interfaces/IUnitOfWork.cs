using System;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IOrganizationRepository Organizations { get; }
        IBranchRepository Branches { get; }
        IDepartmentRepository Departments { get; }
        IRoleRepository Roles { get; }
        IUserRepository Users { get; }
        ISpecializationRepository Specializations { get; }
        IDoctorRepository Doctors { get; }
        IDoctorAvailabilityRepository DoctorAvailabilities { get; }
        IDoctorLeaveRepository DoctorLeaves { get; }
        IReceptionistRepository Receptionists { get; }
        IPatientRepository Patients { get; }
        IPatientMedicalHistoryRepository PatientMedicalHistories { get; }
        IPatientVitalsRepository PatientVitals { get; }
        IMedicalDocumentRepository MedicalDocuments { get; }
        IAppointmentRepository Appointments { get; }
        IAppointmentStatusHistoryRepository AppointmentStatusHistories { get; }
        IPaymentRepository Payments { get; }
        IDoctorNoteRepository DoctorNotes { get; }
        IPrescriptionRepository Prescriptions { get; }
        IPrescriptionMedicineRepository PrescriptionMedicines { get; }
        IFollowUpRepository FollowUps { get; }
        ICMSSettingRepository CMSSettings { get; }
        IHeroSectionRepository HeroSections { get; }
        IServiceRepository Services { get; }
        IContactMessageRepository ContactMessages { get; }
        IConsultationSessionRepository ConsultationSessions { get; }
        INotificationRepository Notifications { get; }
        ISettlementRepository Settlements { get; }

        Task OpenConnectionAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
