using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Pharma_Script.Helpers
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            // Generate a 128-bit salt using a cryptographically strong random sequence
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Derive a 256-bit subkey (use HMAC-SHA256 with 100,000 iterations)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            // Return the salt and hash concatenated with a dot delimiter
            return $"{Convert.ToBase64String(salt)}.{hashed}";
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                var parts = hashedPassword.Split('.');
                if (parts.Length != 2) return false;

                var salt = Convert.FromBase64String(parts[0]);
                var storedHash = parts[1];

                // Derive the key again using the same parameters and the stored salt
                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 256 / 8));

                return CryptographicOperations.FixedTimeEquals(
                    Convert.FromBase64String(hashed), 
                    Convert.FromBase64String(storedHash)
                );
            }
            catch
            {
                return false;
            }
        }
    }
}
