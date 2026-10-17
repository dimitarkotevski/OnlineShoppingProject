using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NexCart.Business.Security
{
    /// <summary>
    /// The single source of truth for the password strength policy.
    ///
    /// It is enforced by the business layer every time a password is created or changed
    /// (registration, change password and the admin user forms) and is mirrored by the Angular
    /// client (<c>utils/password-policy.ts</c>) so the user gets the same feedback before the
    /// request is even sent.
    /// </summary>
    public static class PasswordPolicy
    {
        /// <summary>Shortest password we accept.</summary>
        public const int MinimumLength = 8;

        /// <summary>Longest password we accept (guards against absurd payloads).</summary>
        public const int MaximumLength = 128;

        private static readonly Regex Lowercase = new Regex("[a-z]", RegexOptions.Compiled);
        private static readonly Regex Uppercase = new Regex("[A-Z]", RegexOptions.Compiled);
        private static readonly Regex Digit = new Regex("[0-9]", RegexOptions.Compiled);
        private static readonly Regex Special = new Regex("[^a-zA-Z0-9]", RegexOptions.Compiled);

        /// <summary>True when the password satisfies every requirement.</summary>
        public static bool IsValid(string password) => Validate(password).Count == 0;

        /// <summary>
        /// Returns one human-readable message for every requirement the password misses, or an
        /// empty list when the password is strong enough. The order matches the client checklist.
        /// </summary>
        public static IReadOnlyList<string> Validate(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return new[] { "Password is required." };
            }

            var errors = new List<string>();

            if (password.Length < MinimumLength)
            {
                errors.Add($"Password must be at least {MinimumLength} characters long.");
            }

            if (password.Length > MaximumLength)
            {
                errors.Add($"Password must be at most {MaximumLength} characters long.");
            }

            if (!Uppercase.IsMatch(password))
            {
                errors.Add("Password must contain at least one uppercase letter.");
            }

            if (!Lowercase.IsMatch(password))
            {
                errors.Add("Password must contain at least one lowercase letter.");
            }

            if (!Digit.IsMatch(password))
            {
                errors.Add("Password must contain at least one number.");
            }

            if (!Special.IsMatch(password))
            {
                errors.Add("Password must contain at least one special character.");
            }

            return errors;
        }
    }
}
