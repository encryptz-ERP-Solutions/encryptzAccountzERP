using Microsoft.AspNetCore.Identity;
using System;

namespace BusinessLogic.Admin.Services
{
    /// <summary>
    /// Utility to generate and test password hashes
    /// Can be run as a standalone console app or used in migrations
    /// </summary>
    public class PasswordUtility
    {
        /// <summary>
        /// Generate a password hash for a given plain text password
        /// </summary>
        public static string GenerateHash(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be null or empty", nameof(password));
            }

            return PasswordHasher.HashPassword(password);
        }

        /// <summary>
        /// Verify if a password matches a hash
        /// </summary>
        public static bool VerifyHash(string password, string hash)
        {
            return PasswordHasher.VerifyPassword(password, hash);
        }

        /// <summary>
        /// Test method to generate hashes for common passwords
        /// Useful for creating seed data or testing
        /// </summary>
        public static void GenerateCommonHashes()
        {
            var passwords = new[]
            {
                "Admin@123",
                "Password@123",
                "Test@123",
                "User@123"
            };

            Console.WriteLine("=".PadRight(80, '='));
            Console.WriteLine("Password Hash Generator");
            Console.WriteLine("=".PadRight(80, '='));
            Console.WriteLine();

            foreach (var password in passwords)
            {
                var hash = GenerateHash(password);
                Console.WriteLine($"Password: {password}");
                Console.WriteLine($"Hash:     {hash}");
                Console.WriteLine();

                // Verify the hash works
                var isValid = VerifyHash(password, hash);
                Console.WriteLine($"Verification: {(isValid ? "✓ SUCCESS" : "✗ FAILED")}");
                Console.WriteLine(new string('-', 80));
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Main method for running as standalone utility
        /// </summary>
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("  PasswordUtility generate <password>  - Generate hash for a password");
                Console.WriteLine("  PasswordUtility verify <password> <hash>  - Verify a password against a hash");
                Console.WriteLine("  PasswordUtility batch  - Generate hashes for common passwords");
                Console.WriteLine();
                Console.WriteLine("Running batch mode by default...");
                Console.WriteLine();
                GenerateCommonHashes();
                return;
            }

            var command = args[0].ToLower();

            switch (command)
            {
                case "generate":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Error: Please provide a password to hash");
                        return;
                    }
                    var hash = GenerateHash(args[1]);
                    Console.WriteLine($"Generated Hash: {hash}");
                    break;

                case "verify":
                    if (args.Length < 3)
                    {
                        Console.WriteLine("Error: Please provide both password and hash");
                        return;
                    }
                    var isValid = VerifyHash(args[1], args[2]);
                    Console.WriteLine($"Verification: {(isValid ? "✓ VALID" : "✗ INVALID")}");
                    break;

                case "batch":
                    GenerateCommonHashes();
                    break;

                default:
                    Console.WriteLine($"Unknown command: {command}");
                    break;
            }
        }
    }
}

