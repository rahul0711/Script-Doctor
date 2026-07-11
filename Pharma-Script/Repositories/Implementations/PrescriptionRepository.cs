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
    public class PrescriptionRepository : RepositoryBase<Prescription>, IPrescriptionRepository
    {
        public PrescriptionRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "Prescriptions";
        protected override string PrimaryKeyName => "PrescriptionID";

        protected override Prescription Map(DbDataReader reader)
        {
            return new Prescription
            {
                PrescriptionID = reader.GetInt32(reader.GetOrdinal("PrescriptionID")),
                AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID")),
                OrganizationID = reader.GetInt32(reader.GetOrdinal("OrganizationID")),
                DoctorID = reader.GetInt32(reader.GetOrdinal("DoctorID")),
                PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                PrescriptionNumber = reader.GetString(reader.GetOrdinal("PrescriptionNumber")),
                GeneralInstructions = reader.IsDBNull(reader.GetOrdinal("GeneralInstructions")) ? null : reader.GetString(reader.GetOrdinal("GeneralInstructions")),
                NextVisitDate = reader.IsDBNull(reader.GetOrdinal("NextVisitDate")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("NextVisitDate")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }

        private void AddParameters(MySqlCommand cmd, Prescription entity)
        {
            cmd.Parameters.AddWithValue("@AppointmentID", entity.AppointmentID);
            cmd.Parameters.AddWithValue("@OrganizationID", entity.OrganizationID);
            cmd.Parameters.AddWithValue("@DoctorID", entity.DoctorID);
            cmd.Parameters.AddWithValue("@PatientID", entity.PatientID);
            cmd.Parameters.AddWithValue("@PrescriptionNumber", entity.PrescriptionNumber);
            cmd.Parameters.AddWithValue("@GeneralInstructions", (object?)entity.GeneralInstructions ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NextVisitDate", (object?)entity.NextVisitDate ?? DBNull.Value);
        }

        public override async Task<int> AddAsync(Prescription entity)
        {
            var query = $@"INSERT INTO {TableName} 
                (AppointmentID, OrganizationID, DoctorID, PatientID, PrescriptionNumber, GeneralInstructions, NextVisitDate) 
                VALUES 
                (@AppointmentID, @OrganizationID, @DoctorID, @PatientID, @PrescriptionNumber, @GeneralInstructions, @NextVisitDate);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var id = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
        }

        public override async Task<bool> UpdateAsync(Prescription entity)
        {
            var query = $@"UPDATE {TableName} SET 
                GeneralInstructions = @GeneralInstructions, 
                NextVisitDate = @NextVisitDate
                WHERE PrescriptionID = @PrescriptionID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                cmd.Parameters.AddWithValue("@PrescriptionID", entity.PrescriptionID);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<Prescription?> GetByAppointmentIdAsync(int appointmentId, int? orgId)
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

        public async Task<Prescription?> GetByPrescriptionNumberAsync(string prescriptionNumber, int? orgId)
        {
            var query = $@"SELECT * FROM {TableName} WHERE PrescriptionNumber = @PrescriptionNumber";
            if (orgId.HasValue)
            {
                query += " AND OrganizationID = @OrganizationID";
            }

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@PrescriptionNumber", prescriptionNumber);
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

        public async Task<IEnumerable<Prescription>> GetHistoryByPatientIdAsync(int patientId, int? orgId)
        {
            var list = new List<Prescription>();
            var query = $@"SELECT * FROM {TableName} WHERE PatientID = @PatientID";
            if (orgId.HasValue)
            {
                query += " AND OrganizationID = @OrganizationID";
            }
            query += " ORDER BY CreatedAt DESC";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@PatientID", patientId);
                if (orgId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@OrganizationID", orgId.Value);
                }

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

        public async Task<string> GeneratePrescriptionNumberAsync()
        {
            var year = DateTime.Now.Year;
            var prefix = $"RX-{year}-";

            await EnsureConnectionOpenAsync();
            // Count existing ones with the current year's prefix
            var query = $"SELECT COUNT(*) FROM {TableName} WHERE PrescriptionNumber LIKE @Prefix";
            int count = 0;
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@Prefix", $"{prefix}%");
                count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }

            // Loop to prevent collision in case of deletes
            while (true)
            {
                count++;
                var number = $"{prefix}{count:D6}"; // RX-2026-000001
                
                var checkQuery = $"SELECT COUNT(*) FROM {TableName} WHERE PrescriptionNumber = @Num";
                using (var cmd = CreateCommand(checkQuery))
                {
                    cmd.Parameters.AddWithValue("@Num", number);
                    var exists = Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
                    if (!exists)
                    {
                        return number;
                    }
                }
            }
        }
    }
}
