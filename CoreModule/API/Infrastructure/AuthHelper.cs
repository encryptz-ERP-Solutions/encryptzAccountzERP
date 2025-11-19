using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Infrastructure
{
    /// <summary>
    /// Helper for extracting current user ID from JWT claims
    /// </summary>
    public static class AuthHelper
    {
        /// <summary>
        /// Gets the current user ID from HttpContext claims.
        /// Checks "user_id" first, then "sub" claim.
        /// Returns null if neither is found or if user is not authenticated.
        /// </summary>
        public static Guid? GetCurrentUserId(HttpContext httpContext)
        {
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var claims = httpContext.User;

            // Prefer "user_id" claim, fallback to "sub"
            var userIdClaim = claims.FindFirst("user_id") ?? claims.FindFirst("sub");
            
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }

        /// <summary>
        /// Gets the current user ID from ClaimsPrincipal.
        /// </summary>
        public static Guid? GetCurrentUserId(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var userIdClaim = user.FindFirst("user_id") ?? user.FindFirst("sub");
            
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }
    }
}

