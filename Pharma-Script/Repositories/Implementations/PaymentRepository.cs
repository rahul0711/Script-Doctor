using MySql.Data.MySqlClient;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Implementations
{
    public class PaymentRepository : RepositoryBase<Payment>, IPaymentRepository
    {
        public PaymentRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "Payments";
        protected override string PrimaryKeyName => "PaymentID";

        protected override Payment Map(DbDataReader reader)
        {
            var payment = new Payment
            {
                PaymentID = reader.GetInt32(reader.GetOrdinal("PaymentID")),
                AppointmentID = reader.IsDBNull(reader.GetOrdinal("AppointmentID")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("AppointmentID")),
                OrganizationID = reader.GetInt32(reader.GetOrdinal("OrganizationID")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod")),
                TransactionReference = reader.IsDBNull(reader.GetOrdinal("TransactionReference")) ? null : reader.GetString(reader.GetOrdinal("TransactionReference")),
                PaymentStatus = reader.GetString(reader.GetOrdinal("PaymentStatus")),
                PaidAt = reader.IsDBNull(reader.GetOrdinal("PaidAt")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("PaidAt")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                Currency = reader.IsDBNull(reader.GetOrdinal("Currency")) ? "INR" : reader.GetString(reader.GetOrdinal("Currency")),
                RazorpayOrderId = reader.IsDBNull(reader.GetOrdinal("RazorpayOrderId")) ? null : reader.GetString(reader.GetOrdinal("RazorpayOrderId")),
                RazorpaySignature = reader.IsDBNull(reader.GetOrdinal("RazorpaySignature")) ? null : reader.GetString(reader.GetOrdinal("RazorpaySignature")),
                PlatformCommission = reader.IsDBNull(reader.GetOrdinal("PlatformCommission")) ? null : (decimal?)reader.GetDecimal(reader.GetOrdinal("PlatformCommission")),
                OrganizationAmount = reader.IsDBNull(reader.GetOrdinal("OrganizationAmount")) ? null : (decimal?)reader.GetDecimal(reader.GetOrdinal("OrganizationAmount")),
                RefundStatus = reader.IsDBNull(reader.GetOrdinal("RefundStatus")) ? "None" : reader.GetString(reader.GetOrdinal("RefundStatus"))
            };

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var col = reader.GetName(i);
                if (col.Equals("PatientName", StringComparison.OrdinalIgnoreCase))
                {
                    payment.PatientName = reader.IsDBNull(i) ? null : reader.GetString(i);
                }
                else if (col.Equals("DoctorName", StringComparison.OrdinalIgnoreCase))
                {
                    payment.DoctorName = reader.IsDBNull(i) ? null : reader.GetString(i);
                }
                else if (col.Equals("AppointmentDate", StringComparison.OrdinalIgnoreCase))
                {
                    payment.AppointmentDate = reader.IsDBNull(i) ? null : (DateTime?)reader.GetDateTime(i);
                }
            }

            return payment;
        }

        private void AddParameters(MySqlCommand cmd, Payment entity)
        {
            cmd.Parameters.AddWithValue("@AppointmentID", (object?)entity.AppointmentID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@OrganizationID", entity.OrganizationID);
            cmd.Parameters.AddWithValue("@Amount", entity.Amount);
            cmd.Parameters.AddWithValue("@PaymentMethod", entity.PaymentMethod);
            cmd.Parameters.AddWithValue("@TransactionReference", (object?)entity.TransactionReference ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PaymentStatus", entity.PaymentStatus);
            cmd.Parameters.AddWithValue("@PaidAt", (object?)entity.PaidAt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Currency", entity.Currency);
            cmd.Parameters.AddWithValue("@RazorpayOrderId", (object?)entity.RazorpayOrderId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RazorpaySignature", (object?)entity.RazorpaySignature ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PlatformCommission", (object?)entity.PlatformCommission ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@OrganizationAmount", (object?)entity.OrganizationAmount ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RefundStatus", entity.RefundStatus);
        }

        public override async Task<int> AddAsync(Payment entity)
        {
            var query = $@"INSERT INTO {TableName}
                (AppointmentID, OrganizationID, Amount, PaymentMethod, TransactionReference, PaymentStatus, PaidAt, Currency, RazorpayOrderId, RazorpaySignature, PlatformCommission, OrganizationAmount, RefundStatus)
                VALUES
                (@AppointmentID, @OrganizationID, @Amount, @PaymentMethod, @TransactionReference, @PaymentStatus, @PaidAt, @Currency, @RazorpayOrderId, @RazorpaySignature, @PlatformCommission, @OrganizationAmount, @RefundStatus);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var id = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
        }

        public override async Task<bool> UpdateAsync(Payment entity)
        {
            var query = $@"UPDATE {TableName} SET
                AppointmentID = @AppointmentID,
                Amount = @Amount,
                PaymentMethod = @PaymentMethod,
                TransactionReference = @TransactionReference,
                PaymentStatus = @PaymentStatus,
                PaidAt = @PaidAt,
                Currency = @Currency,
                RazorpayOrderId = @RazorpayOrderId,
                RazorpaySignature = @RazorpaySignature,
                PlatformCommission = @PlatformCommission,
                OrganizationAmount = @OrganizationAmount,
                RefundStatus = @RefundStatus
                WHERE PaymentID = @PaymentID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                cmd.Parameters.AddWithValue("@PaymentID", entity.PaymentID);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<Payment?> GetByAppointmentIdAsync(int appointmentId, int? orgId)
        {
            var query = $@"SELECT p.*,
                                  CONCAT(up.FirstName, ' ', IFNULL(up.LastName, '')) AS PatientName,
                                  CONCAT(ud.FirstName, ' ', IFNULL(ud.LastName, '')) AS DoctorName,
                                  a.AppointmentDate
                           FROM {TableName} p
                           INNER JOIN Appointments a ON p.AppointmentID = a.AppointmentID
                           INNER JOIN Patients pat ON a.PatientID = pat.PatientID
                           INNER JOIN Users up ON pat.UserID = up.UserID
                           INNER JOIN Doctors d ON a.DoctorID = d.DoctorID
                           INNER JOIN Users ud ON d.UserID = ud.UserID
                           WHERE p.AppointmentID = @AppointmentID";

            if (orgId.HasValue)
            {
                query += " AND a.OrganizationID = @OrganizationID";
            }

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@AppointmentID", appointmentId);
                if (orgId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@OrganizationID", orgId.Value);
                }

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

        public async Task<Payment?> GetByTransactionReferenceAsync(string transactionReference)
        {
            var query = $"SELECT * FROM {TableName} WHERE TransactionReference = @TransactionReference LIMIT 1";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@TransactionReference", transactionReference);
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

        public async Task<IEnumerable<Payment>> SearchAndPaginateAsync(int? orgId, string? status, string? searchTerm, int page, int pageSize)
        {
            var list = new List<Payment>();
            var sb = new StringBuilder($@"SELECT p.*,
                                                CONCAT(up.FirstName, ' ', IFNULL(up.LastName, '')) AS PatientName,
                                                CONCAT(ud.FirstName, ' ', IFNULL(ud.LastName, '')) AS DoctorName,
                                                a.AppointmentDate
                                         FROM {TableName} p
                                         INNER JOIN Appointments a ON p.AppointmentID = a.AppointmentID
                                         INNER JOIN Patients pat ON a.PatientID = pat.PatientID
                                         INNER JOIN Users up ON pat.UserID = up.UserID
                                         INNER JOIN Doctors d ON a.DoctorID = d.DoctorID
                                         INNER JOIN Users ud ON d.UserID = ud.UserID
                                         WHERE 1=1");

            var cmd = Connection.CreateCommand();
            var tx = Transaction;
            if (tx != null) cmd.Transaction = tx;

            BuildFilterQuery(sb, cmd, orgId, status, searchTerm);

            sb.Append(" ORDER BY p.CreatedAt DESC LIMIT @Limit OFFSET @Offset");
            cmd.Parameters.AddWithValue("@Limit", pageSize);
            cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);

            cmd.CommandText = sb.ToString();
            await EnsureConnectionOpenAsync();
            using (cmd)
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    list.Add(Map(reader));
                }
            }
            return list;
        }

        public async Task<int> GetSearchCountAsync(int? orgId, string? status, string? searchTerm)
        {
            var sb = new StringBuilder($@"SELECT COUNT(*)
                                         FROM {TableName} p
                                         INNER JOIN Appointments a ON p.AppointmentID = a.AppointmentID
                                         INNER JOIN Patients pat ON a.PatientID = pat.PatientID
                                         INNER JOIN Users up ON pat.UserID = up.UserID
                                         INNER JOIN Doctors d ON a.DoctorID = d.DoctorID
                                         INNER JOIN Users ud ON d.UserID = ud.UserID
                                         WHERE 1=1");

            var cmd = Connection.CreateCommand();
            var tx = Transaction;
            if (tx != null) cmd.Transaction = tx;

            BuildFilterQuery(sb, cmd, orgId, status, searchTerm);

            cmd.CommandText = sb.ToString();
            await EnsureConnectionOpenAsync();
            using (cmd)
            {
                return Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
        }

        private void BuildFilterQuery(StringBuilder sb, MySqlCommand cmd, int? orgId, string? status, string? searchTerm)
        {
            if (orgId.HasValue)
            {
                sb.Append(" AND a.OrganizationID = @OrgID");
                cmd.Parameters.AddWithValue("@OrgID", orgId.Value);
            }
            if (!string.IsNullOrEmpty(status))
            {
                sb.Append(" AND p.PaymentStatus = @Status");
                cmd.Parameters.AddWithValue("@Status", status);
            }
            if (!string.IsNullOrEmpty(searchTerm))
            {
                sb.Append(" AND (up.FirstName LIKE @Term OR up.LastName LIKE @Term OR ud.FirstName LIKE @Term OR ud.LastName LIKE @Term OR p.TransactionReference LIKE @Term)");
                cmd.Parameters.AddWithValue("@Term", $"%{searchTerm}%");
            }
        }

        public async Task<decimal> GetTotalByDoctorIdAsync(int doctorId, int? orgId)
        {
            var query = $@"SELECT IFNULL(SUM(p.Amount), 0)
                           FROM {TableName} p
                           INNER JOIN Appointments a ON p.AppointmentID = a.AppointmentID
                           WHERE a.DoctorID = @DoctorID
                             AND p.PaymentStatus = 'Paid'";

            if (orgId.HasValue)
                query += " AND a.OrganizationID = @OrgID";

            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(query);
            cmd.Parameters.AddWithValue("@DoctorID", doctorId);
            if (orgId.HasValue)
                cmd.Parameters.AddWithValue("@OrgID", orgId.Value);

            var result = await cmd.ExecuteScalarAsync();
            return result == null || result == DBNull.Value ? 0m : Convert.ToDecimal(result);
        }

        public async Task<decimal> GetTotalByOrgIdAsync(int orgId)
        {
            var query = $@"SELECT IFNULL(SUM(Amount), 0)
                           FROM {TableName}
                           WHERE OrganizationID = @OrgID
                             AND PaymentStatus = 'Paid'";

            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(query);
            cmd.Parameters.AddWithValue("@OrgID", orgId);

            var result = await cmd.ExecuteScalarAsync();
            return result == null || result == DBNull.Value ? 0m : Convert.ToDecimal(result);
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            var query = $"SELECT IFNULL(SUM(Amount), 0) FROM {TableName} WHERE PaymentStatus = 'Paid'";
            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(query);
            var result = await cmd.ExecuteScalarAsync();
            return result == null || result == DBNull.Value ? 0m : Convert.ToDecimal(result);
        }

        public async Task<decimal> GetTodayRevenueAsync()
        {
            var query = $"SELECT IFNULL(SUM(Amount), 0) FROM {TableName} WHERE PaymentStatus = 'Paid' AND DATE(PaidAt) = CURDATE()";
            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(query);
            var result = await cmd.ExecuteScalarAsync();
            return result == null || result == DBNull.Value ? 0m : Convert.ToDecimal(result);
        }

        public async Task<IEnumerable<(DateTime Date, decimal Amount)>> GetDailyRevenueAsync(int days)
        {
            var list = new List<(DateTime Date, decimal Amount)>();
            var query = $@"SELECT DATE(PaidAt) AS RevenueDate, SUM(Amount) AS Total
                           FROM {TableName}
                           WHERE PaymentStatus = 'Paid' AND PaidAt >= @Since
                           GROUP BY DATE(PaidAt)
                           ORDER BY RevenueDate";

            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(query);
            cmd.Parameters.AddWithValue("@Since", DateTime.Today.AddDays(-(days - 1)));
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add((reader.GetDateTime(0), reader.GetDecimal(1)));
            }
            return list;
        }

        public async Task<decimal> GetTotalOrganizationEarningsAsync(int orgId)
        {
            var query = $@"SELECT IFNULL(SUM(OrganizationAmount), 0)
                           FROM {TableName}
                           WHERE OrganizationID = @OrgID AND PaymentStatus = 'Paid'";
            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(query);
            cmd.Parameters.AddWithValue("@OrgID", orgId);
            var result = await cmd.ExecuteScalarAsync();
            return result == null || result == DBNull.Value ? 0m : Convert.ToDecimal(result);
        }

        public async Task<decimal> GetTodayOrganizationEarningsAsync(int orgId)
        {
            var query = $@"SELECT IFNULL(SUM(OrganizationAmount), 0)
                           FROM {TableName}
                           WHERE OrganizationID = @OrgID AND PaymentStatus = 'Paid' AND DATE(PaidAt) = CURDATE()";
            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(query);
            cmd.Parameters.AddWithValue("@OrgID", orgId);
            var result = await cmd.ExecuteScalarAsync();
            return result == null || result == DBNull.Value ? 0m : Convert.ToDecimal(result);
        }

        public async Task<IEnumerable<Payment>> GetByPatientAndOrgAsync(int patientId, int orgId)
        {
            var list = new List<Payment>();
            var query = $@"SELECT p.*,
                                   CONCAT(up.FirstName, ' ', IFNULL(up.LastName, '')) AS PatientName,
                                   CONCAT(ud.FirstName, ' ', IFNULL(ud.LastName, '')) AS DoctorName,
                                   a.AppointmentDate
                            FROM {TableName} p
                            INNER JOIN Appointments a ON p.AppointmentID = a.AppointmentID
                            INNER JOIN Patients pat ON a.PatientID = pat.PatientID
                            INNER JOIN Users up ON pat.UserID = up.UserID
                            INNER JOIN Doctors d ON a.DoctorID = d.DoctorID
                            INNER JOIN Users ud ON d.UserID = ud.UserID
                            WHERE a.PatientID = @PatientID
                              AND a.OrganizationID = @OrgID
                            ORDER BY a.AppointmentDate DESC";

            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(query);
            cmd.Parameters.AddWithValue("@PatientID", patientId);
            cmd.Parameters.AddWithValue("@OrgID", orgId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                list.Add(Map(reader));
            return list;
        }
    }
}
