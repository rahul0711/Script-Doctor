using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;

namespace Pharma_Script.Repositories.Implementations
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly MySqlConnection _connection;
        private readonly Func<MySqlTransaction?> _transactionProvider;

        public NotificationRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
        {
            _connection = connection;
            _transactionProvider = transactionProvider;
        }

        // Ensures the shared connection is open before executing any query.
        private async Task EnsureOpenAsync()
        {
            if (_connection.State != ConnectionState.Open)
                await _connection.OpenAsync();
        }

        public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(int userId, int organizationId)
        {
            var list = new List<Notification>();
            try
            {
                await EnsureOpenAsync();
                const string query = @"SELECT * FROM Notifications 
                                       WHERE UserID = @UserId AND OrganizationID = @OrgId AND IsRead = FALSE 
                                       ORDER BY CreatedAt DESC";

                using var cmd = new MySqlCommand(query, _connection, _transactionProvider());
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@OrgId", organizationId);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    list.Add(Map(reader));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationRepository] GetUnreadByUserIdAsync failed: {ex.Message}");
            }
            return list;
        }

        public async Task<IEnumerable<Notification>> GetAllByUserIdAsync(int userId, int organizationId, int limit = 50)
        {
            var list = new List<Notification>();
            try
            {
                await EnsureOpenAsync();
                const string query = @"SELECT * FROM Notifications 
                                       WHERE UserID = @UserId AND OrganizationID = @OrgId 
                                       ORDER BY CreatedAt DESC 
                                       LIMIT @Limit";

                using var cmd = new MySqlCommand(query, _connection, _transactionProvider());
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@OrgId", organizationId);
                cmd.Parameters.AddWithValue("@Limit", limit);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    list.Add(Map(reader));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationRepository] GetAllByUserIdAsync failed: {ex.Message}");
            }
            return list;
        }

        public async Task<int> CreateAsync(Notification notification)
        {
            try
            {
                await EnsureOpenAsync();
                const string query = @"INSERT INTO Notifications 
                                       (OrganizationID, UserID, NotificationType, Title, Message, 
                                        RelatedEntityType, RelatedEntityID, IsRead, CreatedAt)
                                       VALUES 
                                       (@OrgId, @UserId, @Type, @Title, @Message, @RelType, @RelId, @IsRead, @CreatedAt);
                                       SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(query, _connection, _transactionProvider());
                cmd.Parameters.AddWithValue("@OrgId", notification.OrganizationID);
                cmd.Parameters.AddWithValue("@UserId", notification.UserID);
                cmd.Parameters.AddWithValue("@Type", notification.NotificationType);
                cmd.Parameters.AddWithValue("@Title", notification.Title);
                cmd.Parameters.AddWithValue("@Message", notification.Message);
                cmd.Parameters.AddWithValue("@RelType", (object?)notification.RelatedEntityType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RelId", (object?)notification.RelatedEntityID ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsRead", notification.IsRead);
                cmd.Parameters.AddWithValue("@CreatedAt", notification.CreatedAt);

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationRepository] CreateAsync failed: {ex.Message}");
                return 0;
            }
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId, int organizationId)
        {
            try
            {
                await EnsureOpenAsync();
                const string query = @"UPDATE Notifications 
                                       SET IsRead = TRUE 
                                       WHERE NotificationID = @Id AND UserID = @UserId AND OrganizationID = @OrgId";

                using var cmd = new MySqlCommand(query, _connection, _transactionProvider());
                cmd.Parameters.AddWithValue("@Id", notificationId);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@OrgId", organizationId);

                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationRepository] MarkAsReadAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> MarkAllAsReadAsync(int userId, int organizationId)
        {
            try
            {
                await EnsureOpenAsync();
                const string query = @"UPDATE Notifications 
                                       SET IsRead = TRUE 
                                       WHERE UserID = @UserId AND OrganizationID = @OrgId AND IsRead = FALSE";

                using var cmd = new MySqlCommand(query, _connection, _transactionProvider());
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@OrgId", organizationId);

                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationRepository] MarkAllAsReadAsync failed: {ex.Message}");
                return false;
            }
        }

        private static Notification Map(DbDataReader reader)
        {
            return new Notification
            {
                NotificationID    = reader.GetInt32(reader.GetOrdinal("NotificationID")),
                OrganizationID    = reader.GetInt32(reader.GetOrdinal("OrganizationID")),
                UserID            = reader.GetInt32(reader.GetOrdinal("UserID")),
                NotificationType  = reader.GetString(reader.GetOrdinal("NotificationType")),
                Title             = reader.GetString(reader.GetOrdinal("Title")),
                Message           = reader.GetString(reader.GetOrdinal("Message")),
                RelatedEntityType = reader.IsDBNull(reader.GetOrdinal("RelatedEntityType")) ? null : reader.GetString(reader.GetOrdinal("RelatedEntityType")),
                RelatedEntityID   = reader.IsDBNull(reader.GetOrdinal("RelatedEntityID"))   ? (int?)null : reader.GetInt32(reader.GetOrdinal("RelatedEntityID")),
                IsRead            = reader.GetBoolean(reader.GetOrdinal("IsRead")),
                CreatedAt         = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
    }
}
