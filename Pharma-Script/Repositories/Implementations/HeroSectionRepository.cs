using MySql.Data.MySqlClient;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Implementations
{
    public class HeroSectionRepository : RepositoryBase<HeroSection>, IHeroSectionRepository
    {
        public HeroSectionRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "HeroSections";
        protected override string PrimaryKeyName => "HeroSectionID";

        protected override HeroSection Map(DbDataReader reader)
        {
            return new HeroSection
            {
                HeroSectionID = reader.GetInt32(reader.GetOrdinal("HeroSectionID")),
                OrganizationID = reader.GetInt32(reader.GetOrdinal("OrganizationID")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Subtitle = reader.IsDBNull(reader.GetOrdinal("Subtitle")) ? null : reader.GetString(reader.GetOrdinal("Subtitle")),
                BannerImage = reader.IsDBNull(reader.GetOrdinal("BannerImage")) ? null : reader.GetString(reader.GetOrdinal("BannerImage")),
                ButtonText = reader.IsDBNull(reader.GetOrdinal("ButtonText")) ? null : reader.GetString(reader.GetOrdinal("ButtonText")),
                ButtonURL = reader.IsDBNull(reader.GetOrdinal("ButtonURL")) ? null : reader.GetString(reader.GetOrdinal("ButtonURL")),
                IsActive = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("IsActive"))),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }

        private void AddParameters(MySqlCommand cmd, HeroSection entity)
        {
            cmd.Parameters.AddWithValue("@OrganizationID", entity.OrganizationID);
            cmd.Parameters.AddWithValue("@Title", entity.Title);
            cmd.Parameters.AddWithValue("@Subtitle", (object?)entity.Subtitle ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BannerImage", (object?)entity.BannerImage ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ButtonText", (object?)entity.ButtonText ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ButtonURL", (object?)entity.ButtonURL ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", entity.IsActive);
        }

        public override async Task<int> AddAsync(HeroSection entity)
        {
            var query = $@"INSERT INTO {TableName}
                (OrganizationID, Title, Subtitle, BannerImage, ButtonText, ButtonURL, IsActive)
                VALUES (@OrganizationID, @Title, @Subtitle, @BannerImage, @ButtonText, @ButtonURL, @IsActive);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var id = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
        }

        public override async Task<bool> UpdateAsync(HeroSection entity)
        {
            var query = $@"UPDATE {TableName} SET
                Title = @Title, Subtitle = @Subtitle, BannerImage = @BannerImage,
                ButtonText = @ButtonText, ButtonURL = @ButtonURL, IsActive = @IsActive
                WHERE HeroSectionID = @HeroSectionID AND OrganizationID = @OrganizationID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                cmd.Parameters.AddWithValue("@HeroSectionID", entity.HeroSectionID);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<IEnumerable<HeroSection>> GetByOrganizationIdAsync(int organizationId)
        {
            var list = new List<HeroSection>();
            var query = $"SELECT * FROM {TableName} WHERE OrganizationID = @OrganizationID ORDER BY HeroSectionID DESC";
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

        public async Task<IEnumerable<HeroSection>> GetActiveByOrganizationIdAsync(int organizationId)
        {
            var list = new List<HeroSection>();
            var query = $"SELECT * FROM {TableName} WHERE OrganizationID = @OrganizationID AND IsActive = 1 ORDER BY HeroSectionID DESC";
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

        public async Task<HeroSection?> GetByIdForOrganizationAsync(int id, int organizationId)
        {
            var query = $"SELECT * FROM {TableName} WHERE HeroSectionID = @Id AND OrganizationID = @OrganizationID";
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
            var query = $"UPDATE {TableName} SET IsActive = @IsActive WHERE HeroSectionID = @Id AND OrganizationID = @OrganizationID";
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
            var query = $"DELETE FROM {TableName} WHERE HeroSectionID = @Id AND OrganizationID = @OrganizationID";
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
