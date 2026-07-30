using System;
using System.Security.Cryptography;

namespace MythicForge.Services
{
    /// <summary>
    /// Salted PBKDF2 password hashing using the built-in .NET crypto APIs, so the
    /// sample stores no plaintext passwords and needs no third-party packages.
    /// Stored format is Base64(16-byte salt + 32-byte hash).
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        // PBKDF2-SHA256 with a modern iteration count (OWASP-aligned) rather than the
        // legacy SHA1 default and low iteration count.
        private const int Iterations = 210000;
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        public static string Hash(string password)
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, Algorithm))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);
                byte[] combined = new byte[SaltSize + HashSize];
                Buffer.BlockCopy(salt, 0, combined, 0, SaltSize);
                Buffer.BlockCopy(hash, 0, combined, SaltSize, HashSize);
                return Convert.ToBase64String(combined);
            }
        }

        public static bool Verify(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash))
            {
                return false;
            }

            byte[] combined;
            try
            {
                combined = Convert.FromBase64String(storedHash);
            }
            catch (FormatException)
            {
                return false;
            }

            if (combined.Length != SaltSize + HashSize)
            {
                return false;
            }

            byte[] salt = new byte[SaltSize];
            Buffer.BlockCopy(combined, 0, salt, 0, SaltSize);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, Algorithm))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);
                int diff = 0;
                for (int i = 0; i < HashSize; i++)
                {
                    diff |= combined[SaltSize + i] ^ hash[i];
                }

                return diff == 0;
            }
        }
    }
}
