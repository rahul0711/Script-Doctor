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
    public class BranchRepository : RepositoryBase<Branch>, IBranchRepository
    {
        public BranchRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "Branches";
        protected override string PrimaryKeyName => "BranchID";

        protected override Branch Map(DbDataReader reader)
        {
            var branch = new Branch
            {
                BranchID = reader.GetInt32(reader.GetOrdinal("BranchID")),
                OrganizationID = reader.GetInt32(reader.GetOrdinal("OrganizationID")),
                BranchName = reader.GetString(reader.GetOrdinal("BranchName")),
                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                AddressLine1 = reader.GetString(reader.GetOrdinal("AddressLine1")),
                AddressLine2 = reader.IsDBNull(reader.GetOrdinal("AddressLine2")) ? null : reader.GetString(reader.GetOrdinal("AddressLine2")),
                City = reader.GetString(reader.GetOrdinal("City")),
                State = reader.GetString(reader.GetOrdinal("State")),
                Country = reader.GetString(reader.GetOrdinal("Country")),
                Pincode = reader.GetString(reader.GetOrdinal("Pincode")),
                IsMainBranch = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("IsMainBranch"))),
                IsActive = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("IsActive"))),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };

            // Check if OrganizationName is present in the select results
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals("OrganizationName", StringComparison.OrdinalIgnoreCase))
                {
                    branch.OrganizationName = reader.IsDBNull(i) ? null : reader.GetString(i);
                    break;
                }
            }

            return branch;
        }

        private void AddParameters(MySqlCommand cmd, Branch entity)
        {
            cmd.Parameters.AddWithValue("@OrganizationID", entity.OrganizationID);
            cmd.Parameters.AddWithValue("@BranchName", entity.BranchName);
            cmd.Parameters.AddWithValue("@Email", (object?)entity.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Phone", (object?)entity.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AddressLine1", entity.AddressLine1);
            cmd.Parameters.AddWithValue("@AddressLine2", (object?)entity.AddressLine2 ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@City", entity.City);
            cmd.Parameters.AddWithValue("@State", entity.State);
            cmd.Parameters.AddWithValue("@Country", entity.Country);
            cmd.Parameters.AddWithValue("@Pincode", entity.Pincode);
            cmd.Parameters.AddWithValue("@IsMainBranch", entity.IsMainBranch);
            cmd.Parameters.AddWithValue("@IsActive", entity.IsActive);
        }

        public override async Task<IEnumerable<Branch>> GetAllAsync()
        {
            var list = new List<Branch>();
            var query = $@"SELECT b.*, o.OrganizationName 
                           FROM {TableName} b 
                           INNER JOIN Organizations o ON b.OrganizationID = o.OrganizationID 
                           ORDER BY b.BranchID DESC";

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

        public override async Task<Branch?> GetByIdAsync(int id)
        {
            var query = $@"SELECT b.*, o.OrganizationName 
                           FROM {TableName} b 
                           INNER JOIN Organizations o ON b.OrganizationID = o.OrganizationID 
                           WHERE b.BranchID = @BranchID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@BranchID", id);
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

        public async Task<IEnumerable<Branch>> GetByOrganizationIdAsync(int organizationId)
        {
            var list = new List<Branch>();
            var query = $@"SELECT b.*, o.OrganizationName 
                           FROM {TableName} b 
                           INNER JOIN Organizations o ON b.OrganizationID = o.OrganizationID 
                           WHERE b.OrganizationID = @OrganizationID 
                           ORDER BY b.BranchID DESC";

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

        public override async Task<int> AddAsync(Branch entity)
        {
            var query = $@"INSERT INTO {TableName} 
                (OrganizationID, BranchName, Email, Phone, AddressLine1, AddressLine2, City, State, Country, Pincode, IsMainBranch, IsActive) 
                VALUES 
                (@OrganizationID, @BranchName, @Email, @Phone, @AddressLine1, @AddressLine2, @City, @State, @Country, @Pincode, @IsMainBranch, @IsActive);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var idObj = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(idObj);
            }
        }

        public override async Task<bool> UpdateAsync(Branch entity)
        {
            var query = $@"UPDATE {TableName} SET 
                OrganizationID = @OrganizationID, 
                BranchName = @BranchName, 
                Email = @Email, 
                Phone = @Phone, 
                AddressLine1 = @AddressLine1, 
                AddressLine2 = @AddressLine2, 
                City = @City, 
                State = @State, 
                Country = @Country, 
                Pincode = @Pincode, 
                IsMainBranch = @IsMainBranch, 
                IsActive = @IsActive 
                WHERE BranchID = @BranchID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                cmd.Parameters.AddWithValue("@BranchID", entity.BranchID);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }
    }
}
