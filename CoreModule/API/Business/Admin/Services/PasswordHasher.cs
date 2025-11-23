using Microsoft.AspNetCore.Identity;

namespace BusinessLogic.Admin.Services
{
    /// <summary>
    /// Secure password hasher using ASP.NET Core Identity's PasswordHasher
    /// which uses PBKDF2 with HMAC-SHA256, 128-bit salt, 256-bit subkey, 10000 iterations
    /// </summary>
    public static class PasswordHasher
    {
        private static readonly PasswordHasher<object> _hasher = new PasswordHasher<object>();

        /// <summary>
        /// Hashes a password using PBKDF2 with secure defaults
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be null or empty", nameof(password));
            }

            return _hasher.HashPassword(null!, password);
        }

        /// <summary>
        /// Verifies a password against a hashed password
        /// </summary>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
            {
                return false;
            }

            var result = _hasher.VerifyHashedPassword(null!, hashedPassword, password);
            return result == PasswordVerificationResult.Success || 
                   result == PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}