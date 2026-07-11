using MySql.Data.MySqlClient;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Implementations
{
    public class ContactMessageRepository : RepositoryBase<ContactMessage>, IContactMessageRepository
    {
        public ContactMessageRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "ContactMessages";
        protected override string PrimaryKeyName => "ContactMessageID";

        protected override ContactMessage Map(DbDataReader reader)
        {
            return new ContactMessage
            {
                ContactMessageID = reader.GetInt32(reader.GetOrdinal("ContactMessageID")),
                OrganizationID = reader.GetInt32(reader.GetOrdinal("OrganizationID")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                Subject = reader.IsDBNull(reader.GetOrdinal("Subject")) ? null : reader.GetString(reader.GetOrdinal("Subject")),
                Message = reader.GetString(reader.GetOrdinal("Message")),
                IsRead = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("IsRead"))),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }

        private void AddParameters(MySqlCommand cmd, ContactMessage entity)
        {
            cmd.Parameters.AddWithValue("@OrganizationID", entity.OrganizationID);
            cmd.Parameters.AddWithValue("@Name", entity.Name);
            cmd.Parameters.AddWithValue("@Email", (object?)entity.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Phone", (object?)entity.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Subject", (object?)entity.Subject ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Message", entity.Message);
            cmd.Parameters.AddWithValue("@IsRead", entity.IsRead);
        }

        public override async Task<int> AddAsync(ContactMessage entity)
        {
            var query = $@"INSERT INTO {TableName}
                (OrganizationID, Name, Email, Phone, Subject, Message, IsRead)
                VALUES (@OrganizationID, @Name, @Email, @Phone, @Subject, @Message, @IsRead);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var id = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
        }

        public override async Task<bool> UpdateAsync(ContactMessage entity)
        {
            var query = $@"UPDATE {TableName} SET
                Name = @Name, Email = @Email, Phone = @Phone, Subject = @Subject, Message = @Message, IsRead = @IsRead
                WHERE ContactMessageID = @ContactMessageID AND OrganizationID = @OrganizationID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                cmd.Parameters.AddWithValue("@ContactMessageID", entity.ContactMessageID);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<IEnumerable<ContactMessage>> SearchAndPaginateAsync(int organizationId, bool? isRead, int page, int pageSize)
        {
            var list = new List<ContactMessage>();
            var offset = (page - 1) * pageSize;
            var query = $@"SELECT * FROM {TableName}
                WHERE OrganizationID = @OrganizationID
                  AND (@IsRead IS NULL OR IsRead = @IsRead)
                ORDER BY CreatedAt DESC
                LIMIT @Limit OFFSET @Offset";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@OrganizationID", organizationId);
                cmd.Parameters.AddWithValue("@IsRead", (object?)isRead ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Limit", pageSize);
                cmd.Parameters.AddWithValue("@Offset", offset);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync()) list.Add(Map(reader));
                }
            }
            return list;
        }

        public async Task<int> GetSearchCountAsync(int organizationId, bool? isRead)
        {
            var query = $@"SELECT COUNT(*) FROM {TableName}
                WHERE OrganizationID = @OrganizationID AND (@IsRead IS NULL OR IsRead = @IsRead)";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@OrganizationID", organizationId);
                cmd.Parameters.AddWithValue("@IsRead", (object?)isRead ?? DBNull.Value);
                var count = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(count);
            }
        }

        public async Task<ContactMessage?> GetByIdForOrganizationAsync(int id, int organizationId)
        {
            var query = $"SELECT * FROM {TableName} WHERE ContactMessageID = @Id AND OrganizationID = @OrganizationID";
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

        public async Task<bool> SetReadAsync(int id, int organizationId, bool isRead)
        {
            var query = $"UPDATE {TableName} SET IsRead = @IsRead WHERE ContactMessageID = @Id AND OrganizationID = @OrganizationID";
            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@IsRead", isRead);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@OrganizationID", organizationId);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<int> GetUnreadCountAsync(int organizationId)
        {
            var query = $"SELECT COUNT(*) FROM {TableName} WHERE OrganizationID = @OrganizationID AND IsRead = 0";
            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@OrganizationID", organizationId);
                var count = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(count);
            }
        }
    }
}
