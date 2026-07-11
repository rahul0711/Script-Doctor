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
    public class AppointmentStatusHistoryRepository : RepositoryBase<AppointmentStatusHistory>, IAppointmentStatusHistoryRepository
    {
        public AppointmentStatusHistoryRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "AppointmentStatusHistory";
        protected override string PrimaryKeyName => "HistoryID";

        protected override AppointmentStatusHistory Map(DbDataReader reader)
        {
            var history = new AppointmentStatusHistory
            {
                HistoryID = reader.GetInt32(reader.GetOrdinal("HistoryID")),
                AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID")),
                OldStatus = reader.IsDBNull(reader.GetOrdinal("OldStatus")) ? null : reader.GetString(reader.GetOrdinal("OldStatus")),
                NewStatus = reader.GetString(reader.GetOrdinal("NewStatus")),
                ChangedByUserID = reader.GetInt32(reader.GetOrdinal("ChangedByUserID")),
                Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks")),
                ChangedAt = reader.GetDateTime(reader.GetOrdinal("ChangedAt"))
            };

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var col = reader.GetName(i);
                if (col.Equals("ChangedByUserName", StringComparison.OrdinalIgnoreCase))
                {
                    history.ChangedByUserName = reader.IsDBNull(i) ? null : reader.GetString(i);
                }
            }

            return history;
        }

        private void AddParameters(MySqlCommand cmd, AppointmentStatusHistory entity)
        {
            cmd.Parameters.AddWithValue("@AppointmentID", entity.AppointmentID);
            cmd.Parameters.AddWithValue("@OldStatus", (object?)entity.OldStatus ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NewStatus", entity.NewStatus);
            cmd.Parameters.AddWithValue("@ChangedByUserID", entity.ChangedByUserID);
            cmd.Parameters.AddWithValue("@Remarks", (object?)entity.Remarks ?? DBNull.Value);
        }

        public override async Task<int> AddAsync(AppointmentStatusHistory entity)
        {
            var query = $@"INSERT INTO {TableName} 
                (AppointmentID, OldStatus, NewStatus, ChangedByUserID, Remarks) 
                VALUES 
                (@AppointmentID, @OldStatus, @NewStatus, @ChangedByUserID, @Remarks);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var id = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
        }

        public override Task<bool> UpdateAsync(AppointmentStatusHistory entity)
        {
            throw new NotImplementedException("Appointment status history cannot be updated once logged.");
        }

        public async Task<IEnumerable<AppointmentStatusHistory>> GetByAppointmentIdAsync(int appointmentId)
        {
            var list = new List<AppointmentStatusHistory>();
            var query = $@"SELECT h.*, CONCAT(u.FirstName, ' ', IFNULL(u.LastName, '')) AS ChangedByUserName
                           FROM {TableName} h
                           INNER JOIN Users u ON h.ChangedByUserID = u.UserID
                           WHERE h.AppointmentID = @AppointmentID
                           ORDER BY h.ChangedAt DESC";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@AppointmentID", appointmentId);
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
