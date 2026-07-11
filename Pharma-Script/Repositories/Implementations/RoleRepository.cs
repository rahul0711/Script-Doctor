using MySql.Data.MySqlClient;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Implementations
{
    public class RoleRepository : RepositoryBase<Role>, IRoleRepository
    {
        public RoleRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "Roles";
        protected override string PrimaryKeyName => "RoleID";

        protected override Role Map(DbDataReader reader)
        {
            return new Role
            {
                RoleID = reader.GetInt32(reader.GetOrdinal("RoleID")),
                RoleName = reader.GetString(reader.GetOrdinal("RoleName")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description"))
            };
        }

        private void AddParameters(MySqlCommand cmd, Role entity)
        {
            cmd.Parameters.AddWithValue("@RoleName", entity.RoleName);
            cmd.Parameters.AddWithValue("@Description", (object?)entity.Description ?? DBNull.Value);
        }

        public async Task<Role?> GetByNameAsync(string roleName)
        {
            var query = $"SELECT * FROM {TableName} WHERE RoleName = @RoleName";
            
            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@RoleName", roleName);
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

        public override async Task<int> AddAsync(Role entity)
        {
            var query = $@"INSERT INTO {TableName} 
                (RoleName, Description) 
                VALUES 
                (@RoleName, @Description);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var idObj = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(idObj);
            }
        }

        public override async Task<bool> UpdateAsync(Role entity)
        {
            var query = $@"UPDATE {TableName} SET 
                RoleName = @RoleName, 
                Description = @Description 
                WHERE RoleID = @RoleID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                cmd.Parameters.AddWithValue("@RoleID", entity.RoleID);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }
    }
}
