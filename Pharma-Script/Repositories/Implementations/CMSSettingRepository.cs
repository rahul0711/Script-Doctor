using MySql.Data.MySqlClient;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Implementations
{
    public class CMSSettingRepository : RepositoryBase<CMSSetting>, ICMSSettingRepository
    {
        public CMSSettingRepository(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
            : base(connection, transactionProvider)
        {
        }

        protected override string TableName => "CMSSettings";
        protected override string PrimaryKeyName => "CMSSettingID";

        protected override CMSSetting Map(DbDataReader reader)
        {
            return new CMSSetting
            {
                CMSSettingID = reader.GetInt32(reader.GetOrdinal("CMSSettingID")),
                OrganizationID = reader.GetInt32(reader.GetOrdinal("OrganizationID")),
                WebsiteTitle = reader.GetString(reader.GetOrdinal("WebsiteTitle")),
                WebsiteLogo = reader.IsDBNull(reader.GetOrdinal("WebsiteLogo")) ? null : reader.GetString(reader.GetOrdinal("WebsiteLogo")),
                Favicon = reader.IsDBNull(reader.GetOrdinal("Favicon")) ? null : reader.GetString(reader.GetOrdinal("Favicon")),
                PrimaryColor = reader.IsDBNull(reader.GetOrdinal("PrimaryColor")) ? null : reader.GetString(reader.GetOrdinal("PrimaryColor")),
                SecondaryColor = reader.IsDBNull(reader.GetOrdinal("SecondaryColor")) ? null : reader.GetString(reader.GetOrdinal("SecondaryColor")),
                AboutUs = reader.IsDBNull(reader.GetOrdinal("AboutUs")) ? null : reader.GetString(reader.GetOrdinal("AboutUs")),
                Mission = reader.IsDBNull(reader.GetOrdinal("Mission")) ? null : reader.GetString(reader.GetOrdinal("Mission")),
                Vision = reader.IsDBNull(reader.GetOrdinal("Vision")) ? null : reader.GetString(reader.GetOrdinal("Vision")),
                ContactEmail = reader.IsDBNull(reader.GetOrdinal("ContactEmail")) ? null : reader.GetString(reader.GetOrdinal("ContactEmail")),
                ContactPhone = reader.IsDBNull(reader.GetOrdinal("ContactPhone")) ? null : reader.GetString(reader.GetOrdinal("ContactPhone")),
                EmergencyPhone = reader.IsDBNull(reader.GetOrdinal("EmergencyPhone")) ? null : reader.GetString(reader.GetOrdinal("EmergencyPhone")),
                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                GoogleMapEmbed = reader.IsDBNull(reader.GetOrdinal("GoogleMapEmbed")) ? null : reader.GetString(reader.GetOrdinal("GoogleMapEmbed")),
                FacebookURL = reader.IsDBNull(reader.GetOrdinal("FacebookURL")) ? null : reader.GetString(reader.GetOrdinal("FacebookURL")),
                InstagramURL = reader.IsDBNull(reader.GetOrdinal("InstagramURL")) ? null : reader.GetString(reader.GetOrdinal("InstagramURL")),
                LinkedInURL = reader.IsDBNull(reader.GetOrdinal("LinkedInURL")) ? null : reader.GetString(reader.GetOrdinal("LinkedInURL")),
                TwitterURL = reader.IsDBNull(reader.GetOrdinal("TwitterURL")) ? null : reader.GetString(reader.GetOrdinal("TwitterURL")),
                YouTubeURL = reader.IsDBNull(reader.GetOrdinal("YouTubeURL")) ? null : reader.GetString(reader.GetOrdinal("YouTubeURL")),
                FooterText = reader.IsDBNull(reader.GetOrdinal("FooterText")) ? null : reader.GetString(reader.GetOrdinal("FooterText")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }

        private void AddParameters(MySqlCommand cmd, CMSSetting entity)
        {
            cmd.Parameters.AddWithValue("@OrganizationID", entity.OrganizationID);
            cmd.Parameters.AddWithValue("@WebsiteTitle", entity.WebsiteTitle);
            cmd.Parameters.AddWithValue("@WebsiteLogo", (object?)entity.WebsiteLogo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Favicon", (object?)entity.Favicon ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PrimaryColor", (object?)entity.PrimaryColor ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SecondaryColor", (object?)entity.SecondaryColor ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AboutUs", (object?)entity.AboutUs ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Mission", (object?)entity.Mission ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Vision", (object?)entity.Vision ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ContactEmail", (object?)entity.ContactEmail ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ContactPhone", (object?)entity.ContactPhone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EmergencyPhone", (object?)entity.EmergencyPhone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object?)entity.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GoogleMapEmbed", (object?)entity.GoogleMapEmbed ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FacebookURL", (object?)entity.FacebookURL ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@InstagramURL", (object?)entity.InstagramURL ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LinkedInURL", (object?)entity.LinkedInURL ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TwitterURL", (object?)entity.TwitterURL ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@YouTubeURL", (object?)entity.YouTubeURL ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FooterText", (object?)entity.FooterText ?? DBNull.Value);
        }

        public override async Task<int> AddAsync(CMSSetting entity)
        {
            var query = $@"INSERT INTO {TableName}
                (OrganizationID, WebsiteTitle, WebsiteLogo, Favicon, PrimaryColor, SecondaryColor, AboutUs, Mission, Vision,
                 ContactEmail, ContactPhone, EmergencyPhone, Address, GoogleMapEmbed, FacebookURL, InstagramURL, LinkedInURL,
                 TwitterURL, YouTubeURL, FooterText)
                VALUES
                (@OrganizationID, @WebsiteTitle, @WebsiteLogo, @Favicon, @PrimaryColor, @SecondaryColor, @AboutUs, @Mission, @Vision,
                 @ContactEmail, @ContactPhone, @EmergencyPhone, @Address, @GoogleMapEmbed, @FacebookURL, @InstagramURL, @LinkedInURL,
                 @TwitterURL, @YouTubeURL, @FooterText);
                SELECT LAST_INSERT_ID();";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                var id = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
        }

        public override async Task<bool> UpdateAsync(CMSSetting entity)
        {
            var query = $@"UPDATE {TableName} SET
                WebsiteTitle = @WebsiteTitle, WebsiteLogo = @WebsiteLogo, Favicon = @Favicon,
                PrimaryColor = @PrimaryColor, SecondaryColor = @SecondaryColor, AboutUs = @AboutUs,
                Mission = @Mission, Vision = @Vision, ContactEmail = @ContactEmail, ContactPhone = @ContactPhone,
                EmergencyPhone = @EmergencyPhone, Address = @Address, GoogleMapEmbed = @GoogleMapEmbed,
                FacebookURL = @FacebookURL, InstagramURL = @InstagramURL, LinkedInURL = @LinkedInURL,
                TwitterURL = @TwitterURL, YouTubeURL = @YouTubeURL, FooterText = @FooterText
                WHERE CMSSettingID = @CMSSettingID AND OrganizationID = @OrganizationID";

            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                AddParameters(cmd, entity);
                cmd.Parameters.AddWithValue("@CMSSettingID", entity.CMSSettingID);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<CMSSetting?> GetByOrganizationIdAsync(int organizationId)
        {
            var query = $"SELECT * FROM {TableName} WHERE OrganizationID = @OrganizationID LIMIT 1";
            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@OrganizationID", organizationId);
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

        public async Task<int> UpsertAsync(CMSSetting entity)
        {
            var existing = await GetByOrganizationIdAsync(entity.OrganizationID);
            if (existing == null)
            {
                return await AddAsync(entity);
            }

            entity.CMSSettingID = existing.CMSSettingID;
            await UpdateAsync(entity);
            return existing.CMSSettingID;
        }
    }
}
