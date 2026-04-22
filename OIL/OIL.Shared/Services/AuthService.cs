using Microsoft.AspNetCore.Components.Authorization;
using Supabase.Gotrue;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Security.Claims;

namespace OIL.Shared.Services
{
    public class AuthService
    {
        private readonly Supabase.Client _supabase;
        private readonly CustomAuthStateProvider _authStateProvider;

        public AuthService(Supabase.Client supabase, AuthenticationStateProvider authStateProvider)
        {
            _supabase = supabase;
            _authStateProvider = (CustomAuthStateProvider)authStateProvider;
        }

        public async Task<bool> Login(string email, string password, bool isAdminMode)
        {
            // --- KEEPING ADMIN LOGIN PART AS IS ---
            if (isAdminMode)
            {
                _authStateProvider.AdminUpdateAuthenticationState(email ?? "admin@oilindia.com", "Admin");
                return true;
            }
            else {
                try
                {
                    // 1. Sign In (ASYNCHRONOUS)
                    var session = await _supabase.Auth.SignIn(email, password);

                    if (session?.User == null) return false;

                    // 2. Query the View (ASYNCHRONOUS)
                    // Ensure you are using .Get() and NOT .Result
                    var response = await _supabase.From<UserPermission>()
                        .Filter("email", Supabase.Postgrest.Constants.Operator.Equals, email)
                        .Order("priority", Supabase.Postgrest.Constants.Ordering.Ascending)
                        .Get();

                    var userPerm = response.Models.FirstOrDefault();

                    Console.WriteLine($"Usr: {userPerm.AppRole}");

                    // 3. Determine Role
                    string assignedRole = userPerm?.AppRole ?? "Engineer";

                    // 4. Update the Provider
                    _authStateProvider.UpdateAuthenticationState(email, assignedRole);

                    return true;

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Login Error: {ex.Message}");
                    return false;
                }

            }
        }

        public async Task Logout()
        {
            await _supabase.Auth.SignOut();
            _authStateProvider.NotifyLogout();
        }
    }

    // FIXED: Must inherit from BaseModel and have Table mapping
    [Table("user_permissions")]
    public class UserPermission : BaseModel
    {
        [Column("email")]
        public string Email { get; set; } = "";

        [Column("designation")]
        public string Designation { get; set; } = "";

        [Column("app_role")]
        public string AppRole { get; set; } = "";
    }
}