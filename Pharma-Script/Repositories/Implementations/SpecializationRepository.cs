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
    public class SpecializationRepository : RepositoryBase<Specialization>, ISpecializationRepository
    {
        public SpecializationRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "Specializations";
        protected override string PrimaryKeyName => "SpecializationID";

        protected override Specialization Map(DbDataReader reader)
        {
            return new Specialization
            {
                SpecializationID = reader.GetInt32(reader.GetOrdinal("SpecializationID")),
                SpecializationName = reader.GetString(reader.GetOrdinal("SpecializationName")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                IsActive = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("IsActive")))
            };
        }

        private void AddParameters(MySqlCommand cmd, Specialization entity)
        {
            cmd.Parameters.AddWithValue("@SpecializationName", entity.SpecializationName);
            cmd.Parameters.AddWithValue("@Description", (object?)entity.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", entity.IsActive);
        }

        public override async Task<int> AddAsync(Specialization entity)
        {
            var query = $@"INSERT INTO {TableName} 
                (SpecializationName, Description, IsActive) 
                VALUES 
                (@SpecializationName, @Description, @IsActive);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var idObj = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(idObj);
            }
        }

        public override async Task<bool> UpdateAsync(Specialization entity)
        {
            var query = $@"UPDATE {TableName} SET 
                SpecializationName = @SpecializationName, 
                Description = @Description, 
                IsActive = @IsActive 
                WHERE SpecializationID = @SpecializationID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                cmd.Parameters.AddWithValue("@SpecializationID", entity.SpecializationID);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<IEnumerable<Specialization>> SearchAndPaginateAsync(string searchTerm, int page, int pageSize)
        {
            var offset = (page - 1) * pageSize;
            var list = new List<Specialization>();
            var query = $@"SELECT * FROM {TableName} 
                WHERE (@SearchTerm = '' OR SpecializationName LIKE CONCAT('%', @SearchTerm, '%') 
                       OR Description LIKE CONCAT('%', @SearchTerm, '%'))
                ORDER BY SpecializationID DESC 
                LIMIT @Limit OFFSET @Offset";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", searchTerm ?? "");
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

        public async Task<int> GetSearchCountAsync(string searchTerm)
        {
            var query = $@"SELECT COUNT(*) FROM {TableName} 
                WHERE (@SearchTerm = '' OR SpecializationName LIKE CONCAT('%', @SearchTerm, '%') 
                       OR Description LIKE CONCAT('%', @SearchTerm, '%'))";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", searchTerm ?? "");
                var count = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(count);
            }
        }
    }
}
