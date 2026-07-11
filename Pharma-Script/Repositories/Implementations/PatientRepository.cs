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
    public class PatientRepository : RepositoryBase<Patient>, IPatientRepository
    {
        public PatientRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "Patients";
        protected override string PrimaryKeyName => "PatientID";

        protected override Patient Map(DbDataReader reader)
        {
            var patient = new Patient
            {
                PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                OrganizationID = reader.GetInt32(reader.GetOrdinal("OrganizationID")),
                BranchID = reader.IsDBNull(reader.GetOrdinal("BranchID")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("BranchID")),
                DateOfBirth = reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                Gender = reader.GetString(reader.GetOrdinal("Gender")),
                BloodGroup = reader.IsDBNull(reader.GetOrdinal("BloodGroup")) ? null : reader.GetString(reader.GetOrdinal("BloodGroup")),
                Height = reader.IsDBNull(reader.GetOrdinal("Height")) ? null : (decimal?)reader.GetDecimal(reader.GetOrdinal("Height")),
                Weight = reader.IsDBNull(reader.GetOrdinal("Weight")) ? null : (decimal?)reader.GetDecimal(reader.GetOrdinal("Weight")),
                EmergencyContactName = reader.IsDBNull(reader.GetOrdinal("EmergencyContactName")) ? null : reader.GetString(reader.GetOrdinal("EmergencyContactName")),
                EmergencyContactNumber = reader.IsDBNull(reader.GetOrdinal("EmergencyContactNumber")) ? null : reader.GetString(reader.GetOrdinal("EmergencyContactNumber")),
                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                City = reader.IsDBNull(reader.GetOrdinal("City")) ? null : reader.GetString(reader.GetOrdinal("City")),
                State = reader.IsDBNull(reader.GetOrdinal("State")) ? null : reader.GetString(reader.GetOrdinal("State")),
                Country = reader.IsDBNull(reader.GetOrdinal("Country")) ? null : reader.GetString(reader.GetOrdinal("Country")),
                Pincode = reader.IsDBNull(reader.GetOrdinal("Pincode")) ? null : reader.GetString(reader.GetOrdinal("Pincode")),
                IsActive = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("IsActive"))),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                if (name.Equals("FirstName", StringComparison.OrdinalIgnoreCase))
                    patient.FirstName = reader.GetString(i);
                else if (name.Equals("LastName", StringComparison.OrdinalIgnoreCase))
                    patient.LastName = reader.IsDBNull(i) ? null : reader.GetString(i);
                else if (name.Equals("Email", StringComparison.OrdinalIgnoreCase))
                    patient.Email = reader.GetString(i);
                else if (name.Equals("Phone", StringComparison.OrdinalIgnoreCase))
                    patient.Phone = reader.GetString(i);
                else if (name.Equals("BranchName", StringComparison.OrdinalIgnoreCase))
                    patient.BranchName = reader.IsDBNull(i) ? null : reader.GetString(i);
                else if (name.Equals("OrganizationName", StringComparison.OrdinalIgnoreCase))
                    patient.OrganizationName = reader.IsDBNull(i) ? null : reader.GetString(i);
            }

            return patient;
        }

        private void AddParameters(MySqlCommand cmd, Patient entity)
        {
            cmd.Parameters.AddWithValue("@UserID", entity.UserID);
            cmd.Parameters.AddWithValue("@OrganizationID", entity.OrganizationID);
            cmd.Parameters.AddWithValue("@BranchID", (object?)entity.BranchID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DateOfBirth", entity.DateOfBirth.Date);
            cmd.Parameters.AddWithValue("@Gender", entity.Gender);
            cmd.Parameters.AddWithValue("@BloodGroup", (object?)entity.BloodGroup ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Height", (object?)entity.Height ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Weight", (object?)entity.Weight ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EmergencyContactName", (object?)entity.EmergencyContactName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EmergencyContactNumber", (object?)entity.EmergencyContactNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object?)entity.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@City", (object?)entity.City ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@State", (object?)entity.State ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Country", (object?)entity.Country ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Pincode", (object?)entity.Pincode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", entity.IsActive);
        }

        public override async Task<int> AddAsync(Patient entity)
        {
            var query = $@"INSERT INTO {TableName} 
                (UserID, OrganizationID, BranchID, DateOfBirth, Gender, BloodGroup, Height, Weight, 
                 EmergencyContactName, EmergencyContactNumber, Address, City, State, Country, Pincode, IsActive) 
                VALUES 
                (@UserID, @OrganizationID, @BranchID, @DateOfBirth, @Gender, @BloodGroup, @Height, @Weight, 
                 @EmergencyContactName, @EmergencyContactNumber, @Address, @City, @State, @Country, @Pincode, @IsActive);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var id = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
        }

        public override async Task<bool> UpdateAsync(Patient entity)
        {
            var query = $@"UPDATE {TableName} SET 
                BranchID = @BranchID, 
                DateOfBirth = @DateOfBirth, 
                Gender = @Gender, 
                BloodGroup = @BloodGroup, 
                Height = @Height, 
                Weight = @Weight, 
                EmergencyContactName = @EmergencyContactName, 
                EmergencyContactNumber = @EmergencyContactNumber, 
                Address = @Address, 
                City = @City, 
                State = @State, 
                Country = @Country, 
                Pincode = @Pincode, 
                IsActive = @IsActive
                WHERE PatientID = @PatientID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                cmd.Parameters.AddWithValue("@PatientID", entity.PatientID);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<bool> UpdateStatusAsync(int id, bool isActive)
        {
            var query = $"UPDATE {TableName} SET IsActive = @IsActive WHERE PatientID = @PatientID";
            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@IsActive", isActive);
                cmd.Parameters.AddWithValue("@PatientID", id);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<Patient?> GetPatientDetailsByIdAsync(int id, int? orgId)
        {
            var query = $@"SELECT p.*, u.FirstName, u.LastName, u.Email, u.Phone, b.BranchName, o.OrganizationName
                           FROM {TableName} p
                           INNER JOIN Users u ON p.UserID = u.UserID
                           LEFT JOIN Branches b ON p.BranchID = b.BranchID
                           LEFT JOIN Organizations o ON p.OrganizationID = o.OrganizationID
                           WHERE p.PatientID = @PatientID";

            if (orgId.HasValue)
            {
                query += " AND p.OrganizationID = @OrganizationID";
            }

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@PatientID", id);
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

        public async Task<IEnumerable<Patient>> SearchAndPaginateAsync(int? orgId, int? branchId, string searchTerm, int page, int pageSize)
        {
            var offset = (page - 1) * pageSize;
            var list = new List<Patient>();

            var sb = new StringBuilder();
            sb.Append($@"SELECT p.*, u.FirstName, u.LastName, u.Email, u.Phone, b.BranchName, o.OrganizationName
                         FROM {TableName} p
                         INNER JOIN Users u ON p.UserID = u.UserID
                         LEFT JOIN Branches b ON p.BranchID = b.BranchID
                         LEFT JOIN Organizations o ON p.OrganizationID = o.OrganizationID
                         WHERE 1=1");

            if (orgId.HasValue)
            {
                sb.Append(" AND p.OrganizationID = @OrganizationID");
            }
            if (branchId.HasValue)
            {
                sb.Append(" AND p.BranchID = @BranchID");
            }
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sb.Append(" AND (u.FirstName LIKE @SearchTerm OR u.LastName LIKE @SearchTerm OR u.Email LIKE @SearchTerm OR u.Phone LIKE @SearchTerm OR p.City LIKE @SearchTerm)");
            }

            sb.Append(" ORDER BY p.PatientID DESC LIMIT @Limit OFFSET @Offset");

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(sb.ToString()))
            {
                if (orgId.HasValue) cmd.Parameters.AddWithValue("@OrganizationID", orgId.Value);
                if (branchId.HasValue) cmd.Parameters.AddWithValue("@BranchID", branchId.Value);
                if (!string.IsNullOrWhiteSpace(searchTerm)) cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
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

        public async Task<int> GetSearchCountAsync(int? orgId, int? branchId, string searchTerm)
        {
            var sb = new StringBuilder();
            sb.Append(@"SELECT COUNT(*)
                        FROM Patients p
                        INNER JOIN Users u ON p.UserID = u.UserID
                        WHERE 1=1");

            if (orgId.HasValue)
            {
                sb.Append(" AND p.OrganizationID = @OrganizationID");
            }
            if (branchId.HasValue)
            {
                sb.Append(" AND p.BranchID = @BranchID");
            }
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sb.Append(" AND (u.FirstName LIKE @SearchTerm OR u.LastName LIKE @SearchTerm OR u.Email LIKE @SearchTerm OR u.Phone LIKE @SearchTerm OR p.City LIKE @SearchTerm)");
            }

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(sb.ToString()))
            {
                if (orgId.HasValue) cmd.Parameters.AddWithValue("@OrganizationID", orgId.Value);
                if (branchId.HasValue) cmd.Parameters.AddWithValue("@BranchID", branchId.Value);
                if (!string.IsNullOrWhiteSpace(searchTerm)) cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

                var count = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(count);
            }
        }

        public async Task<Patient?> GetByUserIdAsync(int userId)
        {
            var query = $@"SELECT p.*, u.FirstName, u.LastName, u.Email, u.Phone, b.BranchName, o.OrganizationName
                           FROM {TableName} p
                           INNER JOIN Users u ON p.UserID = u.UserID
                           LEFT JOIN Branches b ON p.BranchID = b.BranchID
                           LEFT JOIN Organizations o ON p.OrganizationID = o.OrganizationID
                           WHERE p.UserID = @UserID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
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
    }
}
