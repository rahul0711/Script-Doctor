using MySql.Data.MySqlClient;
using Pharma_Script.Repositories.Interfaces;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MySqlConnection _connection;
        private MySqlTransaction? _transaction;
        private bool _disposed;

        public IOrganizationRepository Organizations { get; }
        public IBranchRepository Branches { get; }
        public IDepartmentRepository Departments { get; }
        public IRoleRepository Roles { get; }
        public IUserRepository Users { get; }
        public ISpecializationRepository Specializations { get; }
        public IDoctorRepository Doctors { get; }
        public IDoctorPaymentGatewayRepository DoctorPaymentGateways { get; }
        public IDoctorAvailabilityRepository DoctorAvailabilities { get; }
        public IDoctorLeaveRepository DoctorLeaves { get; }
        public IReceptionistRepository Receptionists { get; }
        public IPatientRepository Patients { get; }
        public IPatientMedicalHistoryRepository PatientMedicalHistories { get; }
        public IPatientVitalsRepository PatientVitals { get; }
        public IMedicalDocumentRepository MedicalDocuments { get; }
        public IAppointmentRepository Appointments { get; }
        public IAppointmentStatusHistoryRepository AppointmentStatusHistories { get; }
        public IPaymentRepository Payments { get; }
        public IDoctorNoteRepository DoctorNotes { get; }
        public IPrescriptionRepository Prescriptions { get; }
        public IPrescriptionMedicineRepository PrescriptionMedicines { get; }
        public IFollowUpRepository FollowUps { get; }
        public ICMSSettingRepository CMSSettings { get; }
        public IHeroSectionRepository HeroSections { get; }
        public IServiceRepository Services { get; }
        public IContactMessageRepository ContactMessages { get; }
        public IConsultationSessionRepository ConsultationSessions { get; }
        public INotificationRepository Notifications { get; }

        public UnitOfWork(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

            _connection = new MySqlConnection(connectionString);

            // Pass the connection and a delegate that resolves the active transaction dynamically
            Organizations = new OrganizationRepository(_connection, () => _transaction);
            Branches = new BranchRepository(_connection, () => _transaction);
            Departments = new DepartmentRepository(_connection, () => _transaction);
            Roles = new RoleRepository(_connection, () => _transaction);
            Users = new UserRepository(_connection, () => _transaction);
            Specializations = new SpecializationRepository(_connection, () => _transaction);
            Doctors = new DoctorRepository(_connection, () => _transaction);
            DoctorPaymentGateways = new DoctorPaymentGatewayRepository(_connection, () => _transaction);
            DoctorAvailabilities = new DoctorAvailabilityRepository(_connection, () => _transaction);
            DoctorLeaves = new DoctorLeaveRepository(_connection, () => _transaction);
            Receptionists = new ReceptionistRepository(_connection, () => _transaction);
            Patients = new PatientRepository(_connection, () => _transaction);
            PatientMedicalHistories = new PatientMedicalHistoryRepository(_connection, () => _transaction);
            PatientVitals = new PatientVitalsRepository(_connection, () => _transaction);
            MedicalDocuments = new MedicalDocumentRepository(_connection, () => _transaction);
            Appointments = new AppointmentRepository(_connection, () => _transaction);
            AppointmentStatusHistories = new AppointmentStatusHistoryRepository(_connection, () => _transaction);
            Payments = new PaymentRepository(_connection, () => _transaction);
            DoctorNotes = new DoctorNoteRepository(_connection, () => _transaction);
            Prescriptions = new PrescriptionRepository(_connection, () => _transaction);
            PrescriptionMedicines = new PrescriptionMedicineRepository(_connection, () => _transaction);
            FollowUps = new FollowUpRepository(_connection, () => _transaction);
            CMSSettings = new CMSSettingRepository(_connection, () => _transaction);
            HeroSections = new HeroSectionRepository(_connection, () => _transaction);
            Services = new ServiceRepository(_connection, () => _transaction);
            ContactMessages = new ContactMessageRepository(_connection, () => _transaction);
            ConsultationSessions = new ConsultationSessionRepository(_connection, () => _transaction);
            Notifications = new NotificationRepository(_connection, () => _transaction);
        }

        public async Task OpenConnectionAsync()
        {
            if (_connection.State != ConnectionState.Open)
            {
                await _connection.OpenAsync();
            }
        }

        public async Task BeginTransactionAsync()
        {
            await OpenConnectionAsync();
            _transaction = await _connection.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _connection?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
