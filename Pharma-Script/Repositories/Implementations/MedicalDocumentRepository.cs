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
    public class MedicalDocumentRepository : RepositoryBase<MedicalDocument>, IMedicalDocumentRepository
    {
        public MedicalDocumentRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "MedicalDocuments";
        protected override string PrimaryKeyName => "DocumentID";

        protected override MedicalDocument Map(DbDataReader reader)
        {
            var doc = new MedicalDocument
            {
                DocumentID = reader.GetInt32(reader.GetOrdinal("DocumentID")),
                PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                OrganizationID = reader.GetInt32(reader.GetOrdinal("OrganizationID")),
                UploadedByUserID = reader.GetInt32(reader.GetOrdinal("UploadedByUserID")),
                DocumentTitle = reader.GetString(reader.GetOrdinal("DocumentTitle")),
                DocumentType = reader.GetString(reader.GetOrdinal("DocumentType")),
                FileName = reader.GetString(reader.GetOrdinal("FileName")),
                FilePath = reader.GetString(reader.GetOrdinal("FilePath")),
                FileSize = reader.IsDBNull(reader.GetOrdinal("FileSize")) ? null : (long?)reader.GetInt64(reader.GetOrdinal("FileSize")),
                UploadDate = reader.GetDateTime(reader.GetOrdinal("UploadDate"))
            };

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                if (name.Equals("UploadedByUserName", StringComparison.OrdinalIgnoreCase))
                {
                    doc.UploadedByUserName = reader.IsDBNull(i) ? null : reader.GetString(i);
                }
            }

            return doc;
        }

        private void AddParameters(MySqlCommand cmd, MedicalDocument entity)
        {
            cmd.Parameters.AddWithValue("@PatientID", entity.PatientID);
            cmd.Parameters.AddWithValue("@OrganizationID", entity.OrganizationID);
            cmd.Parameters.AddWithValue("@UploadedByUserID", entity.UploadedByUserID);
            cmd.Parameters.AddWithValue("@DocumentTitle", entity.DocumentTitle);
            cmd.Parameters.AddWithValue("@DocumentType", entity.DocumentType);
            cmd.Parameters.AddWithValue("@FileName", entity.FileName);
            cmd.Parameters.AddWithValue("@FilePath", entity.FilePath);
            cmd.Parameters.AddWithValue("@FileSize", (object?)entity.FileSize ?? DBNull.Value);
        }

        public override async Task<int> AddAsync(MedicalDocument entity)
        {
            var query = $@"INSERT INTO {TableName} 
                (PatientID, OrganizationID, UploadedByUserID, DocumentTitle, DocumentType, FileName, FilePath, FileSize) 
                VALUES 
                (@PatientID, @OrganizationID, @UploadedByUserID, @DocumentTitle, @DocumentType, @FileName, @FilePath, @FileSize);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var id = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
        }

        public override async Task<bool> UpdateAsync(MedicalDocument entity)
        {
            var query = $@"UPDATE {TableName} SET 
                DocumentTitle = @DocumentTitle, 
                DocumentType = @DocumentType, 
                FileName = @FileName, 
                FilePath = @FilePath, 
                FileSize = @FileSize
                WHERE DocumentID = @DocumentID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                cmd.Parameters.AddWithValue("@DocumentID", entity.DocumentID);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<IEnumerable<MedicalDocument>> GetByPatientIdAsync(int patientId, int? orgId)
        {
            var list = new List<MedicalDocument>();
            var query = $@"SELECT d.*, CONCAT(u.FirstName, ' ', IFNULL(u.LastName, '')) AS UploadedByUserName
                           FROM {TableName} d
                           INNER JOIN Users u ON d.UploadedByUserID = u.UserID
                           WHERE d.PatientID = @PatientID";

            if (orgId.HasValue)
            {
                query += " AND d.OrganizationID = @OrganizationID";
            }

            query += " ORDER BY d.UploadDate DESC";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@PatientID", patientId);
                if (orgId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@OrganizationID", orgId.Value);
                }

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
    }
}
