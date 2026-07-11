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
    public class DoctorLeaveRepository : RepositoryBase<DoctorLeave>, IDoctorLeaveRepository
    {
        public DoctorLeaveRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "DoctorLeave";
        protected override string PrimaryKeyName => "LeaveID";

        protected override DoctorLeave Map(DbDataReader reader)
        {
            return new DoctorLeave
            {
                LeaveID = reader.GetInt32(reader.GetOrdinal("LeaveID")),
                DoctorID = reader.GetInt32(reader.GetOrdinal("DoctorID")),
                LeaveStartDate = reader.GetDateTime(reader.GetOrdinal("LeaveStartDate")),
                LeaveEndDate = reader.GetDateTime(reader.GetOrdinal("LeaveEndDate")),
                Reason = reader.IsDBNull(reader.GetOrdinal("Reason")) ? null : reader.GetString(reader.GetOrdinal("Reason"))
            };
        }

        private void AddParameters(MySqlCommand cmd, DoctorLeave entity)
        {
            cmd.Parameters.AddWithValue("@DoctorID", entity.DoctorID);
            cmd.Parameters.AddWithValue("@LeaveStartDate", entity.LeaveStartDate.Date);
            cmd.Parameters.AddWithValue("@LeaveEndDate", entity.LeaveEndDate.Date);
            cmd.Parameters.AddWithValue("@Reason", (object?)entity.Reason ?? DBNull.Value);
        }

        public override async Task<int> AddAsync(DoctorLeave entity)
        {
            var query = $@"INSERT INTO {TableName} 
                (DoctorID, LeaveStartDate, LeaveEndDate, Reason) 
                VALUES 
                (@DoctorID, @LeaveStartDate, @LeaveEndDate, @Reason);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var id = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
        }

        public override async Task<bool> UpdateAsync(DoctorLeave entity)
        {
            var query = $@"UPDATE {TableName} SET 
                LeaveStartDate = @LeaveStartDate, 
                LeaveEndDate = @LeaveEndDate, 
                Reason = @Reason
                WHERE LeaveID = @LeaveID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                cmd.Parameters.AddWithValue("@LeaveID", entity.LeaveID);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<IEnumerable<DoctorLeave>> GetUpcomingLeavesByDoctorIdAsync(int doctorId)
        {
            var list = new List<DoctorLeave>();
            var query = $"SELECT * FROM {TableName} WHERE DoctorID = @DoctorID AND LeaveEndDate >= CURDATE() ORDER BY LeaveStartDate ASC";

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

        public async Task<IEnumerable<DoctorLeave>> GetPastLeavesByDoctorIdAsync(int doctorId)
        {
            var list = new List<DoctorLeave>();
            var query = $"SELECT * FROM {TableName} WHERE DoctorID = @DoctorID AND LeaveEndDate < CURDATE() ORDER BY LeaveStartDate DESC";

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

        public async Task<bool> CheckLeaveOverlapAsync(int doctorId, DateTime startDate, DateTime endDate, int? excludeId)
        {
            var query = $@"SELECT COUNT(*) FROM {TableName} 
                           WHERE DoctorID = @DoctorID 
                           AND LeaveStartDate <= @EndDate 
                           AND LeaveEndDate >= @StartDate";

            if (excludeId.HasValue)
            {
                query += " AND LeaveID != @ExcludeId";
            }

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@DoctorID", doctorId);
                cmd.Parameters.AddWithValue("@StartDate", startDate.Date);
                cmd.Parameters.AddWithValue("@EndDate", endDate.Date);
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
