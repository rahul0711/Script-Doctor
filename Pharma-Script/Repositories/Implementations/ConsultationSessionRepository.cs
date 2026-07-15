using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;

namespace Pharma_Script.Repositories.Implementations
{
    public class ConsultationSessionRepository : IConsultationSessionRepository
    {
        private readonly MySqlConnection _connection;
        private readonly Func<MySqlTransaction?> _transactionProvider;

        public ConsultationSessionRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
        {
            _connection = connection;
            _transactionProvider = transactionProvider;
        }

        private async Task EnsureConnectionOpenAsync()
        {
            if (_connection.State != ConnectionState.Open)
            {
                await _connection.OpenAsync();
            }
        }

        public async Task<ConsultationSession?> GetByAppointmentIdAsync(int appointmentId, int organizationId)
        {
            try
            {
                await EnsureConnectionOpenAsync();
                var query = @"SELECT * FROM ConsultationSessions 
                              WHERE AppointmentID = @ApptId AND OrganizationID = @OrgId";

                using var cmd = new MySqlCommand(query, _connection, _transactionProvider());
                cmd.Parameters.AddWithValue("@ApptId", appointmentId);
                cmd.Parameters.AddWithValue("@OrgId", organizationId);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return Map(reader);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConsultationSessionRepository] GetByAppointmentIdAsync failed: {ex.Message}");
            }
            return null;
        }

        public async Task<ConsultationSession?> GetByIdAsync(int sessionId, int organizationId)
        {
            try
            {
                await EnsureConnectionOpenAsync();
                var query = @"SELECT * FROM ConsultationSessions 
                              WHERE ConsultationSessionID = @Id AND OrganizationID = @OrgId";

                using var cmd = new MySqlCommand(query, _connection, _transactionProvider());
                cmd.Parameters.AddWithValue("@Id", sessionId);
                cmd.Parameters.AddWithValue("@OrgId", organizationId);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return Map(reader);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConsultationSessionRepository] GetByIdAsync failed: {ex.Message}");
            }
            return null;
        }

        public async Task<int> CreateAsync(ConsultationSession session)
        {
            try
            {
                await EnsureConnectionOpenAsync();
                var query = @"INSERT INTO ConsultationSessions 
                              (OrganizationID, AppointmentID, DoctorID, PatientID, ConsultationType, 
                               MeetingProvider, MeetingURL, SessionStatus, CreatedByUserID, CreatedAt, UpdatedAt)
                              VALUES 
                              (@OrgId, @ApptId, @DocId, @PatId, @Type, @Provider, @Url, @Status, @CreatedBy, @CreatedAt, @UpdatedAt);
                              SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(query, _connection, _transactionProvider());
                cmd.Parameters.AddWithValue("@OrgId", session.OrganizationID);
                cmd.Parameters.AddWithValue("@ApptId", session.AppointmentID);
                cmd.Parameters.AddWithValue("@DocId", session.DoctorID);
                cmd.Parameters.AddWithValue("@PatId", session.PatientID);
                cmd.Parameters.AddWithValue("@Type", session.ConsultationType);
                cmd.Parameters.AddWithValue("@Provider", (object?)session.MeetingProvider ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Url", (object?)session.MeetingURL ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", session.SessionStatus);
                cmd.Parameters.AddWithValue("@CreatedBy", session.CreatedByUserID);
                cmd.Parameters.AddWithValue("@CreatedAt", session.CreatedAt);
                cmd.Parameters.AddWithValue("@UpdatedAt", session.UpdatedAt);

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConsultationSessionRepository] CreateAsync failed: {ex.Message}");
                return 0;
            }
        }

        public async Task<bool> UpdateAsync(ConsultationSession session)
        {
            try
            {
                await EnsureConnectionOpenAsync();
                var query = @"UPDATE ConsultationSessions 
                              SET MeetingProvider = @Provider,
                                  MeetingURL = @Url,
                                  SessionStatus = @Status,
                                  UpdatedAt = @UpdatedAt
                              WHERE ConsultationSessionID = @Id AND OrganizationID = @OrgId";

                using var cmd = new MySqlCommand(query, _connection, _transactionProvider());
                cmd.Parameters.AddWithValue("@Provider", (object?)session.MeetingProvider ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Url", (object?)session.MeetingURL ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", session.SessionStatus);
                cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@Id", session.ConsultationSessionID);
                cmd.Parameters.AddWithValue("@OrgId", session.OrganizationID);

                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConsultationSessionRepository] UpdateAsync failed: {ex.Message}");
                return false;
            }
        }

        private ConsultationSession Map(DbDataReader reader)
        {
            return new ConsultationSession
            {
                ConsultationSessionID = reader.GetInt32(reader.GetOrdinal("ConsultationSessionID")),
                OrganizationID = reader.GetInt32(reader.GetOrdinal("OrganizationID")),
                AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID")),
                DoctorID = reader.GetInt32(reader.GetOrdinal("DoctorID")),
                PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                ConsultationType = reader.GetString(reader.GetOrdinal("ConsultationType")),
                MeetingProvider = reader.IsDBNull(reader.GetOrdinal("MeetingProvider")) ? null : reader.GetString(reader.GetOrdinal("MeetingProvider")),
                MeetingURL = reader.IsDBNull(reader.GetOrdinal("MeetingURL")) ? null : reader.GetString(reader.GetOrdinal("MeetingURL")),
                SessionStatus = reader.GetString(reader.GetOrdinal("SessionStatus")),
                CreatedByUserID = reader.GetInt32(reader.GetOrdinal("CreatedByUserID")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }
    }
}
