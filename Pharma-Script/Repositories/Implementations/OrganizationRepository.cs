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
    public class OrganizationRepository : RepositoryBase<Organization>, IOrganizationRepository
    {
        public OrganizationRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "Organizations";
        protected override string PrimaryKeyName => "OrganizationID";

        protected override Organization Map(DbDataReader reader)
        {
            return new Organization
            {
                OrganizationID = reader.GetInt32(reader.GetOrdinal("OrganizationID")),
                OrganizationName = reader.GetString(reader.GetOrdinal("OrganizationName")),
                OrganizationSlug = reader.IsDBNull(reader.GetOrdinal("OrganizationSlug")) ? null : reader.GetString(reader.GetOrdinal("OrganizationSlug")),
                OrganizationType = reader.GetString(reader.GetOrdinal("OrganizationType")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                AlternatePhone = reader.IsDBNull(reader.GetOrdinal("AlternatePhone")) ? null : reader.GetString(reader.GetOrdinal("AlternatePhone")),
                AddressLine1 = reader.GetString(reader.GetOrdinal("AddressLine1")),
                AddressLine2 = reader.IsDBNull(reader.GetOrdinal("AddressLine2")) ? null : reader.GetString(reader.GetOrdinal("AddressLine2")),
                City = reader.GetString(reader.GetOrdinal("City")),
                State = reader.GetString(reader.GetOrdinal("State")),
                Country = reader.GetString(reader.GetOrdinal("Country")),
                Pincode = reader.GetString(reader.GetOrdinal("Pincode")),
                GSTNumber = reader.IsDBNull(reader.GetOrdinal("GSTNumber")) ? null : reader.GetString(reader.GetOrdinal("GSTNumber")),
                LicenseNumber = reader.IsDBNull(reader.GetOrdinal("LicenseNumber")) ? null : reader.GetString(reader.GetOrdinal("LicenseNumber")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }

        private void AddParameters(MySqlCommand cmd, Organization entity)
        {
            cmd.Parameters.AddWithValue("@OrganizationName", entity.OrganizationName);
            cmd.Parameters.AddWithValue("@OrganizationSlug", (object?)entity.OrganizationSlug ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@OrganizationType", entity.OrganizationType);
            cmd.Parameters.AddWithValue("@Email", entity.Email);
            cmd.Parameters.AddWithValue("@Phone", entity.Phone);
            cmd.Parameters.AddWithValue("@AlternatePhone", (object?)entity.AlternatePhone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AddressLine1", entity.AddressLine1);
            cmd.Parameters.AddWithValue("@AddressLine2", (object?)entity.AddressLine2 ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@City", entity.City);
            cmd.Parameters.AddWithValue("@State", entity.State);
            cmd.Parameters.AddWithValue("@Country", entity.Country);
            cmd.Parameters.AddWithValue("@Pincode", entity.Pincode);
            cmd.Parameters.AddWithValue("@GSTNumber", (object?)entity.GSTNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LicenseNumber", (object?)entity.LicenseNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", entity.IsActive);
            cmd.Parameters.AddWithValue("@CreatedAt", entity.CreatedAt);
            cmd.Parameters.AddWithValue("@UpdatedAt", (object?)entity.UpdatedAt ?? DBNull.Value);
        }

        public override async Task<int> AddAsync(Organization entity)
        {
            var query = $@"INSERT INTO {TableName}
                (OrganizationName, OrganizationSlug, OrganizationType, Email, Phone, AlternatePhone, AddressLine1, AddressLine2, City, State, Country, Pincode, GSTNumber, LicenseNumber, IsActive, CreatedAt, UpdatedAt)
                VALUES
                (@OrganizationName, @OrganizationSlug, @OrganizationType, @Email, @Phone, @AlternatePhone, @AddressLine1, @AddressLine2, @City, @State, @Country, @Pincode, @GSTNumber, @LicenseNumber, @IsActive, @CreatedAt, @UpdatedAt);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var idObj = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(idObj);
            }
        }

        public override async Task<bool> UpdateAsync(Organization entity)
        {
            var query = $@"UPDATE {TableName} SET
                OrganizationName = @OrganizationName,
                OrganizationSlug = @OrganizationSlug,
                OrganizationType = @OrganizationType,
                Email = @Email, 
                Phone = @Phone, 
                AlternatePhone = @AlternatePhone, 
                AddressLine1 = @AddressLine1, 
                AddressLine2 = @AddressLine2, 
                City = @City, 
                State = @State, 
                Country = @Country, 
                Pincode = @Pincode, 
                GSTNumber = @GSTNumber, 
                LicenseNumber = @LicenseNumber, 
                IsActive = @IsActive, 
                UpdatedAt = @UpdatedAt 
                WHERE OrganizationID = @OrganizationID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                cmd.Parameters.AddWithValue("@OrganizationID", entity.OrganizationID);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<IEnumerable<Organization>> SearchAndPaginateAsync(string searchTerm, int page, int pageSize)
        {
            var offset = (page - 1) * pageSize;
            var list = new List<Organization>();
            var query = $@"SELECT * FROM {TableName} 
                WHERE (@SearchTerm = '' OR OrganizationName LIKE CONCAT('%', @SearchTerm, '%') 
                       OR City LIKE CONCAT('%', @SearchTerm, '%') 
                       OR OrganizationType LIKE CONCAT('%', @SearchTerm, '%'))
                ORDER BY OrganizationID DESC 
                LIMIT @Limit OFFSET @Offset";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", searchTerm ?? "");
                cmd.Parameters.AddWithValue("@Limit", pageSize);
                cmd.Parameters.AddWithValue("@Offset", offset);

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

        public async Task<int> GetSearchCountAsync(string searchTerm)
        {
            var query = $@"SELECT COUNT(*) FROM {TableName} 
                WHERE (@SearchTerm = '' OR OrganizationName LIKE CONCAT('%', @SearchTerm, '%') 
                       OR City LIKE CONCAT('%', @SearchTerm, '%') 
                       OR OrganizationType LIKE CONCAT('%', @SearchTerm, '%'))";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", searchTerm ?? "");
                var count = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(count);
            }
        }

        public async Task<Organization?> GetBySlugAsync(string slug)
        {
            var query = $"SELECT * FROM {TableName} WHERE OrganizationSlug = @Slug AND IsActive = 1 LIMIT 1";
            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@Slug", slug);
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

        public async Task<bool> IsSlugTakenAsync(string slug, int? excludeOrganizationId)
        {
            var query = $@"SELECT COUNT(*) FROM {TableName}
                WHERE OrganizationSlug = @Slug
                AND (@ExcludeID IS NULL OR OrganizationID != @ExcludeID)";
            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@Slug", slug);
                cmd.Parameters.AddWithValue("@ExcludeID", (object?)excludeOrganizationId ?? DBNull.Value);
                var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return count > 0;
            }
        }

        public async Task<bool> UpdateStatusAsync(int id, bool isActive)
        {
            var query = $"UPDATE {TableName} SET IsActive = @IsActive, UpdatedAt = @UpdatedAt WHERE OrganizationID = @OrganizationID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@IsActive", isActive);
                cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                cmd.Parameters.AddWithValue("@OrganizationID", id);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        // Deleting an Organization must first clear every dependent row across the
        // schema, since none of those foreign keys cascade on delete. Tables that
        // every code path is known to create are deleted unconditionally; a few
        // legacy/optional tables (Reviews, NotificationTemplates, NotificationLogs,
        // HomepageSections) are only referenced in old reference SQL dumps and may
        // never have been created on a given database, so those are skipped
        // gracefully if MySQL reports the table doesn't exist (error 1146).
        public override async Task<bool> DeleteAsync(int id)
        {
            await EnsureConnectionOpenAsync();

            var ownsTransaction = Transaction == null;
            var tx = Transaction ?? await Connection.BeginTransactionAsync();

            async Task ExecAsync(string sql)
            {
                using var cmd = Connection.CreateCommand();
                cmd.CommandText = sql;
                cmd.Transaction = tx;
                cmd.Parameters.AddWithValue("@OrganizationID", id);
                await cmd.ExecuteNonQueryAsync();
            }

            async Task TryExecAsync(string sql)
            {
                try
                {
                    await ExecAsync(sql);
                }
                catch (MySqlException ex) when (ex.Number == 1146) // table doesn't exist
                {
                }
            }

            try
            {
                await ExecAsync("DELETE FROM PrescriptionMedicines WHERE PrescriptionID IN (SELECT PrescriptionID FROM Prescriptions WHERE OrganizationID = @OrganizationID)");
                await ExecAsync("DELETE FROM Prescriptions WHERE OrganizationID = @OrganizationID");
                await ExecAsync("DELETE FROM Payments WHERE OrganizationID = @OrganizationID");
                await ExecAsync("DELETE FROM FollowUps WHERE OrganizationID = @OrganizationID");
                await ExecAsync("DELETE FROM DoctorNotes WHERE OrganizationID = @OrganizationID");
                await ExecAsync("DELETE FROM ConsultationSessions WHERE OrganizationID = @OrganizationID");
                await TryExecAsync("DELETE FROM Reviews WHERE OrganizationID = @OrganizationID");
                await ExecAsync("DELETE FROM AppointmentStatusHistory WHERE AppointmentID IN (SELECT AppointmentID FROM Appointments WHERE OrganizationID = @OrganizationID)");
                await ExecAsync("DELETE FROM Appointments WHERE OrganizationID = @OrganizationID");
                await ExecAsync("DELETE FROM MedicalDocuments WHERE OrganizationID = @OrganizationID");
                await TryExecAsync("DELETE FROM NotificationLogs WHERE NotificationID IN (SELECT NotificationID FROM Notifications WHERE OrganizationID = @OrganizationID)");
                await ExecAsync("DELETE FROM Notifications WHERE OrganizationID = @OrganizationID");
                await ExecAsync("DELETE FROM DoctorSpecializations WHERE DoctorID IN (SELECT DoctorID FROM Doctors WHERE OrganizationID = @OrganizationID)");
                await ExecAsync("DELETE FROM DoctorAvailability WHERE DoctorID IN (SELECT DoctorID FROM Doctors WHERE OrganizationID = @OrganizationID)");
                await ExecAsync("DELETE FROM DoctorLeave WHERE DoctorID IN (SELECT DoctorID FROM Doctors WHERE OrganizationID = @OrganizationID)");
                await ExecAsync("DELETE FROM Doctors WHERE OrganizationID = @OrganizationID");
                await ExecAsync("DELETE FROM PatientMedicalHistory WHERE PatientID IN (SELECT PatientID FROM Patients WHERE OrganizationID = @OrganizationID)");
                await ExecAsync("DELETE FROM PatientVitals WHERE PatientID IN (SELECT PatientID FROM Patients WHERE OrganizationID = @OrganizationID)");
                await ExecAsync("DELETE FROM Patients WHERE OrganizationID = @OrganizationID");
                await ExecAsync("DELETE FROM Receptionists WHERE OrganizationID = @OrganizationID");
                await ExecAsync("DELETE FROM CMSSettings WHERE OrganizationID = @OrganizationID");
                await ExecAsync("DELETE FROM HeroSections WHERE OrganizationID = @OrganizationID");
                await ExecAsync("DELETE FROM Services WHERE OrganizationID = @OrganizationID");
                await ExecAsync("DELETE FROM Gallery WHERE OrganizationID = @OrganizationID");
                await ExecAsync("DELETE FROM FAQs WHERE OrganizationID = @OrganizationID");
                await ExecAsync("DELETE FROM ContactMessages WHERE OrganizationID = @OrganizationID");
                await TryExecAsync("DELETE FROM HomepageSections WHERE OrganizationID = @OrganizationID");
                await TryExecAsync("DELETE FROM NotificationTemplates WHERE OrganizationID = @OrganizationID");
                await ExecAsync("DELETE FROM Departments WHERE OrganizationID = @OrganizationID");
                await ExecAsync("DELETE FROM Users WHERE OrganizationID = @OrganizationID");
                await ExecAsync("DELETE FROM Branches WHERE OrganizationID = @OrganizationID");

                int rows;
                using (var cmd = Connection.CreateCommand())
                {
                    cmd.CommandText = $"DELETE FROM {TableName} WHERE {PrimaryKeyName} = @OrganizationID";
                    cmd.Transaction = tx;
                    cmd.Parameters.AddWithValue("@OrganizationID", id);
                    rows = await cmd.ExecuteNonQueryAsync();
                }

                if (ownsTransaction)
                {
                    await tx.CommitAsync();
                }
                return rows > 0;
            }
            catch
            {
                if (ownsTransaction)
                {
                    await tx.RollbackAsync();
                }
                throw;
            }
            finally
            {
                if (ownsTransaction)
                {
                    await tx.DisposeAsync();
                }
            }
        }
    }
}
