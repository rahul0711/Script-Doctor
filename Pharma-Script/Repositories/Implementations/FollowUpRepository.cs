using MySql.Data.MySqlClient;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Implementations
{
    public class FollowUpRepository : RepositoryBase<FollowUp>, IFollowUpRepository
    {
        public FollowUpRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "FollowUps";
        protected override string PrimaryKeyName => "FollowUpID";

        protected override FollowUp Map(DbDataReader reader)
        {
            var followUp = new FollowUp
            {
                FollowUpID = reader.GetInt32(reader.GetOrdinal("FollowUpID")),
                AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID")),
                OrganizationID = reader.GetInt32(reader.GetOrdinal("OrganizationID")),
                DoctorID = reader.GetInt32(reader.GetOrdinal("DoctorID")),
                PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                FollowUpDate = reader.GetDateTime(reader.GetOrdinal("FollowUpDate")),
                Reason = reader.IsDBNull(reader.GetOrdinal("Reason")) ? null : reader.GetString(reader.GetOrdinal("Reason")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var col = reader.GetName(i);
                if (col.Equals("DoctorName", StringComparison.OrdinalIgnoreCase))
                {
                    followUp.DoctorName = reader.IsDBNull(i) ? null : reader.GetString(i);
                }
                else if (col.Equals("PatientName", StringComparison.OrdinalIgnoreCase))
                {
                    followUp.PatientName = reader.IsDBNull(i) ? null : reader.GetString(i);
                }
            }

            return followUp;
        }

        private void AddParameters(MySqlCommand cmd, FollowUp entity)
        {
            cmd.Parameters.AddWithValue("@AppointmentID", entity.AppointmentID);
            cmd.Parameters.AddWithValue("@OrganizationID", entity.OrganizationID);
            cmd.Parameters.AddWithValue("@DoctorID", entity.DoctorID);
            cmd.Parameters.AddWithValue("@PatientID", entity.PatientID);
            cmd.Parameters.AddWithValue("@FollowUpDate", entity.FollowUpDate.Date);
            cmd.Parameters.AddWithValue("@Reason", (object?)entity.Reason ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", entity.Status);
        }

        public override async Task<int> AddAsync(FollowUp entity)
        {
            var query = $@"INSERT INTO {TableName} 
                (AppointmentID, OrganizationID, DoctorID, PatientID, FollowUpDate, Reason, Status) 
                VALUES 
                (@AppointmentID, @OrganizationID, @DoctorID, @PatientID, @FollowUpDate, @Reason, @Status);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var id = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
        }

        public override async Task<bool> UpdateAsync(FollowUp entity)
        {
            var query = $@"UPDATE {TableName} SET 
                FollowUpDate = @FollowUpDate, 
                Reason = @Reason, 
                Status = @Status
                WHERE FollowUpID = @FollowUpID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                cmd.Parameters.AddWithValue("@FollowUpID", entity.FollowUpID);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<FollowUp?> GetByAppointmentIdAsync(int appointmentId, int? orgId)
        {
            var query = $@"SELECT f.*,
                                  CONCAT(ud.FirstName, ' ', IFNULL(ud.LastName, '')) AS DoctorName,
                                  CONCAT(up.FirstName, ' ', IFNULL(up.LastName, '')) AS PatientName
                           FROM {TableName} f
                           INNER JOIN Doctors d ON f.DoctorID = d.DoctorID
                           INNER JOIN Users ud ON d.UserID = ud.UserID
                           INNER JOIN Patients p ON f.PatientID = p.PatientID
                           INNER JOIN Users up ON p.UserID = up.UserID
                           WHERE f.AppointmentID = @AppointmentID";

            if (orgId.HasValue)
            {
                query += " AND f.OrganizationID = @OrganizationID";
            }

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@AppointmentID", appointmentId);
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

        public async Task<IEnumerable<FollowUp>> GetUpcomingFollowUpsAsync(int? orgId, int? doctorId, int? patientId)
        {
            var list = new List<FollowUp>();
            var query = $@"SELECT f.*,
                                  CONCAT(ud.FirstName, ' ', IFNULL(ud.LastName, '')) AS DoctorName,
                                  CONCAT(up.FirstName, ' ', IFNULL(up.LastName, '')) AS PatientName
                           FROM {TableName} f
                           INNER JOIN Doctors d ON f.DoctorID = d.DoctorID
                           INNER JOIN Users ud ON d.UserID = ud.UserID
                           INNER JOIN Patients p ON f.PatientID = p.PatientID
                           INNER JOIN Users up ON p.UserID = up.UserID
                           WHERE f.Status = 'Pending' AND f.FollowUpDate >= CURDATE()";

            if (orgId.HasValue)
            {
                query += " AND f.OrganizationID = @OrgID";
            }
            if (doctorId.HasValue)
            {
                query += " AND f.DoctorID = @DoctorID";
            }
            if (patientId.HasValue)
            {
                query += " AND f.PatientID = @PatientID";
            }
            query += " ORDER BY f.FollowUpDate ASC";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                if (orgId.HasValue) cmd.Parameters.AddWithValue("@OrgID", orgId.Value);
                if (doctorId.HasValue) cmd.Parameters.AddWithValue("@DoctorID", doctorId.Value);
                if (patientId.HasValue) cmd.Parameters.AddWithValue("@PatientID", patientId.Value);

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

        public async Task<bool> UpdateStatusAsync(int followUpId, string status, int? orgId)
        {
            var query = $"UPDATE {TableName} SET Status = @Status WHERE FollowUpID = @FollowUpID";
            if (orgId.HasValue)
            {
                query += " AND OrganizationID = @OrganizationID";
            }

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@FollowUpID", followUpId);
                if (orgId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@OrganizationID", orgId.Value);
                }

                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }
    }
}
