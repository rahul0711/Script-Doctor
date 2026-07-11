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
    public class DepartmentRepository : RepositoryBase<Department>, IDepartmentRepository
    {
        public DepartmentRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "Departments";
        protected override string PrimaryKeyName => "DepartmentID";

        protected override Department Map(DbDataReader reader)
        {
            var dept = new Department
            {
                DepartmentID = reader.GetInt32(reader.GetOrdinal("DepartmentID")),
                OrganizationID = reader.GetInt32(reader.GetOrdinal("OrganizationID")),
                DepartmentName = reader.GetString(reader.GetOrdinal("DepartmentName")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                IsActive = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("IsActive"))),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };

            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals("OrganizationName", StringComparison.OrdinalIgnoreCase))
                {
                    dept.OrganizationName = reader.IsDBNull(i) ? null : reader.GetString(i);
                    break;
                }
            }

            return dept;
        }

        private void AddParameters(MySqlCommand cmd, Department entity)
        {
            cmd.Parameters.AddWithValue("@OrganizationID", entity.OrganizationID);
            cmd.Parameters.AddWithValue("@DepartmentName", entity.DepartmentName);
            cmd.Parameters.AddWithValue("@Description", (object?)entity.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", entity.IsActive);
        }

        public override async Task<IEnumerable<Department>> GetAllAsync()
        {
            var list = new List<Department>();
            var query = $@"SELECT d.*, o.OrganizationName 
                           FROM {TableName} d 
                           INNER JOIN Organizations o ON d.OrganizationID = o.OrganizationID 
                           ORDER BY d.DepartmentID DESC";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    list.Add(Map(reader));
                }
            }
            return list;
        }

        public override async Task<Department?> GetByIdAsync(int id)
        {
            var query = $@"SELECT d.*, o.OrganizationName 
                           FROM {TableName} d 
                           INNER JOIN Organizations o ON d.OrganizationID = o.OrganizationID 
                           WHERE d.DepartmentID = @DepartmentID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@DepartmentID", id);
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

        public async Task<IEnumerable<Department>> GetByOrganizationIdAsync(int organizationId)
        {
            var list = new List<Department>();
            var query = $@"SELECT d.*, o.OrganizationName 
                           FROM {TableName} d 
                           INNER JOIN Organizations o ON d.OrganizationID = o.OrganizationID 
                           WHERE d.OrganizationID = @OrganizationID 
                           ORDER BY d.DepartmentID DESC";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@OrganizationID", organizationId);
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

        public override async Task<int> AddAsync(Department entity)
        {
            var query = $@"INSERT INTO {TableName} 
                (OrganizationID, DepartmentName, Description, IsActive) 
                VALUES 
                (@OrganizationID, @DepartmentName, @Description, @IsActive);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var idObj = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(idObj);
            }
        }

        public override async Task<bool> UpdateAsync(Department entity)
        {
            var query = $@"UPDATE {TableName} SET 
                OrganizationID = @OrganizationID, 
                DepartmentName = @DepartmentName, 
                Description = @Description, 
                IsActive = @IsActive 
                WHERE DepartmentID = @DepartmentID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                cmd.Parameters.AddWithValue("@DepartmentID", entity.DepartmentID);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }
    }
}
