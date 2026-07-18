using MySql.Data.MySqlClient;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Implementations
{
    public class SettlementRepository : RepositoryBase<OrganizationSettlement>, ISettlementRepository
    {
        public SettlementRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "OrganizationSettlements";
        protected override string PrimaryKeyName => "SettlementID";

        protected override OrganizationSettlement Map(DbDataReader reader)
        {
            var settlement = new OrganizationSettlement
            {
                SettlementID = reader.GetInt32(reader.GetOrdinal("SettlementID")),
                OrganizationID = reader.GetInt32(reader.GetOrdinal("OrganizationID")),
                TotalGrossAmount = reader.GetDecimal(reader.GetOrdinal("TotalGrossAmount")),
                TotalCommission = reader.GetDecimal(reader.GetOrdinal("TotalCommission")),
                TotalNetAmount = reader.GetDecimal(reader.GetOrdinal("TotalNetAmount")),
                SettlementStatus = reader.GetString(reader.GetOrdinal("SettlementStatus")),
                GeneratedAt = reader.GetDateTime(reader.GetOrdinal("GeneratedAt")),
                SettledAt = reader.IsDBNull(reader.GetOrdinal("SettledAt")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("SettledAt")),
                SettledByUserID = reader.IsDBNull(reader.GetOrdinal("SettledByUserID")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("SettledByUserID")),
                PaymentReference = reader.IsDBNull(reader.GetOrdinal("PaymentReference")) ? null : reader.GetString(reader.GetOrdinal("PaymentReference")),
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes"))
            };

            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals("OrganizationName", StringComparison.OrdinalIgnoreCase))
                {
                    settlement.OrganizationName = reader.IsDBNull(i) ? null : reader.GetString(i);
                }
            }

            return settlement;
        }

        public override async Task<int> AddAsync(OrganizationSettlement entity)
        {
            var query = $@"INSERT INTO {TableName}
                (OrganizationID, TotalGrossAmount, TotalCommission, TotalNetAmount, SettlementStatus)
                VALUES
                (@OrganizationID, @TotalGrossAmount, @TotalCommission, @TotalNetAmount, @SettlementStatus);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(query);
            cmd.Parameters.AddWithValue("@OrganizationID", entity.OrganizationID);
            cmd.Parameters.AddWithValue("@TotalGrossAmount", entity.TotalGrossAmount);
            cmd.Parameters.AddWithValue("@TotalCommission", entity.TotalCommission);
            cmd.Parameters.AddWithValue("@TotalNetAmount", entity.TotalNetAmount);
            cmd.Parameters.AddWithValue("@SettlementStatus", entity.SettlementStatus);
            var id = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(id);
        }

        public override async Task<bool> UpdateAsync(OrganizationSettlement entity)
        {
            var query = $@"UPDATE {TableName} SET
                SettlementStatus = @SettlementStatus,
                SettledAt = @SettledAt,
                SettledByUserID = @SettledByUserID,
                PaymentReference = @PaymentReference,
                Notes = @Notes
                WHERE SettlementID = @SettlementID";

            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(query);
            cmd.Parameters.AddWithValue("@SettlementStatus", entity.SettlementStatus);
            cmd.Parameters.AddWithValue("@SettledAt", (object?)entity.SettledAt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SettledByUserID", (object?)entity.SettledByUserID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PaymentReference", (object?)entity.PaymentReference ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Notes", (object?)entity.Notes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SettlementID", entity.SettlementID);
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<IEnumerable<OrganizationSettlement>> SearchAndPaginateAsync(int? orgId, string? status, int page, int pageSize)
        {
            var list = new List<OrganizationSettlement>();
            var sb = new StringBuilder($@"SELECT s.*, o.OrganizationName
                                         FROM {TableName} s
                                         INNER JOIN Organizations o ON s.OrganizationID = o.OrganizationID
                                         WHERE 1=1");

            var cmd = Connection.CreateCommand();
            var tx = Transaction;
            if (tx != null) cmd.Transaction = tx;

            BuildFilterQuery(sb, cmd, orgId, status);

            sb.Append(" ORDER BY s.GeneratedAt DESC LIMIT @Limit OFFSET @Offset");
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

        public async Task<int> GetSearchCountAsync(int? orgId, string? status)
        {
            var sb = new StringBuilder($@"SELECT COUNT(*) FROM {TableName} s WHERE 1=1");

            var cmd = Connection.CreateCommand();
            var tx = Transaction;
            if (tx != null) cmd.Transaction = tx;

            BuildFilterQuery(sb, cmd, orgId, status);

            cmd.CommandText = sb.ToString();
            await EnsureConnectionOpenAsync();
            using (cmd)
            {
                return Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
        }

        private void BuildFilterQuery(StringBuilder sb, MySqlCommand cmd, int? orgId, string? status)
        {
            if (orgId.HasValue)
            {
                sb.Append(" AND s.OrganizationID = @OrgID");
                cmd.Parameters.AddWithValue("@OrgID", orgId.Value);
            }
            if (!string.IsNullOrEmpty(status))
            {
                sb.Append(" AND s.SettlementStatus = @Status");
                cmd.Parameters.AddWithValue("@Status", status);
            }
        }

        public async Task<OrganizationSettlement?> GetByIdScopedAsync(int settlementId, int? orgId)
        {
            var query = $@"SELECT s.*, o.OrganizationName
                           FROM {TableName} s
                           INNER JOIN Organizations o ON s.OrganizationID = o.OrganizationID
                           WHERE s.SettlementID = @Id";
            if (orgId.HasValue)
            {
                query += " AND s.OrganizationID = @OrgID";
            }

            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(query);
            cmd.Parameters.AddWithValue("@Id", settlementId);
            if (orgId.HasValue)
            {
                cmd.Parameters.AddWithValue("@OrgID", orgId.Value);
            }

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return Map(reader);
            }
            return null;
        }

        public async Task<IEnumerable<SettlementTransaction>> GetTransactionsBySettlementIdAsync(int settlementId)
        {
            var list = new List<SettlementTransaction>();
            var query = @"SELECT st.*,
                                 CONCAT(up.FirstName, ' ', IFNULL(up.LastName, '')) AS PatientName,
                                 CONCAT(ud.FirstName, ' ', IFNULL(ud.LastName, '')) AS DoctorName,
                                 p.PaidAt
                          FROM SettlementTransactions st
                          INNER JOIN Payments p ON st.PaymentID = p.PaymentID
                          INNER JOIN Appointments a ON p.AppointmentID = a.AppointmentID
                          INNER JOIN Patients pat ON a.PatientID = pat.PatientID
                          INNER JOIN Users up ON pat.UserID = up.UserID
                          INNER JOIN Doctors d ON a.DoctorID = d.DoctorID
                          INNER JOIN Users ud ON d.UserID = ud.UserID
                          WHERE st.SettlementID = @SettlementID
                          ORDER BY p.PaidAt";

            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(query);
            cmd.Parameters.AddWithValue("@SettlementID", settlementId);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new SettlementTransaction
                {
                    SettlementTransactionID = reader.GetInt32(reader.GetOrdinal("SettlementTransactionID")),
                    SettlementID = reader.GetInt32(reader.GetOrdinal("SettlementID")),
                    PaymentID = reader.GetInt32(reader.GetOrdinal("PaymentID")),
                    GrossAmount = reader.GetDecimal(reader.GetOrdinal("GrossAmount")),
                    CommissionAmount = reader.GetDecimal(reader.GetOrdinal("CommissionAmount")),
                    NetAmount = reader.GetDecimal(reader.GetOrdinal("NetAmount")),
                    PatientName = reader.IsDBNull(reader.GetOrdinal("PatientName")) ? null : reader.GetString(reader.GetOrdinal("PatientName")),
                    DoctorName = reader.IsDBNull(reader.GetOrdinal("DoctorName")) ? null : reader.GetString(reader.GetOrdinal("DoctorName")),
                    PaidAt = reader.IsDBNull(reader.GetOrdinal("PaidAt")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("PaidAt"))
                });
            }
            return list;
        }

        // Only Razorpay-captured payments ever reach the platform's account - manually recorded
        // Cash/offline payments (Appointments/AddPayment) never touch the platform and are excluded.
        public async Task<OrganizationSettlement?> GenerateSettlementAsync(int orgId)
        {
            var unsettledQuery = @"
                SELECT p.PaymentID, p.Amount, p.PlatformCommission, p.OrganizationAmount
                FROM Payments p
                LEFT JOIN SettlementTransactions st ON st.PaymentID = p.PaymentID
                WHERE p.OrganizationID = @OrgID
                  AND p.PaymentStatus = 'Paid'
                  AND p.PaymentMethod = 'Razorpay'
                  AND st.SettlementTransactionID IS NULL";

            await EnsureConnectionOpenAsync();

            var rows = new List<(int PaymentId, decimal Gross, decimal Commission, decimal Net)>();
            using (var cmd = CreateCommand(unsettledQuery))
            {
                cmd.Parameters.AddWithValue("@OrgID", orgId);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var gross = reader.GetDecimal(reader.GetOrdinal("Amount"));
                    var commission = reader.IsDBNull(reader.GetOrdinal("PlatformCommission")) ? 0m : reader.GetDecimal(reader.GetOrdinal("PlatformCommission"));
                    var net = reader.IsDBNull(reader.GetOrdinal("OrganizationAmount")) ? gross : reader.GetDecimal(reader.GetOrdinal("OrganizationAmount"));
                    rows.Add((reader.GetInt32(reader.GetOrdinal("PaymentID")), gross, commission, net));
                }
            }

            if (rows.Count == 0)
            {
                return null;
            }

            var settlement = new OrganizationSettlement
            {
                OrganizationID = orgId,
                TotalGrossAmount = 0,
                TotalCommission = 0,
                TotalNetAmount = 0,
                SettlementStatus = "Pending"
            };
            foreach (var row in rows)
            {
                settlement.TotalGrossAmount += row.Gross;
                settlement.TotalCommission += row.Commission;
                settlement.TotalNetAmount += row.Net;
            }

            settlement.SettlementID = await AddAsync(settlement);

            const string insertTransaction = @"
                INSERT INTO SettlementTransactions (SettlementID, PaymentID, GrossAmount, CommissionAmount, NetAmount)
                VALUES (@SettlementID, @PaymentID, @GrossAmount, @CommissionAmount, @NetAmount);";

            foreach (var row in rows)
            {
                using var cmd = CreateCommand(insertTransaction);
                cmd.Parameters.AddWithValue("@SettlementID", settlement.SettlementID);
                cmd.Parameters.AddWithValue("@PaymentID", row.PaymentId);
                cmd.Parameters.AddWithValue("@GrossAmount", row.Gross);
                cmd.Parameters.AddWithValue("@CommissionAmount", row.Commission);
                cmd.Parameters.AddWithValue("@NetAmount", row.Net);
                await cmd.ExecuteNonQueryAsync();
            }

            return settlement;
        }

        public async Task<bool> MarkPaidAsync(int settlementId, string? paymentReference, string? notes, int settledByUserId)
        {
            var query = $@"UPDATE {TableName} SET
                SettlementStatus = 'Paid',
                SettledAt = @SettledAt,
                SettledByUserID = @SettledByUserID,
                PaymentReference = @PaymentReference,
                Notes = @Notes
                WHERE SettlementID = @SettlementID AND SettlementStatus = 'Pending'";

            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(query);
            cmd.Parameters.AddWithValue("@SettledAt", DateTime.Now);
            cmd.Parameters.AddWithValue("@SettledByUserID", settledByUserId);
            cmd.Parameters.AddWithValue("@PaymentReference", (object?)paymentReference ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Notes", (object?)notes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SettlementID", settlementId);
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<int> GetPendingCountAsync(int? orgId)
        {
            var query = $"SELECT COUNT(*) FROM {TableName} WHERE SettlementStatus = 'Pending'";
            if (orgId.HasValue) query += " AND OrganizationID = @OrgID";
            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(query);
            if (orgId.HasValue) cmd.Parameters.AddWithValue("@OrgID", orgId.Value);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<int> GetCompletedCountAsync(int? orgId)
        {
            var query = $"SELECT COUNT(*) FROM {TableName} WHERE SettlementStatus = 'Paid'";
            if (orgId.HasValue) query += " AND OrganizationID = @OrgID";
            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(query);
            if (orgId.HasValue) cmd.Parameters.AddWithValue("@OrgID", orgId.Value);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<decimal> GetPendingTotalAsync(int orgId)
        {
            var query = $"SELECT IFNULL(SUM(TotalNetAmount), 0) FROM {TableName} WHERE OrganizationID = @OrgID AND SettlementStatus = 'Pending'";
            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(query);
            cmd.Parameters.AddWithValue("@OrgID", orgId);
            var result = await cmd.ExecuteScalarAsync();
            return result == null || result == DBNull.Value ? 0m : Convert.ToDecimal(result);
        }

        public async Task<OrganizationSettlement?> GetLastPaidAsync(int orgId)
        {
            var query = $@"SELECT s.*, o.OrganizationName
                           FROM {TableName} s
                           INNER JOIN Organizations o ON s.OrganizationID = o.OrganizationID
                           WHERE s.OrganizationID = @OrgID AND s.SettlementStatus = 'Paid'
                           ORDER BY s.SettledAt DESC LIMIT 1";
            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(query);
            cmd.Parameters.AddWithValue("@OrgID", orgId);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return Map(reader);
            }
            return null;
        }

        public async Task<IEnumerable<OrganizationSettlement>> GetRecentAsync(int? orgId, int count)
        {
            var list = new List<OrganizationSettlement>();
            var query = $@"SELECT s.*, o.OrganizationName
                           FROM {TableName} s
                           INNER JOIN Organizations o ON s.OrganizationID = o.OrganizationID
                           WHERE 1=1";
            if (orgId.HasValue) query += " AND s.OrganizationID = @OrgID";
            query += " ORDER BY s.GeneratedAt DESC LIMIT @Count";

            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(query);
            if (orgId.HasValue) cmd.Parameters.AddWithValue("@OrgID", orgId.Value);
            cmd.Parameters.AddWithValue("@Count", count);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(Map(reader));
            }
            return list;
        }
    }
}
