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
    public class PatientVitalsRepository : RepositoryBase<PatientVitals>, IPatientVitalsRepository
    {
        public PatientVitalsRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "PatientVitals";
        protected override string PrimaryKeyName => "VitalID";

        protected override PatientVitals Map(DbDataReader reader)
        {
            return new PatientVitals
            {
                VitalID = reader.GetInt32(reader.GetOrdinal("VitalID")),
                PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                Height = reader.IsDBNull(reader.GetOrdinal("Height")) ? null : (decimal?)reader.GetDecimal(reader.GetOrdinal("Height")),
                Weight = reader.IsDBNull(reader.GetOrdinal("Weight")) ? null : (decimal?)reader.GetDecimal(reader.GetOrdinal("Weight")),
                BloodPressure = reader.IsDBNull(reader.GetOrdinal("BloodPressure")) ? null : reader.GetString(reader.GetOrdinal("BloodPressure")),
                HeartRate = reader.IsDBNull(reader.GetOrdinal("HeartRate")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("HeartRate")),
                Temperature = reader.IsDBNull(reader.GetOrdinal("Temperature")) ? null : (decimal?)reader.GetDecimal(reader.GetOrdinal("Temperature")),
                OxygenLevel = reader.IsDBNull(reader.GetOrdinal("OxygenLevel")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("OxygenLevel")),
                BloodSugar = reader.IsDBNull(reader.GetOrdinal("BloodSugar")) ? null : reader.GetString(reader.GetOrdinal("BloodSugar")),
                BMI = reader.IsDBNull(reader.GetOrdinal("BMI")) ? null : (decimal?)reader.GetDecimal(reader.GetOrdinal("BMI")),
                RecordedAt = reader.GetDateTime(reader.GetOrdinal("RecordedAt"))
            };
        }

        private void AddParameters(MySqlCommand cmd, PatientVitals entity)
        {
            cmd.Parameters.AddWithValue("@PatientID", entity.PatientID);
            cmd.Parameters.AddWithValue("@Height", (object?)entity.Height ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Weight", (object?)entity.Weight ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BloodPressure", (object?)entity.BloodPressure ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@HeartRate", (object?)entity.HeartRate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Temperature", (object?)entity.Temperature ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@OxygenLevel", (object?)entity.OxygenLevel ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BloodSugar", (object?)entity.BloodSugar ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BMI", (object?)entity.BMI ?? DBNull.Value);
        }

        public override async Task<int> AddAsync(PatientVitals entity)
        {
            var query = $@"INSERT INTO {TableName} 
                (PatientID, Height, Weight, BloodPressure, HeartRate, Temperature, OxygenLevel, BloodSugar, BMI) 
                VALUES 
                (@PatientID, @Height, @Weight, @BloodPressure, @HeartRate, @Temperature, @OxygenLevel, @BloodSugar, @BMI);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var id = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
        }

        public override async Task<bool> UpdateAsync(PatientVitals entity)
        {
            var query = $@"UPDATE {TableName} SET 
                Height = @Height, 
                Weight = @Weight, 
                BloodPressure = @BloodPressure, 
                HeartRate = @HeartRate, 
                Temperature = @Temperature, 
                OxygenLevel = @OxygenLevel, 
                BloodSugar = @BloodSugar, 
                BMI = @BMI
                WHERE VitalID = @VitalID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                cmd.Parameters.AddWithValue("@VitalID", entity.VitalID);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<PatientVitals?> GetLatestByPatientIdAsync(int patientId)
        {
            var query = $"SELECT * FROM {TableName} WHERE PatientID = @PatientID ORDER BY RecordedAt DESC LIMIT 1";
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

        public async Task<IEnumerable<PatientVitals>> GetHistoryByPatientIdAsync(int patientId)
        {
            var list = new List<PatientVitals>();
            var query = $"SELECT * FROM {TableName} WHERE PatientID = @PatientID ORDER BY RecordedAt DESC";
            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@PatientID", patientId);
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
    }
}
