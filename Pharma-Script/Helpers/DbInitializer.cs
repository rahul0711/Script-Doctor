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
