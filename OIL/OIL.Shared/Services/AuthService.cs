using Google.GenAI.Types;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;
using MudBlazor;
using Supabase;
using Supabase.Gotrue;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Http.Json;
using System.Security.Claims;
using static OIL.Shared.Services.CustomAuthStateProvider;
using static System.Net.WebRequestMethods;


namespace OIL.Shared.Services
{
    public class AuthService
    {
        private readonly Supabase.Client _supabase;
        private readonly CustomAuthStateProvider _authStateProvider;
        private readonly HttpClient _http; // Add this

        public AuthService(Supabase.Client supabase, AuthenticationStateProvider authStateProvider, HttpClient http)
        {
            _supabase = supabase;
            _authStateProvider = (CustomAuthStateProvider)authStateProvider;
            _http = http; // Add this
        }

        public async Task<bool> Login(string loginId, string password, bool isAdminMode)
        {
            try
            {
                if (isAdminMode)
                {
                    var adminSession = new CustomAuthStateProvider.UserSession { Email = loginId, Role = "Admin" };
                    await _authStateProvider.UpdateStateAsync(adminSession);
                    return true;
                }

                // Branch 1: Executive Login
                if (loginId.Contains("@"))
                {
                    var session = await _supabase.Auth.SignIn(loginId, password);
                    if (session?.AccessToken != null)
                    {
                        var response = await _supabase.From<EmployeeExecutive>().Where(x => x.Email == loginId).Single();
                        var role = response?.IsLocalAdmin == true ? "Admin" : "Executive";

                        var userSession = new CustomAuthStateProvider.UserSession
                        {
                            Email = loginId,
                            Role = role,
                            FullName = response?.FullName ?? loginId,
                            Designation = response?.Designation ?? "",
                            Grade = "Executive"
                        };

                        GlobalVariables.GlobalCurrentUserID = session.User?.Id?.ToString();
                        GlobalVariables.GlobalCurrentUserRole = role;
                        await _authStateProvider.UpdateStateAsync(userSession);
                        return true;
                    }
                }
                // Branch 2: Engineer Login
                else
                {
                    var response = await _supabase.From<EmployeeEngineer>().Where(x => x.EmpCode == loginId).Single();

                    if (response != null && response.PersonalCode == password)
                    {
                        // 1. Get the JWT from your Edge Function
                        // Replace with your actual project URL
                        var jwtResponse = await _http.PostAsJsonAsync("https://pmwutokmedbbphpwxafo.supabase.co/functions/v1/generate-jwt", new { empCode = response.EmpCode });

                        if (!jwtResponse.IsSuccessStatusCode) return false;

                        var tokenObj = await jwtResponse.Content.ReadFromJsonAsync<TokenResponse>();

                        // 2. THIS AUTHENTICATES THE SUPABASE CLIENT
                        //await _supabase.Auth.SetSession(tokenObj.Token, "");
                        //await _supabase.Auth.SetSession(tokenObj.Token, tokenObj.Token);
                        // This forces the database client to use your token for every RLS request
                        

                        // 3. Determine Role
                        string role = (loginId is "103606" or "204957" or "205169") ? "Store" : "Engineer";

                        // 4. Create Session
                        var userSession = new CustomAuthStateProvider.UserSession
                        {
                            Email = response.EmpCode,
                            Role = role,
                            FullName = response.FullName ?? "",
                            Designation = response.Designation ?? "",
                            Grade = response.Grade ?? ""
                        };


                        _supabase.Postgrest.Options.Headers["Authorization"] = $"Bearer {tokenObj.Token}";

                        // 3. Update the UI State (as you already do)
                        await _authStateProvider.UpdateStateAsync(userSession);

                        GlobalVariables.GlobalCurrentUserID = response.Id.ToString();
                        GlobalVariables.GlobalCurrentUserEmail = response.EmpCode;
                        GlobalVariables.GlobalCurrentUserRole = role;

                        await _authStateProvider.UpdateStateAsync(userSession);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical Login Error: {ex.Message}");
                return false;
            }
        }

        public async Task Logout()
        {
            await _supabase.Auth.SignOut();
            _authStateProvider.NotifyLogout();
        }
    }

    // Add this helper class at the bottom of your file
    public class TokenResponse { public string Token { get; set; } }

    // FIXED: Must inherit from BaseModel and have Table mapping
    [Supabase.Postgrest.Attributes.Table("user_permissions")]
    public class UserPermission : BaseModel
    {
        [Supabase.Postgrest.Attributes.Column("email")]
        public string Email { get; set; } = "";

        [Supabase.Postgrest.Attributes.Column("designation")]
        public string Designation { get; set; } = "";

        [Supabase.Postgrest.Attributes.Column("app_role")]
        public string AppRole { get; set; } = "";
    }


    [Supabase.Postgrest.Attributes.Table("employees_engineers")]
    public class EmployeeEngineer : BaseModel
    {
        [Key]
        [Supabase.Postgrest.Attributes.Column("id")]
        public long Id { get; set; }

        [Supabase.Postgrest.Attributes.Column("reports_to_id")]
        public long? ReportsToId { get; set; }

        [EmailAddress]
        [Supabase.Postgrest.Attributes.Column("email")]
        public string? Email { get; set; }

        [Supabase.Postgrest.Attributes.Column("emp_code")]
        public string? EmpCode { get; set; }

        [Supabase.Postgrest.Attributes.Column("designation")]
        public string? Designation { get; set; }

        [Supabase.Postgrest.Attributes.Column("grade")]
        public string? Grade { get; set; }

        [Supabase.Postgrest.Attributes.Column("full_name")]
        public string? FullName { get; set; }

        [Supabase.Postgrest.Attributes.Column("gender")]
        public string? Gender { get; set; }

        [Supabase.Postgrest.Attributes.Column("mobile")]
        public long? Mobile { get; set; }

        [Supabase.Postgrest.Attributes.Column("department")]
        public string? Department { get; set; }

        [Supabase.Postgrest.Attributes.Column("section")]
        public string? Section { get; set; }

        [Supabase.Postgrest.Attributes.Column("created_at")]
        public DateTimeOffset? CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Supabase.Postgrest.Attributes.Column("annual_cost")]
        public long? AnnualCost { get; set; }

        [Supabase.Postgrest.Attributes.Column("personal_code")]
        [StringLength(255)] // Maps to character varying
        public string? PersonalCode { get; set; }

        //// Navigation property for self-referencing relationship
        //[ForeignKey("ReportsToId")]
        //public virtual Employee_FM? Manager { get; set; }
    }



