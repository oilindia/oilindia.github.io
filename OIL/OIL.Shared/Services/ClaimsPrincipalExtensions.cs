using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace OIL.Shared.Services
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetEmail(this ClaimsPrincipal user)
            => user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        public static string GetFullName(this ClaimsPrincipal user)
            => user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

        public static string GetRole(this ClaimsPrincipal user)
            => user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        public static string GetDesignation(this ClaimsPrincipal user)
            => user.FindFirst("Designation")?.Value ?? string.Empty;

        public static string GetGrade(this ClaimsPrincipal user)
            => user.FindFirst("Grade")?.Value ?? string.Empty;

      
    }
}
