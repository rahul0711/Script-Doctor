using MySql.Data.MySqlClient;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Implementations
{
    public class DoctorPaymentGatewayRepository : RepositoryBase<DoctorPaymentGateway>, IDoctorPaymentGatewayRepository
    {
        public DoctorPaymentGatewayRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "DoctorPaymentGateway";
        protected override string PrimaryKeyName => "DoctorPaymentGatewayID";

        protected override DoctorPaymentGateway Map(DbDataReader reader)
        {
            return new DoctorPaymentGateway
            {
                DoctorPaymentGatewayID = reader.GetInt32(reader.GetOrdinal("DoctorPaymentGatewayID")),
                OrganizationID = reader.GetInt32(reader.GetOrdinal("OrganizationID")),
                DoctorID = reader.GetInt32(reader.GetOrdinal("DoctorID")),
                PaymentProvider = reader.GetString(reader.GetOrdinal("PaymentProvider")),
                KeyID = reader.GetString(reader.GetOrdinal("KeyID")),
                KeySecret = reader.GetString(reader.GetOrdinal("KeySecret")),
                IsActive = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("IsActive"))),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt"))
                    ? reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                    : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }

        private void AddParameters(MySqlCommand cmd, DoctorPaymentGateway entity)
        {
            cmd.Parameters.AddWithValue("@OrganizationID", entity.OrganizationID);
            cmd.Parameters.AddWithValue("@DoctorID", entity.DoctorID);
            cmd.Parameters.AddWithValue("@PaymentProvider", entity.PaymentProvider);
            cmd.Parameters.AddWithValue("@KeyID", entity.KeyID);
            cmd.Parameters.AddWithValue("@KeySecret", entity.KeySecret);
            cmd.Parameters.AddWithValue("@IsActive", entity.IsActive);
        }

        public override async Task<int> AddAsync(DoctorPaymentGateway entity)
        {
            var query = $@"INSERT INTO {TableName}
                (OrganizationID, DoctorID, PaymentProvider, KeyID, KeySecret, IsActive)
                VALUES
                (@OrganizationID, @DoctorID, @PaymentProvider, @KeyID, @KeySecret, @IsActive);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var id = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
        }

        public override async Task<bool> UpdateAsync(DoctorPaymentGateway entity)
        {
            var query = $@"UPDATE {TableName} SET
                KeyID = @KeyID,
                KeySecret = @KeySecret,
                IsActive = @IsActive
                WHERE {PrimaryKeyName} = @Id AND DoctorID = @DoctorID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                cmd.Parameters.AddWithValue("@Id", entity.DoctorPaymentGatewayID);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        // One row per (DoctorID, PaymentProvider) by schema constraint - this is always the doctor's
        // single Razorpay config, regardless of IsActive (verification of an already-captured payment
        // must still work even if an admin later disables online payments for this doctor).
        public async Task<DoctorPaymentGateway?> GetByDoctorIdAsync(int doctorId, string provider = "Razorpay")
        {
            var query = $"SELECT * FROM {TableName} WHERE DoctorID = @DoctorID AND PaymentProvider = @Provider LIMIT 1";
            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@DoctorID", doctorId);
                cmd.Parameters.AddWithValue("@Provider", provider);
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

        public async Task<int> UpsertAsync(DoctorPaymentGateway entity)
        {
            var existing = await GetByDoctorIdAsync(entity.DoctorID, entity.PaymentProvider);
            if (existing == null)
            {
                return await AddAsync(entity);
            }

            entity.DoctorPaymentGatewayID = existing.DoctorPaymentGatewayID;
            await UpdateAsync(entity);
            return existing.DoctorPaymentGatewayID;
        }
    }
}
