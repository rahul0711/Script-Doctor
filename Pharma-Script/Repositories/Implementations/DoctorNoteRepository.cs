using MySql.Data.MySqlClient;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Implementations
{
    public class DoctorNoteRepository : RepositoryBase<DoctorNote>, IDoctorNoteRepository
    {
        public DoctorNoteRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "DoctorNotes";
        protected override string PrimaryKeyName => "NoteID";

        protected override DoctorNote Map(DbDataReader reader)
        {
            return new DoctorNote
            {
                NoteID = reader.GetInt32(reader.GetOrdinal("NoteID")),
                AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID")),
                OrganizationID = reader.GetInt32(reader.GetOrdinal("OrganizationID")),
                DoctorID = reader.GetInt32(reader.GetOrdinal("DoctorID")),
                PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                ClinicalNotes = reader.IsDBNull(reader.GetOrdinal("ClinicalNotes")) ? null : reader.GetString(reader.GetOrdinal("ClinicalNotes")),
                Diagnosis = reader.IsDBNull(reader.GetOrdinal("Diagnosis")) ? null : reader.GetString(reader.GetOrdinal("Diagnosis")),
                Advice = reader.IsDBNull(reader.GetOrdinal("Advice")) ? null : reader.GetString(reader.GetOrdinal("Advice")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }

        private void AddParameters(MySqlCommand cmd, DoctorNote entity)
        {
            cmd.Parameters.AddWithValue("@AppointmentID", entity.AppointmentID);
            cmd.Parameters.AddWithValue("@OrganizationID", entity.OrganizationID);
            cmd.Parameters.AddWithValue("@DoctorID", entity.DoctorID);
            cmd.Parameters.AddWithValue("@PatientID", entity.PatientID);
            cmd.Parameters.AddWithValue("@ClinicalNotes", (object?)entity.ClinicalNotes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Diagnosis", (object?)entity.Diagnosis ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Advice", (object?)entity.Advice ?? DBNull.Value);
        }

        public override async Task<int> AddAsync(DoctorNote entity)
        {
            var query = $@"INSERT INTO {TableName} 
                (AppointmentID, OrganizationID, DoctorID, PatientID, ClinicalNotes, Diagnosis, Advice) 
                VALUES 
                (@AppointmentID, @OrganizationID, @DoctorID, @PatientID, @ClinicalNotes, @Diagnosis, @Advice);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var id = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
        }

        public override async Task<bool> UpdateAsync(DoctorNote entity)
        {
            var query = $@"UPDATE {TableName} SET 
                ClinicalNotes = @ClinicalNotes, 
                Diagnosis = @Diagnosis, 
                Advice = @Advice
                WHERE NoteID = @NoteID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                cmd.Parameters.AddWithValue("@NoteID", entity.NoteID);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<DoctorNote?> GetByAppointmentIdAsync(int appointmentId, int? orgId)
        {
            var query = $@"SELECT * FROM {TableName} WHERE AppointmentID = @AppointmentID";
            if (orgId.HasValue)
            {
                query += " AND OrganizationID = @OrganizationID";
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
    }
}
