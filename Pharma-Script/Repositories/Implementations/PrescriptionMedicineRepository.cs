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
    public class PrescriptionMedicineRepository : RepositoryBase<PrescriptionMedicine>, IPrescriptionMedicineRepository
    {
        public PrescriptionMedicineRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "PrescriptionMedicines";
        protected override string PrimaryKeyName => "PrescriptionMedicineID";

        protected override PrescriptionMedicine Map(DbDataReader reader)
        {
            return new PrescriptionMedicine
            {
                PrescriptionMedicineID = reader.GetInt32(reader.GetOrdinal("PrescriptionMedicineID")),
                PrescriptionID = reader.GetInt32(reader.GetOrdinal("PrescriptionID")),
                MedicineName = reader.GetString(reader.GetOrdinal("MedicineName")),
                Strength = reader.IsDBNull(reader.GetOrdinal("Strength")) ? null : reader.GetString(reader.GetOrdinal("Strength")),
                Dosage = reader.IsDBNull(reader.GetOrdinal("Dosage")) ? null : reader.GetString(reader.GetOrdinal("Dosage")),
                Morning = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("Morning"))),
                Afternoon = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("Afternoon"))),
                Night = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("Night"))),
                BeforeFood = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("BeforeFood"))),
                AfterFood = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("AfterFood"))),
                DurationDays = reader.GetInt32(reader.GetOrdinal("DurationDays")),
                Quantity = reader.IsDBNull(reader.GetOrdinal("Quantity")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("Quantity")),
                Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks"))
            };
        }

        private void AddParameters(MySqlCommand cmd, PrescriptionMedicine entity)
        {
            cmd.Parameters.AddWithValue("@PrescriptionID", entity.PrescriptionID);
            cmd.Parameters.AddWithValue("@MedicineName", entity.MedicineName);
            cmd.Parameters.AddWithValue("@Strength", (object?)entity.Strength ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Dosage", (object?)entity.Dosage ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Morning", entity.Morning);
            cmd.Parameters.AddWithValue("@Afternoon", entity.Afternoon);
            cmd.Parameters.AddWithValue("@Night", entity.Night);
            cmd.Parameters.AddWithValue("@BeforeFood", entity.BeforeFood);
            cmd.Parameters.AddWithValue("@AfterFood", entity.AfterFood);
            cmd.Parameters.AddWithValue("@DurationDays", entity.DurationDays);
            cmd.Parameters.AddWithValue("@Quantity", (object?)entity.Quantity ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Remarks", (object?)entity.Remarks ?? DBNull.Value);
        }

        public override async Task<int> AddAsync(PrescriptionMedicine entity)
        {
            var query = $@"INSERT INTO {TableName} 
                (PrescriptionID, MedicineName, Strength, Dosage, Morning, Afternoon, Night, BeforeFood, AfterFood, DurationDays, Quantity, Remarks) 
                VALUES 
                (@PrescriptionID, @MedicineName, @Strength, @Dosage, @Morning, @Afternoon, @Night, @BeforeFood, @AfterFood, @DurationDays, @Quantity, @Remarks);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var id = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
        }

        public override async Task<bool> UpdateAsync(PrescriptionMedicine entity)
        {
            var query = $@"UPDATE {TableName} SET 
                MedicineName = @MedicineName, 
                Strength = @Strength, 
                Dosage = @Dosage, 
                Morning = @Morning, 
                Afternoon = @Afternoon, 
                Night = @Night, 
                BeforeFood = @BeforeFood, 
                AfterFood = @AfterFood, 
                DurationDays = @DurationDays, 
                Quantity = @Quantity, 
                Remarks = @Remarks
                WHERE PrescriptionMedicineID = @PrescriptionMedicineID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                cmd.Parameters.AddWithValue("@PrescriptionMedicineID", entity.PrescriptionMedicineID);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<IEnumerable<PrescriptionMedicine>> GetByPrescriptionIdAsync(int prescriptionId)
        {
            var list = new List<PrescriptionMedicine>();
            var query = $@"SELECT * FROM {TableName} WHERE PrescriptionID = @PrescriptionID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@PrescriptionID", prescriptionId);
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

        public async Task<bool> DeleteByPrescriptionIdAsync(int prescriptionId)
        {
            var query = $"DELETE FROM {TableName} WHERE PrescriptionID = @PrescriptionID";
            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@PrescriptionID", prescriptionId);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }
    }
}
