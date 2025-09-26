using System.Security.Cryptography;
using System.Text;

namespace BusinessLogic.Admin.Services
{
    public static class PasswordHasher
    {
        // This is a simple placeholder implementation.
        // For a real application, use a strong, well-vetted library like BCrypt.Net or ASP.NET Core Identity's password hasher.
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return "sha256_" + BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (!hashedPassword.StartsWith("sha256_"))
            {
                // For this simple example, we only support our own hash format.
                // A real implementation would handle legacy formats or different algorithms.
                return false;
            }

            var expectedHash = HashPassword(password);
            return expectedHash == hashedPassword;
        }
    }
}