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
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "Users";
        protected override string PrimaryKeyName => "UserID";

        protected override User Map(DbDataReader reader)
        {
            var user = new User
            {
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                OrganizationID = reader.IsDBNull(reader.GetOrdinal("OrganizationID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("OrganizationID")),
                RoleID = reader.GetInt32(reader.GetOrdinal("RoleID")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.IsDBNull(reader.GetOrdinal("LastName")) ? null : reader.GetString(reader.GetOrdinal("LastName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                ProfileImage = reader.IsDBNull(reader.GetOrdinal("ProfileImage")) ? null : reader.GetString(reader.GetOrdinal("ProfileImage")),
                IsActive = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("IsActive"))),
                LastLogin = reader.IsDBNull(reader.GetOrdinal("LastLogin")) ? null : reader.GetDateTime(reader.GetOrdinal("LastLogin")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var colName = reader.GetName(i);
                if (colName.Equals("RoleName", StringComparison.OrdinalIgnoreCase))
                {
                    user.RoleName = reader.IsDBNull(i) ? null : reader.GetString(i);
                }
                else if (colName.Equals("OrganizationName", StringComparison.OrdinalIgnoreCase))
                {
                    user.OrganizationName = reader.IsDBNull(i) ? null : reader.GetString(i);
                }
            }

            return user;
        }

        private void AddParameters(MySqlCommand cmd, User entity)
        {
            cmd.Parameters.AddWithValue("@OrganizationID", (object?)entity.OrganizationID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RoleID", entity.RoleID);
            cmd.Parameters.AddWithValue("@FirstName", entity.FirstName);
            cmd.Parameters.AddWithValue("@LastName", (object?)entity.LastName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", entity.Email);
            cmd.Parameters.AddWithValue("@Phone", entity.Phone);
            cmd.Parameters.AddWithValue("@PasswordHash", entity.PasswordHash);
            cmd.Parameters.AddWithValue("@ProfileImage", (object?)entity.ProfileImage ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", entity.IsActive);
            cmd.Parameters.AddWithValue("@LastLogin", (object?)entity.LastLogin ?? DBNull.Value);
        }

        public override async Task<IEnumerable<User>> GetAllAsync()
        {
            var list = new List<User>();
            var query = $@"SELECT u.*, r.RoleName, o.OrganizationName 
                           FROM {TableName} u 
                           INNER JOIN Roles r ON u.RoleID = r.RoleID 
                           LEFT JOIN Organizations o ON u.OrganizationID = o.OrganizationID 
                           ORDER BY u.UserID DESC";

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

        public override async Task<User?> GetByIdAsync(int id)
        {
            var query = $@"SELECT u.*, r.RoleName, o.OrganizationName 
                           FROM {TableName} u 
                           INNER JOIN Roles r ON u.RoleID = r.RoleID 
                           LEFT JOIN Organizations o ON u.OrganizationID = o.OrganizationID 
                           WHERE u.UserID = @UserID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@UserID", id);
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

        public async Task<User?> GetByEmailAsync(string email)
        {
            var query = $@"SELECT u.*, r.RoleName, o.OrganizationName 
                           FROM {TableName} u 
                           INNER JOIN Roles r ON u.RoleID = r.RoleID 
                           LEFT JOIN Organizations o ON u.OrganizationID = o.OrganizationID 
                           WHERE u.Email = @Email";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@Email", email);
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

        public async Task<IEnumerable<User>> GetByOrganizationIdAsync(int organizationId)
        {
            var list = new List<User>();
            var query = $@"SELECT u.*, r.RoleName, o.OrganizationName 
                           FROM {TableName} u 
                           INNER JOIN Roles r ON u.RoleID = r.RoleID 
                           LEFT JOIN Organizations o ON u.OrganizationID = o.OrganizationID 
                           WHERE u.OrganizationID = @OrganizationID 
                           ORDER BY u.UserID DESC";

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

        public override async Task<int> AddAsync(User entity)
        {
            var query = $@"INSERT INTO {TableName} 
                (OrganizationID, RoleID, FirstName, LastName, Email, Phone, PasswordHash, ProfileImage, IsActive, LastLogin) 
                VALUES 
                (@OrganizationID, @RoleID, @FirstName, @LastName, @Email, @Phone, @PasswordHash, @ProfileImage, @IsActive, @LastLogin);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var idObj = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(idObj);
            }
        }

        public override async Task<bool> UpdateAsync(User entity)
        {
            var query = $@"UPDATE {TableName} SET 
                OrganizationID = @OrganizationID, 
                RoleID = @RoleID, 
                FirstName = @FirstName, 
                LastName = @LastName, 
                Email = @Email, 
                Phone = @Phone, 
                ProfileImage = @ProfileImage, 
                IsActive = @IsActive,
                UpdatedAt = CURRENT_TIMESTAMP
                WHERE UserID = @UserID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                cmd.Parameters.AddWithValue("@UserID", entity.UserID);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<bool> UpdateStatusAsync(int id, bool isActive)
        {
            var query = $"UPDATE {TableName} SET IsActive = @IsActive, UpdatedAt = CURRENT_TIMESTAMP WHERE UserID = @UserID";
            
            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@IsActive", isActive);
                cmd.Parameters.AddWithValue("@UserID", id);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<bool> UpdatePasswordAsync(int id, string passwordHash)
        {
            var query = $"UPDATE {TableName} SET PasswordHash = @PasswordHash, UpdatedAt = CURRENT_TIMESTAMP WHERE UserID = @UserID";
            
            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                cmd.Parameters.AddWithValue("@UserID", id);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }
    }
}
