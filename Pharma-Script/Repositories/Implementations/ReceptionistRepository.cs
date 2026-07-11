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
    public class ReceptionistRepository : RepositoryBase<Receptionist>, IReceptionistRepository
    {
        public ReceptionistRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "Receptionists";
        protected override string PrimaryKeyName => "ReceptionistID";

        protected override Receptionist Map(DbDataReader reader)
        {
            var receptionist = new Receptionist
            {
                ReceptionistID = reader.GetInt32(reader.GetOrdinal("ReceptionistID")),
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                OrganizationID = reader.GetInt32(reader.GetOrdinal("OrganizationID")),
                BranchID = reader.IsDBNull(reader.GetOrdinal("BranchID")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("BranchID")),
                IsActive = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("IsActive"))),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                if (name.Equals("FirstName", StringComparison.OrdinalIgnoreCase))
                    receptionist.FirstName = reader.GetString(i);
                else if (name.Equals("LastName", StringComparison.OrdinalIgnoreCase))
                    receptionist.LastName = reader.IsDBNull(i) ? null : reader.GetString(i);
                else if (name.Equals("Email", StringComparison.OrdinalIgnoreCase))
                    receptionist.Email = reader.GetString(i);
                else if (name.Equals("Phone", StringComparison.OrdinalIgnoreCase))
                    receptionist.Phone = reader.GetString(i);
                else if (name.Equals("BranchName", StringComparison.OrdinalIgnoreCase))
                    receptionist.BranchName = reader.IsDBNull(i) ? null : reader.GetString(i);
                else if (name.Equals("OrganizationName", StringComparison.OrdinalIgnoreCase))
                    receptionist.OrganizationName = reader.IsDBNull(i) ? null : reader.GetString(i);
            }

            return receptionist;
        }

        private void AddParameters(MySqlCommand cmd, Receptionist entity)
        {
            cmd.Parameters.AddWithValue("@UserID", entity.UserID);
            cmd.Parameters.AddWithValue("@OrganizationID", entity.OrganizationID);
            cmd.Parameters.AddWithValue("@BranchID", (object?)entity.BranchID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", entity.IsActive);
        }

        public override async Task<int> AddAsync(Receptionist entity)
        {
            var query = $@"INSERT INTO {TableName} 
                (UserID, OrganizationID, BranchID, IsActive) 
                VALUES 
                (@UserID, @OrganizationID, @BranchID, @IsActive);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var id = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
        }

        public override async Task<bool> UpdateAsync(Receptionist entity)
        {
            var query = $@"UPDATE {TableName} SET 
                BranchID = @BranchID, 
                IsActive = @IsActive
                WHERE ReceptionistID = @ReceptionistID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                cmd.Parameters.AddWithValue("@ReceptionistID", entity.ReceptionistID);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<bool> UpdateStatusAsync(int id, bool isActive)
        {
            var query = $"UPDATE {TableName} SET IsActive = @IsActive WHERE ReceptionistID = @ReceptionistID";
            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@IsActive", isActive);
                cmd.Parameters.AddWithValue("@ReceptionistID", id);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<IEnumerable<Receptionist>> SearchAndPaginateAsync(int? orgId, int? branchId, string searchTerm, int page, int pageSize)
        {
            var offset = (page - 1) * pageSize;
            var list = new List<Receptionist>();

            var sb = new StringBuilder();
            sb.Append($@"SELECT r.*, u.FirstName, u.LastName, u.Email, u.Phone, b.BranchName, o.OrganizationName
                         FROM {TableName} r
                         INNER JOIN Users u ON r.UserID = u.UserID
                         LEFT JOIN Branches b ON r.BranchID = b.BranchID
                         LEFT JOIN Organizations o ON r.OrganizationID = o.OrganizationID
                         WHERE 1=1");

            if (orgId.HasValue)
            {
                sb.Append(" AND r.OrganizationID = @OrganizationID");
            }
            if (branchId.HasValue)
            {
                sb.Append(" AND r.BranchID = @BranchID");
            }
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sb.Append(" AND (u.FirstName LIKE @SearchTerm OR u.LastName LIKE @SearchTerm OR u.Email LIKE @SearchTerm OR u.Phone LIKE @SearchTerm)");
            }

            sb.Append(" ORDER BY r.ReceptionistID DESC LIMIT @Limit OFFSET @Offset");

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(sb.ToString()))
            {
                if (orgId.HasValue) cmd.Parameters.AddWithValue("@OrganizationID", orgId.Value);
                if (branchId.HasValue) cmd.Parameters.AddWithValue("@BranchID", branchId.Value);
                if (!string.IsNullOrWhiteSpace(searchTerm)) cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
                cmd.Parameters.AddWithValue("@Limit", pageSize);
                cmd.Parameters.AddWithValue("@Offset", offset);

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

        public async Task<int> GetSearchCountAsync(int? orgId, int? branchId, string searchTerm)
        {
            var sb = new StringBuilder();
            sb.Append(@"SELECT COUNT(*)
                        FROM Receptionists r
                        INNER JOIN Users u ON r.UserID = u.UserID
                        WHERE 1=1");

            if (orgId.HasValue)
            {
                sb.Append(" AND r.OrganizationID = @OrganizationID");
            }
            if (branchId.HasValue)
            {
                sb.Append(" AND r.BranchID = @BranchID");
            }
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sb.Append(" AND (u.FirstName LIKE @SearchTerm OR u.LastName LIKE @SearchTerm OR u.Email LIKE @SearchTerm OR u.Phone LIKE @SearchTerm)");
            }

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(sb.ToString()))
            {
                if (orgId.HasValue) cmd.Parameters.AddWithValue("@OrganizationID", orgId.Value);
                if (branchId.HasValue) cmd.Parameters.AddWithValue("@BranchID", branchId.Value);
                if (!string.IsNullOrWhiteSpace(searchTerm)) cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

                var count = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(count);
            }
        }

        public async Task<Receptionist?> GetByUserIdAsync(int userId)
        {
            var query = $@"SELECT r.*, u.FirstName, u.LastName, u.Email, u.Phone, b.BranchName, o.OrganizationName
                           FROM {TableName} r
                           INNER JOIN Users u ON r.UserID = u.UserID
                           LEFT JOIN Branches b ON r.BranchID = b.BranchID
                           LEFT JOIN Organizations o ON r.OrganizationID = o.OrganizationID
                           WHERE r.UserID = @UserID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
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
