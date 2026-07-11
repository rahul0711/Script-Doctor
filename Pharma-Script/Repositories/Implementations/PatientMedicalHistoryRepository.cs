using MySql.Data.MySqlClient;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Implementations
{
    public class PatientMedicalHistoryRepository : RepositoryBase<PatientMedicalHistory>, IPatientMedicalHistoryRepository
    {
        public PatientMedicalHistoryRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "PatientMedicalHistory";
        protected override string PrimaryKeyName => "MedicalHistoryID";

        protected override PatientMedicalHistory Map(DbDataReader reader)
        {
            return new PatientMedicalHistory
            {
                MedicalHistoryID = reader.GetInt32(reader.GetOrdinal("MedicalHistoryID")),
                PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                Diabetes = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("Diabetes"))),
                BloodPressure = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("BloodPressure"))),
                HeartDisease = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("HeartDisease"))),
                Asthma = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("Asthma"))),
                Thyroid = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("Thyroid"))),
                Allergies = reader.IsDBNull(reader.GetOrdinal("Allergies")) ? null : reader.GetString(reader.GetOrdinal("Allergies")),
                CurrentMedications = reader.IsDBNull(reader.GetOrdinal("CurrentMedications")) ? null : reader.GetString(reader.GetOrdinal("CurrentMedications")),
                PastSurgeries = reader.IsDBNull(reader.GetOrdinal("PastSurgeries")) ? null : reader.GetString(reader.GetOrdinal("PastSurgeries")),
                FamilyMedicalHistory = reader.IsDBNull(reader.GetOrdinal("FamilyMedicalHistory")) ? null : reader.GetString(reader.GetOrdinal("FamilyMedicalHistory")),
                OtherMedicalConditions = reader.IsDBNull(reader.GetOrdinal("OtherMedicalConditions")) ? null : reader.GetString(reader.GetOrdinal("OtherMedicalConditions")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }

        private void AddParameters(MySqlCommand cmd, PatientMedicalHistory entity)
        {
            cmd.Parameters.AddWithValue("@PatientID", entity.PatientID);
            cmd.Parameters.AddWithValue("@Diabetes", entity.Diabetes);
            cmd.Parameters.AddWithValue("@BloodPressure", entity.BloodPressure);
            cmd.Parameters.AddWithValue("@HeartDisease", entity.HeartDisease);
            cmd.Parameters.AddWithValue("@Asthma", entity.Asthma);
            cmd.Parameters.AddWithValue("@Thyroid", entity.Thyroid);
            cmd.Parameters.AddWithValue("@Allergies", (object?)entity.Allergies ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CurrentMedications", (object?)entity.CurrentMedications ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PastSurgeries", (object?)entity.PastSurgeries ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FamilyMedicalHistory", (object?)entity.FamilyMedicalHistory ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@OtherMedicalConditions", (object?)entity.OtherMedicalConditions ?? DBNull.Value);
        }

        public override async Task<int> AddAsync(PatientMedicalHistory entity)
        {
            var query = $@"INSERT INTO {TableName} 
                (PatientID, Diabetes, BloodPressure, HeartDisease, Asthma, Thyroid, Allergies, CurrentMedications, PastSurgeries, FamilyMedicalHistory, OtherMedicalConditions) 
                VALUES 
                (@PatientID, @Diabetes, @BloodPressure, @HeartDisease, @Asthma, @Thyroid, @Allergies, @CurrentMedications, @PastSurgeries, @FamilyMedicalHistory, @OtherMedicalConditions);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var id = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
        }

        public override async Task<bool> UpdateAsync(PatientMedicalHistory entity)
        {
            var query = $@"UPDATE {TableName} SET 
                Diabetes = @Diabetes, 
                BloodPressure = @BloodPressure, 
                HeartDisease = @HeartDisease, 
                Asthma = @Asthma, 
                Thyroid = @Thyroid, 
                Allergies = @Allergies, 
                CurrentMedications = @CurrentMedications, 
                PastSurgeries = @PastSurgeries, 
                FamilyMedicalHistory = @FamilyMedicalHistory, 
                OtherMedicalConditions = @OtherMedicalConditions
                WHERE MedicalHistoryID = @MedicalHistoryID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                cmd.Parameters.AddWithValue("@MedicalHistoryID", entity.MedicalHistoryID);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<PatientMedicalHistory?> GetByPatientIdAsync(int patientId)
        {
            var query = $"SELECT * FROM {TableName} WHERE PatientID = @PatientID";
            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@PatientID", patientId);
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
