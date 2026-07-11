using MySql.Data.MySqlClient;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Implementations
{
    public class GalleryRepository : RepositoryBase<GalleryImage>, IGalleryRepository
    {
        public GalleryRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "Gallery";
        protected override string PrimaryKeyName => "GalleryID";

        protected override GalleryImage Map(DbDataReader reader)
        {
            return new GalleryImage
            {
                GalleryID = reader.GetInt32(reader.GetOrdinal("GalleryID")),
                OrganizationID = reader.GetInt32(reader.GetOrdinal("OrganizationID")),
                ImageTitle = reader.IsDBNull(reader.GetOrdinal("ImageTitle")) ? null : reader.GetString(reader.GetOrdinal("ImageTitle")),
                ImagePath = reader.GetString(reader.GetOrdinal("ImagePath")),
                DisplayOrder = reader.GetInt32(reader.GetOrdinal("DisplayOrder")),
                IsActive = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("IsActive"))),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }

        private void AddParameters(MySqlCommand cmd, GalleryImage entity)
        {
            cmd.Parameters.AddWithValue("@OrganizationID", entity.OrganizationID);
            cmd.Parameters.AddWithValue("@ImageTitle", (object?)entity.ImageTitle ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ImagePath", entity.ImagePath);
            cmd.Parameters.AddWithValue("@DisplayOrder", entity.DisplayOrder);
            cmd.Parameters.AddWithValue("@IsActive", entity.IsActive);
        }

        public override async Task<int> AddAsync(GalleryImage entity)
        {
            var query = $@"INSERT INTO {TableName}
                (OrganizationID, ImageTitle, ImagePath, DisplayOrder, IsActive)
                VALUES (@OrganizationID, @ImageTitle, @ImagePath, @DisplayOrder, @IsActive);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var id = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
        }

        public override async Task<bool> UpdateAsync(GalleryImage entity)
        {
            var query = $@"UPDATE {TableName} SET
                ImageTitle = @ImageTitle, ImagePath = @ImagePath, DisplayOrder = @DisplayOrder, IsActive = @IsActive
                WHERE GalleryID = @GalleryID AND OrganizationID = @OrganizationID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                cmd.Parameters.AddWithValue("@GalleryID", entity.GalleryID);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<IEnumerable<GalleryImage>> GetByOrganizationIdAsync(int organizationId)
        {
            var list = new List<GalleryImage>();
            var query = $"SELECT * FROM {TableName} WHERE OrganizationID = @OrganizationID ORDER BY DisplayOrder ASC, GalleryID DESC";
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

        public async Task<IEnumerable<GalleryImage>> GetActiveByOrganizationIdAsync(int organizationId)
        {
            var list = new List<GalleryImage>();
            var query = $"SELECT * FROM {TableName} WHERE OrganizationID = @OrganizationID AND IsActive = 1 ORDER BY DisplayOrder ASC, GalleryID DESC";
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

        public async Task<GalleryImage?> GetByIdForOrganizationAsync(int id, int organizationId)
        {
            var query = $"SELECT * FROM {TableName} WHERE GalleryID = @Id AND OrganizationID = @OrganizationID";
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
            var query = $"UPDATE {TableName} SET IsActive = @IsActive WHERE GalleryID = @Id AND OrganizationID = @OrganizationID";
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

        public async Task<bool> UpdateDisplayOrderAsync(int id, int organizationId, int displayOrder)
        {
            var query = $"UPDATE {TableName} SET DisplayOrder = @DisplayOrder WHERE GalleryID = @Id AND OrganizationID = @OrganizationID";
            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@DisplayOrder", displayOrder);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@OrganizationID", organizationId);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<bool> DeleteForOrganizationAsync(int id, int organizationId)
        {
            var query = $"DELETE FROM {TableName} WHERE GalleryID = @Id AND OrganizationID = @OrganizationID";
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
