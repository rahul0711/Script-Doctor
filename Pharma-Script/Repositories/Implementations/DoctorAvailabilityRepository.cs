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
    public class DoctorAvailabilityRepository : RepositoryBase<DoctorAvailability>, IDoctorAvailabilityRepository
    {
        public DoctorAvailabilityRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "DoctorAvailability";
        protected override string PrimaryKeyName => "AvailabilityID";

        protected override DoctorAvailability Map(DbDataReader reader)
        {
            return new DoctorAvailability
            {
                AvailabilityID = reader.GetInt32(reader.GetOrdinal("AvailabilityID")),
                DoctorID = reader.GetInt32(reader.GetOrdinal("DoctorID")),
                DayOfWeek = reader.GetString(reader.GetOrdinal("DayOfWeek")),
                StartTime = reader.GetFieldValue<TimeSpan>(reader.GetOrdinal("StartTime")),
                EndTime = reader.GetFieldValue<TimeSpan>(reader.GetOrdinal("EndTime")),
                SlotDuration = reader.GetInt32(reader.GetOrdinal("SlotDuration")),
                BreakStart = reader.IsDBNull(reader.GetOrdinal("BreakStart")) ? null : (TimeSpan?)reader.GetFieldValue<TimeSpan>(reader.GetOrdinal("BreakStart")),
                BreakEnd = reader.IsDBNull(reader.GetOrdinal("BreakEnd")) ? null : (TimeSpan?)reader.GetFieldValue<TimeSpan>(reader.GetOrdinal("BreakEnd")),
                IsAvailable = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("IsAvailable")))
            };
        }

        private void AddParameters(MySqlCommand cmd, DoctorAvailability entity)
        {
            cmd.Parameters.AddWithValue("@DoctorID", entity.DoctorID);
            cmd.Parameters.AddWithValue("@DayOfWeek", entity.DayOfWeek);
            cmd.Parameters.AddWithValue("@StartTime", entity.StartTime);
            cmd.Parameters.AddWithValue("@EndTime", entity.EndTime);
            cmd.Parameters.AddWithValue("@SlotDuration", entity.SlotDuration);
            cmd.Parameters.AddWithValue("@BreakStart", (object?)entity.BreakStart ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BreakEnd", (object?)entity.BreakEnd ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsAvailable", entity.IsAvailable);
        }

        public override async Task<int> AddAsync(DoctorAvailability entity)
        {
            var query = $@"INSERT INTO {TableName} 
                (DoctorID, DayOfWeek, StartTime, EndTime, SlotDuration, BreakStart, BreakEnd, IsAvailable) 
                VALUES 
                (@DoctorID, @DayOfWeek, @StartTime, @EndTime, @SlotDuration, @BreakStart, @BreakEnd, @IsAvailable);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var id = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
        }

        public override async Task<bool> UpdateAsync(DoctorAvailability entity)
        {
            var query = $@"UPDATE {TableName} SET 
                DayOfWeek = @DayOfWeek, 
                StartTime = @StartTime, 
                EndTime = @EndTime, 
                SlotDuration = @SlotDuration, 
                BreakStart = @BreakStart, 
                BreakEnd = @BreakEnd, 
                IsAvailable = @IsAvailable
                WHERE AvailabilityID = @AvailabilityID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                cmd.Parameters.AddWithValue("@AvailabilityID", entity.AvailabilityID);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<IEnumerable<DoctorAvailability>> GetAvailabilityByDoctorIdAsync(int doctorId)
        {
            var list = new List<DoctorAvailability>();
            var query = $"SELECT * FROM {TableName} WHERE DoctorID = @DoctorID ORDER BY FIELD(DayOfWeek, 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'), StartTime ASC";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@DoctorID", doctorId);
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

        public async Task<bool> CheckOverlapAsync(int doctorId, string dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? excludeId)
        {
            var query = $@"SELECT COUNT(*) FROM {TableName} 
                           WHERE DoctorID = @DoctorID 
                           AND DayOfWeek = @DayOfWeek 
                           AND StartTime < @EndTime 
                           AND EndTime > @StartTime";

            if (excludeId.HasValue)
            {
                query += " AND AvailabilityID != @ExcludeId";
            }

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@DoctorID", doctorId);
                cmd.Parameters.AddWithValue("@DayOfWeek", dayOfWeek);
                cmd.Parameters.AddWithValue("@StartTime", startTime);
                cmd.Parameters.AddWithValue("@EndTime", endTime);
                if (excludeId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@ExcludeId", excludeId.Value);
                }

                var count = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(count) > 0;
            }
        }
    }
}
