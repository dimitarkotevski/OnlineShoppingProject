using System;
using System.Security.Cryptography;
using System.Text;

namespace NexCart.Business.Security
{
    /// <summary>
    /// Salted, adaptive password hashing (PBKDF2-HMAC-SHA256, random per-password salt).
    ///
    /// Stored format: <c>PBKDF2$&lt;iterations&gt;$&lt;saltBase64&gt;$&lt;hashBase64&gt;</c>.
    ///
    /// Passwords written by the old reversible-AES scheme (<see cref="PasswordEncryption"/>) are
    /// still accepted by <see cref="Verify"/> so existing accounts keep working; callers should
    /// check <see cref="NeedsRehash"/> and upgrade the stored value on a successful login.
    /// </summary>
    public static class PasswordHasher
    {
        private const string Prefix = "PBKDF2";
        private const char Separator = '$';
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 210_000;

        public static string Hash(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password), salt, Iterations, HashAlgorithmName.SHA256, HashSize);

            return string.Join(
                Separator.ToString(),
                Prefix,
                Iterations.ToString(),
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }

        /// <summary>True when the stored value is not already a current-generation PBKDF2 hash.</summary>
        public static bool NeedsRehash(string storedHash)
        {
            return string.IsNullOrEmpty(storedHash)
                || !storedHash.StartsWith(Prefix + Separator, StringComparison.Ordinal);
        }

        public static bool Verify(string password, string storedHash)
        {
            if (password == null || string.IsNullOrEmpty(storedHash))
            {
                return false;
            }

            if (storedHash.StartsWith(Prefix + Separator, StringComparison.Ordinal))
            {
                return VerifyPbkdf2(password, storedHash);
            }

            return VerifyLegacy(password, storedHash);
        }

        private static bool VerifyPbkdf2(string password, string storedHash)
        {
            string[] parts = storedHash.Split(Separator);
            if (parts.Length != 4
                || !int.TryParse(parts[1], out int iterations)
                || iterations < 1)
            {
                return false;
            }

            byte[] salt;
            byte[] expected;
            try
            {
                salt = Convert.FromBase64String(parts[2]);
                expected = Convert.FromBase64String(parts[3]);
            }
            catch (FormatException)
            {
                return false;
            }

            // An empty expected hash would make FixedTimeEquals succeed for any password.
            if (expected.Length == 0 || salt.Length == 0)
            {
                return false;
            }

            byte[] actual = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password), salt, iterations, HashAlgorithmName.SHA256, expected.Length);

            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }

        private static bool VerifyLegacy(string password, string storedHash)
        {
            // Legacy records are deterministic AES. Reproduce the old comparison exactly once so
            // the account can be migrated to PBKDF2 on the next successful login.
            string legacy = PasswordEncryption.Encryption(password);
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(legacy),
                Encoding.UTF8.GetBytes(storedHash));
        }
    }
}
