using MySql.Data.MySqlClient;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pharma_Script.Helpers
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IUnitOfWork uow)
        {
            await uow.OpenConnectionAsync();

            // 1. Check if tables exist in the database
            bool tablesExist = false;
            using (var conn = new MySqlConnection("server=120.138.7.130;uid=scriptindia;pwd=India@4321;database=ScriptIndia_Healthcare;"))
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SHOW TABLES;";
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            tablesExist = true;
                        }
                    }
                }

                // 2. If no tables exist, run the SQL script to create them
                if (!tablesExist)
                {
                    string projectPath = Directory.GetCurrentDirectory();
                    string sqlFilePath = Path.Combine(projectPath, "TEST", "ScriptIndia_Healthcare.sql");

                    if (File.Exists(sqlFilePath))
                    {
                        var sql = await File.ReadAllTextAsync(sqlFilePath);
                        
                        // Split script into individual commands by semicolon
                        var commands = sql.Split(';')
                            .Select(c => c.Trim())
                            .Where(c => !string.IsNullOrEmpty(c));

                        foreach (var command in commands)
                        {
                            if (command.StartsWith("create database", StringComparison.OrdinalIgnoreCase))
                            {
                                continue; // Skip database creation command since it is already created
                            }

                            try
                            {
                                using (var cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = command;
                                    await cmd.ExecuteNonQueryAsync();
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error executing SQL: {command.Substring(0, Math.Min(50, command.Length))}... Error: {ex.Message}");
                            }
                        }
                    }
                }
            }

            // 2b. Ensure Notifications table exists (safe to run every startup)
            using (var conn = new MySqlConnection("server=120.138.7.130;uid=scriptindia;pwd=India@4321;database=ScriptIndia_Healthcare;"))
            {
                await conn.OpenAsync();
                const string createNotifications = @"
                    CREATE TABLE IF NOT EXISTS Notifications (
                        NotificationID   INT           NOT NULL AUTO_INCREMENT PRIMARY KEY,
                        OrganizationID   INT           NOT NULL,
                        UserID           INT           NOT NULL,
                        NotificationType VARCHAR(100)  NOT NULL,
                        Title            VARCHAR(255)  NOT NULL,
                        Message          TEXT          NOT NULL,
                        RelatedEntityType VARCHAR(100) NULL,
                        RelatedEntityID  INT           NULL,
                        IsRead           TINYINT(1)   NOT NULL DEFAULT 0,
                        CreatedAt        DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        INDEX idx_notif_user   (UserID, OrganizationID, IsRead),
                        INDEX idx_notif_entity (RelatedEntityType, RelatedEntityID)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = createNotifications;
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // 2bb. Ensure ConsultationSessions table exists (safe to run every startup)
            using (var conn = new MySqlConnection("server=120.138.7.130;uid=scriptindia;pwd=India@4321;database=ScriptIndia_Healthcare;"))
            {
                await conn.OpenAsync();
                const string createConsultationSessions = @"
                    CREATE TABLE IF NOT EXISTS ConsultationSessions (
                        ConsultationSessionID INT           NOT NULL AUTO_INCREMENT PRIMARY KEY,
                        OrganizationID        INT           NOT NULL,
                        AppointmentID         INT           NOT NULL,
                        DoctorID              INT           NOT NULL,
                        PatientID             INT           NOT NULL,
                        ConsultationType      VARCHAR(20)   NOT NULL DEFAULT 'Video',
                        MeetingProvider       VARCHAR(50)   NULL,
                        MeetingURL            VARCHAR(500)  NULL,
                        SessionStatus         VARCHAR(20)   NOT NULL DEFAULT 'Pending',
                        CreatedByUserID       INT           NOT NULL,
                        CreatedAt             DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        UpdatedAt             DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                        FOREIGN KEY (AppointmentID) REFERENCES Appointments(AppointmentID) ON DELETE CASCADE,
                        FOREIGN KEY (DoctorID) REFERENCES Doctors(DoctorID),
                        FOREIGN KEY (PatientID) REFERENCES Patients(PatientID)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = createConsultationSessions;
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // 2c. Ensure OrganizationID column exists in Prescriptions, Payments, FollowUps, DoctorNotes, and ConsultationSessions
            using (var conn = new MySqlConnection("server=120.138.7.130;uid=scriptindia;pwd=India@4321;database=ScriptIndia_Healthcare;"))
            {
                await conn.OpenAsync();
                string[] tablesToCheck = { "Prescriptions", "Payments", "FollowUps", "DoctorNotes", "ConsultationSessions" };
                foreach (var table in tablesToCheck)
                {
                    bool columnExists = false;
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $@"
                            SELECT COUNT(*) 
                            FROM information_schema.COLUMNS 
                            WHERE TABLE_SCHEMA = 'ScriptIndia_Healthcare' 
                              AND TABLE_NAME = '{table}' 
                              AND COLUMN_NAME = 'OrganizationID';";
                        columnExists = Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
                    }

                    if (!columnExists)
                    {
                        try
                        {
                            // 1. Add column as NULL first to allow table update with existing rows
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = $"ALTER TABLE `{table}` ADD COLUMN OrganizationID INT NULL;";
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // 2. Populate OrganizationID from linked Appointments table
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = $@"
                                    UPDATE `{table}` t 
                                    INNER JOIN Appointments a ON t.AppointmentID = a.AppointmentID 
                                    SET t.OrganizationID = a.OrganizationID;";
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // 3. Make column NOT NULL to match code expectations
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = $"ALTER TABLE `{table}` MODIFY COLUMN OrganizationID INT NOT NULL;";
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[DbInitializer] Failed to add OrganizationID to {table}: {ex.Message}");
                        }
                    }
                }
            }

            // 3. Seed Roles if missing
            var existingRoles = await uow.Roles.GetAllAsync();
            var requiredRoles = new[] { "Platform Owner", "Organization Admin", "Doctor", "Patient", "Receptionist" };

            foreach (var roleName in requiredRoles)
            {
                if (!existingRoles.Any(r => r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase)))
                {
                    await uow.Roles.AddAsync(new Role
                    {
                        RoleName = roleName,
                        Description = $"System defined {roleName} role"
                    });
                }
            }

            // Refresh roles list to get updated IDs
            existingRoles = await uow.Roles.GetAllAsync();

            // 4. Seed a default Platform Owner user if none exists
            var platformOwnerRole = existingRoles.FirstOrDefault(r => r.RoleName.Equals("Platform Owner", StringComparison.OrdinalIgnoreCase));
            if (platformOwnerRole != null)
            {
                var existingUsers = await uow.Users.GetAllAsync();
                var hasOwner = existingUsers.Any(u => u.RoleID == platformOwnerRole.RoleID || u.Email.Equals("admin@scriptindia.com", StringComparison.OrdinalIgnoreCase));
                
                if (!hasOwner)
                {
                    await uow.Users.AddAsync(new User
                    {
                        OrganizationID = null,
                        RoleID = platformOwnerRole.RoleID,
                        FirstName = "Platform",
                        LastName = "Owner",
                        Email = "admin@scriptindia.com",
                        Phone = "9999999999",
                        PasswordHash = PasswordHasher.HashPassword("Password@123"),
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    });
                }
            }
        }
    }
}