    [Supabase.Postgrest.Attributes.Table("employees_executives")]
    public class EmployeeExecutive : BaseModel
    {
        [Supabase.Postgrest.Attributes.PrimaryKey("id", false)]
        public long Id { get; set; }

        [Supabase.Postgrest.Attributes.Column("email")]
        public string Email { get; set; } = string.Empty;

        [Supabase.Postgrest.Attributes.Column("designation")]
        public string Designation { get; set; } = string.Empty;

        [Supabase.Postgrest.Attributes.Column("full_name")]
        public string FullName { get; set; } = string.Empty;

        [Supabase.Postgrest.Attributes.Column("Department")]
        public string? Department { get; set; }

        [Supabase.Postgrest.Attributes.Column("isLocalAdmin")]
        public bool IsLocalAdmin { get; set; }
    }

    [Supabase.Postgrest.Attributes.Table("employees_fm")]
    public class Employee_FM : BaseModel
    {
        [Supabase.Postgrest.Attributes.PrimaryKey("id", false)]
        public long Id { get; set; }

        [Supabase.Postgrest.Attributes.Column("emp_code")]
        public string EmpCode { get; set; } = string.Empty;

        [Supabase.Postgrest.Attributes.Column("designation")]
        public string? Designation { get; set; }

        [Supabase.Postgrest.Attributes.Column("grade")]
        public string? Grade { get; set; }

        [Supabase.Postgrest.Attributes.Column("full_name")]
        public string? FullName { get; set; }

        // Ensure this column exists in your DB to validate the login!
        [Supabase.Postgrest.Attributes.Column("personal_code")]
        public string? PersonalCode { get; set; }
    }





}