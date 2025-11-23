using BusinessLogic.Admin.Services;
using Microsoft.AspNetCore.Mvc;

namespace encryptzERP.Controllers.Admin
{
    /// <summary>
    /// TEMPORARY CONTROLLER - Remove after fixing password hashes in database
    /// This controller helps generate password hashes for migration purposes
    /// DO NOT deploy this to production!
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordHashController : ControllerBase
    {
        /// <summary>
        /// Generate a password hash for a given password
        /// SECURITY WARNING: This endpoint should be removed after database migration
        /// </summary>
        /// <param name="request">Password to hash</param>
        /// <returns>Hashed password in PBKDF2 format</returns>
        [HttpPost("generate")]
        public IActionResult GenerateHash([FromBody] PasswordHashRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { error = "Password is required" });
            }

            try
            {
                var hash = PasswordHasher.HashPassword(request.Password);
                var isValid = PasswordHasher.VerifyPassword(request.Password, hash);

                return Ok(new
                {
                    password = request.Password,
                    hash = hash,
                    hashLength = hash.Length,
                    verificationTest = isValid ? "PASSED" : "FAILED",
                    format = "PBKDF2 (ASP.NET Core Identity)",
                    warning = "This hash is for database migration. Remove this endpoint before production deployment!"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Verify a password against a hash
        /// </summary>
        [HttpPost("verify")]
        public IActionResult VerifyHash([FromBody] PasswordVerifyRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Hash))
            {
                return BadRequest(new { error = "Both password and hash are required" });
            }

            try
            {
                var isValid = PasswordHasher.VerifyPassword(request.Password, request.Hash);
                return Ok(new
                {
                    isValid = isValid,
                    result = isValid ? "✓ Password matches hash" : "✗ Password does not match hash"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    isValid = false,
                    error = ex.Message,
                    hint = "The hash format may be invalid. Expected PBKDF2 format from ASP.NET Core Identity."
                });
            }
        }

        /// <summary>
        /// Generate hashes for multiple common passwords
        /// </summary>
        [HttpGet("batch")]
        public IActionResult GenerateBatch()
        {
            var passwords = new[] { "Admin@123", "Password@123", "Test@123", "User@123" };
            var results = new List<object>();

            foreach (var password in passwords)
            {
                var hash = PasswordHasher.HashPassword(password);
                results.Add(new
                {
                    password = password,
                    hash = hash
                });
            }

            return Ok(new
            {
                hashes = results,
                warning = "These hashes are for development/testing only. Remove this endpoint before production!"
            });
        }
    }

    public class PasswordHashRequest
    {
        public string Password { get; set; } = string.Empty;
    }

    public class PasswordVerifyRequest
    {
        public string Password { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
    }
}

