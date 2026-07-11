using MySql.Data.MySqlClient;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Implementations
{
    public class ServiceRepository : RepositoryBase<Service>, IServiceRepository
    {
        public ServiceRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "Services";
        protected override string PrimaryKeyName => "ServiceID";

        protected override Service Map(DbDataReader reader)
        {
            return new Service
            {
                ServiceID = reader.GetInt32(reader.GetOrdinal("ServiceID")),
                OrganizationID = reader.GetInt32(reader.GetOrdinal("OrganizationID")),
                ServiceName = reader.GetString(reader.GetOrdinal("ServiceName")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                ServiceImage = reader.IsDBNull(reader.GetOrdinal("ServiceImage")) ? null : reader.GetString(reader.GetOrdinal("ServiceImage")),
                IsActive = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("IsActive"))),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }

        private void AddParameters(MySqlCommand cmd, Service entity)
        {
            cmd.Parameters.AddWithValue("@OrganizationID", entity.OrganizationID);
            cmd.Parameters.AddWithValue("@ServiceName", entity.ServiceName);
            cmd.Parameters.AddWithValue("@Description", (object?)entity.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ServiceImage", (object?)entity.ServiceImage ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", entity.IsActive);
        }

        public override async Task<int> AddAsync(Service entity)
        {
            var query = $@"INSERT INTO {TableName}
                (OrganizationID, ServiceName, Description, ServiceImage, IsActive)
                VALUES (@OrganizationID, @ServiceName, @Description, @ServiceImage, @IsActive);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var id = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
        }

        public override async Task<bool> UpdateAsync(Service entity)
        {
            var query = $@"UPDATE {TableName} SET
                ServiceName = @ServiceName, Description = @Description, ServiceImage = @ServiceImage, IsActive = @IsActive
                WHERE ServiceID = @ServiceID AND OrganizationID = @OrganizationID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                cmd.Parameters.AddWithValue("@ServiceID", entity.ServiceID);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<IEnumerable<Service>> GetByOrganizationIdAsync(int organizationId)
        {
            var list = new List<Service>();
            var query = $"SELECT * FROM {TableName} WHERE OrganizationID = @OrganizationID ORDER BY ServiceID DESC";
            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@OrganizationID", organizationId);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync()) list.Add(Map(reader));
                }
            }
            return list;
        }

        public async Task<IEnumerable<Service>> GetActiveByOrganizationIdAsync(int organizationId)
        {
            var list = new List<Service>();
            var query = $"SELECT * FROM {TableName} WHERE OrganizationID = @OrganizationID AND IsActive = 1 ORDER BY ServiceID DESC";
            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@OrganizationID", organizationId);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync()) list.Add(Map(reader));
                }
            }
            return list;
        }

        public async Task<Service?> GetByIdForOrganizationAsync(int id, int organizationId)
        {
            var query = $"SELECT * FROM {TableName} WHERE ServiceID = @Id AND OrganizationID = @OrganizationID";
            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@OrganizationID", organizationId);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync()) return Map(reader);
                }
            }
            return null;
        }

        public async Task<bool> SetActiveAsync(int id, int organizationId, bool isActive)
        {
            var query = $"UPDATE {TableName} SET IsActive = @IsActive WHERE ServiceID = @Id AND OrganizationID = @OrganizationID";
            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@IsActive", isActive);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@OrganizationID", organizationId);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<bool> DeleteForOrganizationAsync(int id, int organizationId)
        {
            var query = $"DELETE FROM {TableName} WHERE ServiceID = @Id AND OrganizationID = @OrganizationID";
            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@OrganizationID", organizationId);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }
    }
}
