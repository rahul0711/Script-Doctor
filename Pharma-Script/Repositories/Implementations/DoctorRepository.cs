using MySql.Data.MySqlClient;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Implementations
{
    public class DoctorRepository : RepositoryBase<Doctor>, IDoctorRepository
    {
        public DoctorRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "Doctors";
        protected override string PrimaryKeyName => "DoctorID";

        protected override Doctor Map(DbDataReader reader)
        {
            var doctor = new Doctor
            {
                DoctorID = reader.GetInt32(reader.GetOrdinal("DoctorID")),
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                OrganizationID = reader.GetInt32(reader.GetOrdinal("OrganizationID")),
                BranchID = reader.IsDBNull(reader.GetOrdinal("BranchID")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("BranchID")),
                DepartmentID = reader.IsDBNull(reader.GetOrdinal("DepartmentID")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("DepartmentID")),
                Qualification = reader.GetString(reader.GetOrdinal("Qualification")),
                ExperienceYears = reader.GetInt32(reader.GetOrdinal("ExperienceYears")),
                MedicalRegistrationNumber = reader.GetString(reader.GetOrdinal("MedicalRegistrationNumber")),
                Biography = reader.IsDBNull(reader.GetOrdinal("Biography")) ? null : reader.GetString(reader.GetOrdinal("Biography")),
                ConsultationFee = reader.GetDecimal(reader.GetOrdinal("ConsultationFee")),
                VideoConsultationFee = reader.GetDecimal(reader.GetOrdinal("VideoConsultationFee")),
                VoiceConsultationFee = reader.GetDecimal(reader.GetOrdinal("VoiceConsultationFee")),
                PriorityConsultationFee = reader.GetDecimal(reader.GetOrdinal("PriorityConsultationFee")),
                IsPriorityAvailable = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("IsPriorityAvailable"))),
                PriorityStartTime = reader.IsDBNull(reader.GetOrdinal("PriorityStartTime")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("PriorityStartTime")),
                PriorityEndTime = reader.IsDBNull(reader.GetOrdinal("PriorityEndTime")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("PriorityEndTime")),
                IsActive = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("IsActive"))),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                if (name.Equals("FirstName", StringComparison.OrdinalIgnoreCase))
                    doctor.FirstName = reader.GetString(i);
                else if (name.Equals("LastName", StringComparison.OrdinalIgnoreCase))
                    doctor.LastName = reader.IsDBNull(i) ? null : reader.GetString(i);
                else if (name.Equals("Email", StringComparison.OrdinalIgnoreCase))
                    doctor.Email = reader.GetString(i);
                else if (name.Equals("Phone", StringComparison.OrdinalIgnoreCase))
                    doctor.Phone = reader.GetString(i);
                else if (name.Equals("ProfileImage", StringComparison.OrdinalIgnoreCase))
                    doctor.ProfileImage = reader.IsDBNull(i) ? null : reader.GetString(i);
                else if (name.Equals("BranchName", StringComparison.OrdinalIgnoreCase))
                    doctor.BranchName = reader.IsDBNull(i) ? null : reader.GetString(i);
                else if (name.Equals("DepartmentName", StringComparison.OrdinalIgnoreCase))
                    doctor.DepartmentName = reader.IsDBNull(i) ? null : reader.GetString(i);
            }

            return doctor;
        }

        private void AddParameters(MySqlCommand cmd, Doctor entity)
        {
            cmd.Parameters.AddWithValue("@UserID", entity.UserID);
            cmd.Parameters.AddWithValue("@OrganizationID", entity.OrganizationID);
            cmd.Parameters.AddWithValue("@BranchID", (object?)entity.BranchID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DepartmentID", (object?)entity.DepartmentID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Qualification", entity.Qualification);
            cmd.Parameters.AddWithValue("@ExperienceYears", entity.ExperienceYears);
            cmd.Parameters.AddWithValue("@MedicalRegistrationNumber", entity.MedicalRegistrationNumber);
            cmd.Parameters.AddWithValue("@Biography", (object?)entity.Biography ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ConsultationFee", entity.ConsultationFee);
            cmd.Parameters.AddWithValue("@VideoConsultationFee", entity.VideoConsultationFee);
            cmd.Parameters.AddWithValue("@VoiceConsultationFee", entity.VoiceConsultationFee);
            cmd.Parameters.AddWithValue("@PriorityConsultationFee", entity.PriorityConsultationFee);
            cmd.Parameters.AddWithValue("@IsPriorityAvailable", entity.IsPriorityAvailable);
            cmd.Parameters.AddWithValue("@PriorityStartTime", (object?)entity.PriorityStartTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PriorityEndTime", (object?)entity.PriorityEndTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", entity.IsActive);
        }

        public override async Task<int> AddAsync(Doctor entity)
        {
            var query = $@"INSERT INTO {TableName} 
                (UserID, OrganizationID, BranchID, DepartmentID, Qualification, ExperienceYears, MedicalRegistrationNumber, Biography, 
                 ConsultationFee, VideoConsultationFee, VoiceConsultationFee, PriorityConsultationFee, IsPriorityAvailable, PriorityStartTime, PriorityEndTime, IsActive) 
                VALUES 
                (@UserID, @OrganizationID, @BranchID, @DepartmentID, @Qualification, @ExperienceYears, @MedicalRegistrationNumber, @Biography, 
                 @ConsultationFee, @VideoConsultationFee, @VoiceConsultationFee, @PriorityConsultationFee, @IsPriorityAvailable, @PriorityStartTime, @PriorityEndTime, @IsActive);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var id = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
        }

        public override async Task<bool> UpdateAsync(Doctor entity)
        {
            var query = $@"UPDATE {TableName} SET 
                BranchID = @BranchID, 
                DepartmentID = @DepartmentID, 
                Qualification = @Qualification, 
                ExperienceYears = @ExperienceYears, 
                MedicalRegistrationNumber = @MedicalRegistrationNumber, 
                Biography = @Biography, 
                ConsultationFee = @ConsultationFee, 
                VideoConsultationFee = @VideoConsultationFee, 
                VoiceConsultationFee = @VoiceConsultationFee, 
                PriorityConsultationFee = @PriorityConsultationFee, 
                IsPriorityAvailable = @IsPriorityAvailable, 
                PriorityStartTime = @PriorityStartTime, 
                PriorityEndTime = @PriorityEndTime, 
                IsActive = @IsActive
                WHERE DoctorID = @DoctorID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                cmd.Parameters.AddWithValue("@DoctorID", entity.DoctorID);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<bool> UpdateStatusAsync(int id, bool isActive)
        {
            var query = $"UPDATE {TableName} SET IsActive = @IsActive WHERE DoctorID = @DoctorID";
            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@IsActive", isActive);
                cmd.Parameters.AddWithValue("@DoctorID", id);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<Doctor?> GetDoctorDetailsByIdAsync(int id, int? orgId)
        {
            var query = $@"SELECT doc.*, u.FirstName, u.LastName, u.Email, u.Phone, u.ProfileImage, b.BranchName, d.DepartmentName
                           FROM {TableName} doc
                           INNER JOIN Users u ON doc.UserID = u.UserID
                           LEFT JOIN Branches b ON doc.BranchID = b.BranchID
                           LEFT JOIN Departments d ON doc.DepartmentID = d.DepartmentID
                           WHERE doc.DoctorID = @DoctorID";

            if (orgId.HasValue)
            {
                query += " AND doc.OrganizationID = @OrganizationID";
            }

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@DoctorID", id);
                if (orgId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@OrganizationID", orgId.Value);
                }

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var doc = Map(reader);
                        return doc;
                    }
                }
            }
            return null;
        }

        public async Task<IEnumerable<Doctor>> SearchAndPaginateAsync(int? orgId, int? branchId, int? departmentId, int? specializationId, bool? isActive, string searchTerm, int page, int pageSize)
        {
            var offset = (page - 1) * pageSize;
            var list = new List<Doctor>();

            var sb = new StringBuilder();
            sb.Append($@"SELECT DISTINCT doc.*, u.FirstName, u.LastName, u.Email, u.Phone, u.ProfileImage, b.BranchName, d.DepartmentName
                         FROM {TableName} doc
                         INNER JOIN Users u ON doc.UserID = u.UserID
                         LEFT JOIN Branches b ON doc.BranchID = b.BranchID
                         LEFT JOIN Departments d ON doc.DepartmentID = d.DepartmentID");

            if (specializationId.HasValue)
            {
                sb.Append(" INNER JOIN DoctorSpecializations ds ON doc.DoctorID = ds.DoctorID");
            }

            sb.Append(" WHERE 1=1");

            if (orgId.HasValue)
            {
                sb.Append(" AND doc.OrganizationID = @OrganizationID");
            }
            if (branchId.HasValue)
            {
                sb.Append(" AND doc.BranchID = @BranchID");
            }
            if (departmentId.HasValue)
            {
                sb.Append(" AND doc.DepartmentID = @DepartmentID");
            }
            if (specializationId.HasValue)
            {
                sb.Append(" AND ds.SpecializationID = @SpecializationID");
            }
            if (isActive.HasValue)
            {
                sb.Append(" AND doc.IsActive = @IsActive");
            }
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sb.Append(" AND (u.FirstName LIKE @SearchTerm OR u.LastName LIKE @SearchTerm OR u.Email LIKE @SearchTerm OR doc.Qualification LIKE @SearchTerm OR doc.MedicalRegistrationNumber LIKE @SearchTerm)");
            }

            sb.Append(" ORDER BY doc.DoctorID DESC LIMIT @Limit OFFSET @Offset");

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(sb.ToString()))
            {
                if (orgId.HasValue) cmd.Parameters.AddWithValue("@OrganizationID", orgId.Value);
                if (branchId.HasValue) cmd.Parameters.AddWithValue("@BranchID", branchId.Value);
                if (departmentId.HasValue) cmd.Parameters.AddWithValue("@DepartmentID", departmentId.Value);
                if (specializationId.HasValue) cmd.Parameters.AddWithValue("@SpecializationID", specializationId.Value);
                if (isActive.HasValue) cmd.Parameters.AddWithValue("@IsActive", isActive.Value);
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

            // Optimized batch loading of specializations to avoid N+1 query issue
            if (list.Any())
            {
                var docIds = list.Select(d => d.DoctorID).ToList();
                var specMap = await GetSpecializationsForDoctorsAsync(docIds);
                foreach (var doc in list)
                {
                    if (specMap.TryGetValue(doc.DoctorID, out var specs))
                    {
                        doc.SpecializationNames = specs.Select(s => s.Name).ToList();
                        doc.SpecializationIDs = specs.Select(s => s.Id).ToList();
                    }
                }
            }

            return list;
        }

        public async Task<int> GetSearchCountAsync(int? orgId, int? branchId, int? departmentId, int? specializationId, bool? isActive, string searchTerm)
        {
            var sb = new StringBuilder();
            sb.Append(@"SELECT COUNT(DISTINCT doc.DoctorID)
                        FROM Doctors doc
                        INNER JOIN Users u ON doc.UserID = u.UserID");

            if (specializationId.HasValue)
            {
                sb.Append(" INNER JOIN DoctorSpecializations ds ON doc.DoctorID = ds.DoctorID");
            }

            sb.Append(" WHERE 1=1");

            if (orgId.HasValue)
            {
                sb.Append(" AND doc.OrganizationID = @OrganizationID");
            }
            if (branchId.HasValue)
            {
                sb.Append(" AND doc.BranchID = @BranchID");
            }
            if (departmentId.HasValue)
            {
                sb.Append(" AND doc.DepartmentID = @DepartmentID");
            }
            if (specializationId.HasValue)
            {
                sb.Append(" AND ds.SpecializationID = @SpecializationID");
            }
            if (isActive.HasValue)
            {
                sb.Append(" AND doc.IsActive = @IsActive");
            }
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sb.Append(" AND (u.FirstName LIKE @SearchTerm OR u.LastName LIKE @SearchTerm OR u.Email LIKE @SearchTerm OR doc.Qualification LIKE @SearchTerm OR doc.MedicalRegistrationNumber LIKE @SearchTerm)");
            }

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(sb.ToString()))
            {
                if (orgId.HasValue) cmd.Parameters.AddWithValue("@OrganizationID", orgId.Value);
                if (branchId.HasValue) cmd.Parameters.AddWithValue("@BranchID", branchId.Value);
                if (departmentId.HasValue) cmd.Parameters.AddWithValue("@DepartmentID", departmentId.Value);
                if (specializationId.HasValue) cmd.Parameters.AddWithValue("@SpecializationID", specializationId.Value);
                if (isActive.HasValue) cmd.Parameters.AddWithValue("@IsActive", isActive.Value);
                if (!string.IsNullOrWhiteSpace(searchTerm)) cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

                var count = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(count);
            }
        }

        private async Task<Dictionary<int, List<(int Id, string Name)>>> GetSpecializationsForDoctorsAsync(List<int> doctorIds)
        {
            var dict = new Dictionary<int, List<(int Id, string Name)>>();
            if (!doctorIds.Any()) return dict;

            var idList = string.Join(",", doctorIds);
            var query = $@"SELECT ds.DoctorID, s.SpecializationID, s.SpecializationName
                           FROM DoctorSpecializations ds
                           INNER JOIN Specializations s ON ds.SpecializationID = s.SpecializationID
                           WHERE ds.DoctorID IN ({idList})";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var docId = reader.GetInt32(0);
                    var specId = reader.GetInt32(1);
                    var specName = reader.GetString(2);

                    if (!dict.ContainsKey(docId))
                    {
                        dict[docId] = new List<(int, string)>();
                    }
                    dict[docId].Add((specId, specName));
                }
            }
            return dict;
        }

        public async Task<IEnumerable<int>> GetDoctorSpecializationIDsAsync(int doctorId)
        {
            var list = new List<int>();
            var query = "SELECT SpecializationID FROM DoctorSpecializations WHERE DoctorID = @DoctorID";
            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@DoctorID", doctorId);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(reader.GetInt32(0));
                    }
                }
            }
            return list;
        }

        public async Task<bool> ClearDoctorSpecializationsAsync(int doctorId)
        {
            var query = "DELETE FROM DoctorSpecializations WHERE DoctorID = @DoctorID";
            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@DoctorID", doctorId);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows >= 0;
            }
        }

        public async Task<bool> AddDoctorSpecializationsAsync(int doctorId, List<int> specializationIds)
        {
            if (specializationIds == null || !specializationIds.Any()) return true;

            // Direct mapping insert to prevent duplicate mapping
            var sb = new StringBuilder();
            sb.Append("INSERT IGNORE INTO DoctorSpecializations (DoctorID, SpecializationID) VALUES ");
            for (int i = 0; i < specializationIds.Count; i++)
            {
                sb.Append($"(@DoctorID, @Spec{i})");
                if (i < specializationIds.Count - 1) sb.Append(", ");
            }

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(sb.ToString()))
            {
                cmd.Parameters.AddWithValue("@DoctorID", doctorId);
                for (int i = 0; i < specializationIds.Count; i++)
                {
                    cmd.Parameters.AddWithValue($"@Spec{i}", specializationIds[i]);
                }
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<Doctor?> GetByUserIdAsync(int userId)
        {
            var query = $@"SELECT d.*, u.FirstName, u.LastName, u.Email, u.Phone, u.ProfileImage, b.BranchName, dept.DepartmentName
                           FROM {TableName} d
                           INNER JOIN Users u ON d.UserID = u.UserID
                           LEFT JOIN Branches b ON d.BranchID = b.BranchID
                           LEFT JOIN Departments dept ON d.DepartmentID = dept.DepartmentID
                           WHERE d.UserID = @UserID";

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
