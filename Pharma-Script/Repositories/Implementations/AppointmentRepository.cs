using MySql.Data.MySqlClient;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Implementations
{
    public class AppointmentRepository : RepositoryBase<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "Appointments";
        protected override string PrimaryKeyName => "AppointmentID";

        public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsForRemindersAsync(DateTime date)
        {
            var query = @"
                SELECT a.*
                FROM Appointments a
                WHERE a.Status = 'Approved' 
                  AND a.AppointmentDate = @Date
                  AND NOT EXISTS (
                      SELECT 1 FROM Notifications n 
                      WHERE n.RelatedEntityID = a.AppointmentID 
                        AND n.RelatedEntityType = 'AppointmentID' 
                        AND n.NotificationType = 'Reminder'
                  )";

            var list = new List<Appointment>();
            using var cmd = new MySqlCommand(query, (MySqlConnection)Connection, (MySqlTransaction?)TransactionProvider?.Invoke());
            cmd.Parameters.AddWithValue("@Date", date.Date);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(Map(reader));
            }
            return list;
        }

        protected override Appointment Map(DbDataReader reader)
        {
            var appointment = new Appointment
            {
                AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID")),
                OrganizationID = reader.GetInt32(reader.GetOrdinal("OrganizationID")),
                BranchID = reader.IsDBNull(reader.GetOrdinal("BranchID")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("BranchID")),
                DoctorID = reader.GetInt32(reader.GetOrdinal("DoctorID")),
                PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                AppointmentType = reader.GetString(reader.GetOrdinal("AppointmentType")),
                AppointmentDate = reader.GetDateTime(reader.GetOrdinal("AppointmentDate")),
                StartTime = reader.GetFieldValue<TimeSpan>(reader.GetOrdinal("StartTime")),
                EndTime = reader.GetFieldValue<TimeSpan>(reader.GetOrdinal("EndTime")),
                ConsultationFee = reader.GetDecimal(reader.GetOrdinal("ConsultationFee")),
                PriorityConsultation = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("PriorityConsultation"))),
                Symptoms = reader.IsDBNull(reader.GetOrdinal("Symptoms")) ? null : reader.GetString(reader.GetOrdinal("Symptoms")),
                AppointmentReason = reader.IsDBNull(reader.GetOrdinal("AppointmentReason")) ? null : reader.GetString(reader.GetOrdinal("AppointmentReason")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };

            // Dynamic safe reading of joined properties if they exist in output reader
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var col = reader.GetName(i);
                if (col.Equals("DoctorName", StringComparison.OrdinalIgnoreCase))
                {
                    appointment.DoctorName = reader.IsDBNull(i) ? null : reader.GetString(i);
                }
                else if (col.Equals("DoctorEmail", StringComparison.OrdinalIgnoreCase))
                {
                    appointment.DoctorEmail = reader.IsDBNull(i) ? null : reader.GetString(i);
                }
                else if (col.Equals("DoctorPhone", StringComparison.OrdinalIgnoreCase))
                {
                    appointment.DoctorPhone = reader.IsDBNull(i) ? null : reader.GetString(i);
                }
                else if (col.Equals("PatientName", StringComparison.OrdinalIgnoreCase))
                {
                    appointment.PatientName = reader.IsDBNull(i) ? null : reader.GetString(i);
                }
                else if (col.Equals("PatientGender", StringComparison.OrdinalIgnoreCase))
                {
                    appointment.PatientGender = reader.IsDBNull(i) ? null : reader.GetString(i);
                }
                else if (col.Equals("PatientBloodGroup", StringComparison.OrdinalIgnoreCase))
                {
                    appointment.PatientBloodGroup = reader.IsDBNull(i) ? null : reader.GetString(i);
                }
                else if (col.Equals("PatientDOB", StringComparison.OrdinalIgnoreCase))
                {
                    appointment.PatientDOB = reader.IsDBNull(i) ? null : (DateTime?)reader.GetDateTime(i);
                }
                else if (col.Equals("BranchName", StringComparison.OrdinalIgnoreCase))
                {
                    appointment.BranchName = reader.IsDBNull(i) ? null : reader.GetString(i);
                }
                else if (col.Equals("OrganizationName", StringComparison.OrdinalIgnoreCase))
                {
                    appointment.OrganizationName = reader.IsDBNull(i) ? null : reader.GetString(i);
                }
                else if (col.Equals("PaymentStatus", StringComparison.OrdinalIgnoreCase))
                {
                    appointment.PaymentStatus = reader.IsDBNull(i) ? null : reader.GetString(i);
                }
            }

            return appointment;
        }

        private void AddParameters(MySqlCommand cmd, Appointment entity)
        {
            cmd.Parameters.AddWithValue("@OrganizationID", entity.OrganizationID);
            cmd.Parameters.AddWithValue("@BranchID", (object?)entity.BranchID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DoctorID", entity.DoctorID);
            cmd.Parameters.AddWithValue("@PatientID", entity.PatientID);
            cmd.Parameters.AddWithValue("@AppointmentType", entity.AppointmentType);
            cmd.Parameters.AddWithValue("@AppointmentDate", entity.AppointmentDate.Date);
            cmd.Parameters.AddWithValue("@StartTime", entity.StartTime);
            cmd.Parameters.AddWithValue("@EndTime", entity.EndTime);
            cmd.Parameters.AddWithValue("@ConsultationFee", entity.ConsultationFee);
            cmd.Parameters.AddWithValue("@PriorityConsultation", entity.PriorityConsultation);
            cmd.Parameters.AddWithValue("@Symptoms", (object?)entity.Symptoms ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AppointmentReason", (object?)entity.AppointmentReason ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", entity.Status);
            cmd.Parameters.AddWithValue("@Notes", (object?)entity.Notes ?? DBNull.Value);
        }

        public override async Task<int> AddAsync(Appointment entity)
        {
            var query = $@"INSERT INTO {TableName} 
                (OrganizationID, BranchID, DoctorID, PatientID, AppointmentType, AppointmentDate, StartTime, EndTime, ConsultationFee, PriorityConsultation, Symptoms, AppointmentReason, Status, Notes) 
                VALUES 
                (@OrganizationID, @BranchID, @DoctorID, @PatientID, @AppointmentType, @AppointmentDate, @StartTime, @EndTime, @ConsultationFee, @PriorityConsultation, @Symptoms, @AppointmentReason, @Status, @Notes);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var id = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
        }

        public override async Task<bool> UpdateAsync(Appointment entity)
        {
            var query = $@"UPDATE {TableName} SET 
                BranchID = @BranchID, 
                DoctorID = @DoctorID, 
                PatientID = @PatientID, 
                AppointmentType = @AppointmentType, 
                AppointmentDate = @AppointmentDate, 
                StartTime = @StartTime, 
                EndTime = @EndTime, 
                ConsultationFee = @ConsultationFee, 
                PriorityConsultation = @PriorityConsultation, 
                Symptoms = @Symptoms, 
                AppointmentReason = @AppointmentReason, 
                Status = @Status, 
                Notes = @Notes
                WHERE AppointmentID = @AppointmentID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                cmd.Parameters.AddWithValue("@AppointmentID", entity.AppointmentID);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<bool> UpdateStatusAsync(int appointmentId, string status)
        {
            var query = $"UPDATE {TableName} SET Status = @Status WHERE AppointmentID = @AppointmentID";
            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@AppointmentID", appointmentId);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<IEnumerable<Appointment>> GetBookedSlotsAsync(int doctorId, DateTime date)
        {
            var list = new List<Appointment>();
            var query = $@"SELECT * FROM {TableName} 
                           WHERE DoctorID = @DoctorID 
                             AND AppointmentDate = @AppointmentDate 
                             AND Status NOT IN ('Cancelled', 'Rejected')";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@DoctorID", doctorId);
                cmd.Parameters.AddWithValue("@AppointmentDate", date.Date);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(Map(reader));
                    }
                }
            }
            return list;
        }

        public async Task<bool> CheckSlotConflictAsync(int doctorId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeAppointmentId)
        {
            var query = $@"SELECT COUNT(*) FROM {TableName} 
                           WHERE DoctorID = @DoctorID 
                             AND AppointmentDate = @AppointmentDate 
                             AND Status NOT IN ('Cancelled', 'Rejected') 
                             AND (
                                 (StartTime <= @StartTime AND EndTime > @StartTime) OR
                                 (StartTime < @EndTime AND EndTime >= @EndTime) OR
                                 (StartTime >= @StartTime AND EndTime <= @EndTime)
                             )";

            if (excludeAppointmentId.HasValue)
            {
                query += " AND AppointmentID != @ExcludeID";
            }

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@DoctorID", doctorId);
                cmd.Parameters.AddWithValue("@AppointmentDate", date.Date);
                cmd.Parameters.AddWithValue("@StartTime", startTime);
                cmd.Parameters.AddWithValue("@EndTime", endTime);
                if (excludeAppointmentId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@ExcludeID", excludeAppointmentId.Value);
                }

                var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return count > 0;
            }
        }

        public async Task<Appointment?> GetAppointmentDetailsByIdAsync(int id, int? orgId)
        {
            var query = $@"SELECT a.*,
                                  CONCAT(ud.FirstName, ' ', IFNULL(ud.LastName, '')) AS DoctorName,
                                  ud.Email AS DoctorEmail,
                                  ud.Phone AS DoctorPhone,
                                  CONCAT(up.FirstName, ' ', IFNULL(up.LastName, '')) AS PatientName,
                                  p.Gender AS PatientGender,
                                  p.BloodGroup AS PatientBloodGroup,
                                  p.DateOfBirth AS PatientDOB,
                                  b.BranchName,
                                  o.OrganizationName,
                                  (SELECT pay.PaymentStatus FROM Payments pay WHERE pay.AppointmentID = a.AppointmentID ORDER BY pay.PaymentID DESC LIMIT 1) AS PaymentStatus
                           FROM {TableName} a
                           INNER JOIN Doctors d ON a.DoctorID = d.DoctorID
                           INNER JOIN Users ud ON d.UserID = ud.UserID
                           INNER JOIN Patients p ON a.PatientID = p.PatientID
                           INNER JOIN Users up ON p.UserID = up.UserID
                           LEFT JOIN Branches b ON a.BranchID = b.BranchID
                           INNER JOIN Organizations o ON a.OrganizationID = o.OrganizationID
                           WHERE a.AppointmentID = @AppointmentID";

            if (orgId.HasValue)
            {
                query += " AND a.OrganizationID = @OrganizationID";
            }

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@AppointmentID", id);
                if (orgId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@OrganizationID", orgId.Value);
                }

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return Map(reader);
                    }
                }
            }
            return null;
        }

        public async Task<IEnumerable<Appointment>> SearchAndPaginateAsync(
            int? orgId, int? branchId, int? doctorId, int? patientId, 
            string? status, string? type, DateTime? startDate, DateTime? endDate, 
            bool? isPriority, string? searchTerm, int page, int pageSize)
        {
            var list = new List<Appointment>();
            var sb = new StringBuilder($@"SELECT a.*,
                                                CONCAT(ud.FirstName, ' ', IFNULL(ud.LastName, '')) AS DoctorName,
                                                CONCAT(up.FirstName, ' ', IFNULL(up.LastName, '')) AS PatientName,
                                                b.BranchName,
                                                (SELECT pay.PaymentStatus FROM Payments pay WHERE pay.AppointmentID = a.AppointmentID ORDER BY pay.PaymentID DESC LIMIT 1) AS PaymentStatus
                                         FROM {TableName} a
                                         INNER JOIN Doctors d ON a.DoctorID = d.DoctorID
                                         INNER JOIN Users ud ON d.UserID = ud.UserID
                                         INNER JOIN Patients p ON a.PatientID = p.PatientID
                                         INNER JOIN Users up ON p.UserID = up.UserID
                                         LEFT JOIN Branches b ON a.BranchID = b.BranchID
                                         WHERE 1=1");

            var cmd = Connection.CreateCommand();
            var tx = Transaction;
            if (tx != null) cmd.Transaction = tx;

            BuildFilterQuery(sb, cmd, orgId, branchId, doctorId, patientId, status, type, startDate, endDate, isPriority, searchTerm);

            // Pagination
            sb.Append(" ORDER BY a.AppointmentDate DESC, a.StartTime DESC LIMIT @Limit OFFSET @Offset");
            cmd.Parameters.AddWithValue("@Limit", pageSize);
            cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);

            cmd.CommandText = sb.ToString();
            await EnsureConnectionOpenAsync();
            using (cmd)
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    list.Add(Map(reader));
                }
            }
            return list;
        }

        public async Task<int> GetSearchCountAsync(
            int? orgId, int? branchId, int? doctorId, int? patientId, 
            string? status, string? type, DateTime? startDate, DateTime? endDate, 
            bool? isPriority, string? searchTerm)
        {
            var sb = new StringBuilder($@"SELECT COUNT(*)
                                         FROM {TableName} a
                                         INNER JOIN Doctors d ON a.DoctorID = d.DoctorID
                                         INNER JOIN Users ud ON d.UserID = ud.UserID
                                         INNER JOIN Patients p ON a.PatientID = p.PatientID
                                         INNER JOIN Users up ON p.UserID = up.UserID
                                         LEFT JOIN Branches b ON a.BranchID = b.BranchID
                                         WHERE 1=1");

            var cmd = Connection.CreateCommand();
            var tx = Transaction;
            if (tx != null) cmd.Transaction = tx;

            BuildFilterQuery(sb, cmd, orgId, branchId, doctorId, patientId, status, type, startDate, endDate, isPriority, searchTerm);

            cmd.CommandText = sb.ToString();
            await EnsureConnectionOpenAsync();
            using (cmd)
            {
                return Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
        }

        private void BuildFilterQuery(
            StringBuilder sb, MySqlCommand cmd, 
            int? orgId, int? branchId, int? doctorId, int? patientId, 
            string? status, string? type, DateTime? startDate, DateTime? endDate, 
            bool? isPriority, string? searchTerm)
        {
            if (orgId.HasValue)
            {
                sb.Append(" AND a.OrganizationID = @OrgID");
                cmd.Parameters.AddWithValue("@OrgID", orgId.Value);
            }
            if (branchId.HasValue)
            {
                sb.Append(" AND a.BranchID = @BranchID");
                cmd.Parameters.AddWithValue("@BranchID", branchId.Value);
            }
            if (doctorId.HasValue)
            {
                sb.Append(" AND a.DoctorID = @DoctorID");
                cmd.Parameters.AddWithValue("@DoctorID", doctorId.Value);
            }
            if (patientId.HasValue)
            {
                sb.Append(" AND a.PatientID = @PatientID");
                cmd.Parameters.AddWithValue("@PatientID", patientId.Value);
            }
            if (!string.IsNullOrEmpty(status))
            {
                sb.Append(" AND a.Status = @Status");
                cmd.Parameters.AddWithValue("@Status", status);
            }
            if (!string.IsNullOrEmpty(type))
            {
                sb.Append(" AND a.AppointmentType = @Type");
                cmd.Parameters.AddWithValue("@Type", type);
            }
            if (startDate.HasValue)
            {
                sb.Append(" AND a.AppointmentDate >= @StartDate");
                cmd.Parameters.AddWithValue("@StartDate", startDate.Value.Date);
            }
            if (endDate.HasValue)
            {
                sb.Append(" AND a.AppointmentDate <= @EndDate");
                cmd.Parameters.AddWithValue("@EndDate", endDate.Value.Date);
            }
            if (isPriority.HasValue)
            {
                sb.Append(" AND a.PriorityConsultation = @IsPriority");
                cmd.Parameters.AddWithValue("@IsPriority", isPriority.Value);
            }
            if (!string.IsNullOrEmpty(searchTerm))
            {
                sb.Append(" AND (ud.FirstName LIKE @Term OR ud.LastName LIKE @Term OR up.FirstName LIKE @Term OR up.LastName LIKE @Term OR a.Symptoms LIKE @Term OR a.AppointmentReason LIKE @Term)");
                cmd.Parameters.AddWithValue("@Term", $"%{searchTerm}%");
            }
        }

        public async Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId, int? orgId)
        {
            var list = new List<Appointment>();
            var query = $@"SELECT a.*,
                                   CONCAT(ud.FirstName, ' ', IFNULL(ud.LastName, '')) AS DoctorName,
                                   CONCAT(up.FirstName, ' ', IFNULL(up.LastName, '')) AS PatientName,
                                   pat.Gender AS PatientGender,
                                   pat.BloodGroup AS PatientBloodGroup,
                                   pat.DateOfBirth AS PatientDOB,
                                   b.BranchName,
                                   o.OrganizationName,
                                   py.PaymentStatus
                            FROM Appointments a
                            INNER JOIN Doctors d ON a.DoctorID = d.DoctorID
                            INNER JOIN Users ud ON d.UserID = ud.UserID
                            INNER JOIN Patients pat ON a.PatientID = pat.PatientID
                            INNER JOIN Users up ON pat.UserID = up.UserID
                            LEFT JOIN Branches b ON a.BranchID = b.BranchID
                            LEFT JOIN Organizations o ON a.OrganizationID = o.OrganizationID
                            LEFT JOIN Payments py ON py.AppointmentID = a.AppointmentID
                            WHERE a.DoctorID = @DoctorID";

            if (orgId.HasValue)
                query += " AND a.OrganizationID = @OrgID";

            query += " ORDER BY a.AppointmentDate DESC";

            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(query);
            cmd.Parameters.AddWithValue("@DoctorID", doctorId);
            if (orgId.HasValue)
                cmd.Parameters.AddWithValue("@OrgID", orgId.Value);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                list.Add(Map(reader));
            return list;
        }

        public async Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId, int? orgId)
        {
            var list = new List<Appointment>();
            var query = $@"SELECT a.*,
                                   CONCAT(ud.FirstName, ' ', IFNULL(ud.LastName, '')) AS DoctorName,
                                   CONCAT(up.FirstName, ' ', IFNULL(up.LastName, '')) AS PatientName,
                                   pat.Gender AS PatientGender,
                                   pat.BloodGroup AS PatientBloodGroup,
                                   pat.DateOfBirth AS PatientDOB,
                                   b.BranchName,
                                   o.OrganizationName,
                                   py.PaymentStatus
                            FROM Appointments a
                            INNER JOIN Doctors d ON a.DoctorID = d.DoctorID
                            INNER JOIN Users ud ON d.UserID = ud.UserID
                            INNER JOIN Patients pat ON a.PatientID = pat.PatientID
                            INNER JOIN Users up ON pat.UserID = up.UserID
                            LEFT JOIN Branches b ON a.BranchID = b.BranchID
                            LEFT JOIN Organizations o ON a.OrganizationID = o.OrganizationID
                            LEFT JOIN Payments py ON py.AppointmentID = a.AppointmentID
                            WHERE a.PatientID = @PatientID";

            if (orgId.HasValue)
                query += " AND a.OrganizationID = @OrgID";

            query += " ORDER BY a.AppointmentDate DESC";

            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(query);
            cmd.Parameters.AddWithValue("@PatientID", patientId);
            if (orgId.HasValue)
                cmd.Parameters.AddWithValue("@OrgID", orgId.Value);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                list.Add(Map(reader));
            return list;
        }
    }
}
