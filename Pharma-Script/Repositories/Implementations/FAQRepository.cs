using MySql.Data.MySqlClient;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Implementations
{
    public class FAQRepository : RepositoryBase<FAQ>, IFAQRepository
    {
        public FAQRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "FAQs";
        protected override string PrimaryKeyName => "FAQID";

        protected override FAQ Map(DbDataReader reader)
        {
            return new FAQ
            {
                FAQID = reader.GetInt32(reader.GetOrdinal("FAQID")),
                OrganizationID = reader.GetInt32(reader.GetOrdinal("OrganizationID")),
                Question = reader.GetString(reader.GetOrdinal("Question")),
                Answer = reader.GetString(reader.GetOrdinal("Answer")),
                DisplayOrder = reader.GetInt32(reader.GetOrdinal("DisplayOrder")),
                IsActive = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("IsActive"))),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }

        private void AddParameters(MySqlCommand cmd, FAQ entity)
        {
            cmd.Parameters.AddWithValue("@OrganizationID", entity.OrganizationID);
            cmd.Parameters.AddWithValue("@Question", entity.Question);
            cmd.Parameters.AddWithValue("@Answer", entity.Answer);
            cmd.Parameters.AddWithValue("@DisplayOrder", entity.DisplayOrder);
            cmd.Parameters.AddWithValue("@IsActive", entity.IsActive);
        }

        public override async Task<int> AddAsync(FAQ entity)
        {
            var query = $@"INSERT INTO {TableName}
                (OrganizationID, Question, Answer, DisplayOrder, IsActive)
                VALUES (@OrganizationID, @Question, @Answer, @DisplayOrder, @IsActive);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var id = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
        }

        public override async Task<bool> UpdateAsync(FAQ entity)
        {
            var query = $@"UPDATE {TableName} SET
                Question = @Question, Answer = @Answer, DisplayOrder = @DisplayOrder, IsActive = @IsActive
                WHERE FAQID = @FAQID AND OrganizationID = @OrganizationID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                cmd.Parameters.AddWithValue("@FAQID", entity.FAQID);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<IEnumerable<FAQ>> GetByOrganizationIdAsync(int organizationId)
        {
            var list = new List<FAQ>();
            var query = $"SELECT * FROM {TableName} WHERE OrganizationID = @OrganizationID ORDER BY DisplayOrder ASC, FAQID DESC";
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

        public async Task<IEnumerable<FAQ>> GetActiveByOrganizationIdAsync(int organizationId)
        {
            var list = new List<FAQ>();
            var query = $"SELECT * FROM {TableName} WHERE OrganizationID = @OrganizationID AND IsActive = 1 ORDER BY DisplayOrder ASC, FAQID DESC";
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

        public async Task<FAQ?> GetByIdForOrganizationAsync(int id, int organizationId)
        {
            var query = $"SELECT * FROM {TableName} WHERE FAQID = @Id AND OrganizationID = @OrganizationID";
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
            var query = $"UPDATE {TableName} SET IsActive = @IsActive WHERE FAQID = @Id AND OrganizationID = @OrganizationID";
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
            var query = $"UPDATE {TableName} SET DisplayOrder = @DisplayOrder WHERE FAQID = @Id AND OrganizationID = @OrganizationID";
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
            var query = $"DELETE FROM {TableName} WHERE FAQID = @Id AND OrganizationID = @OrganizationID";
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
