using Google.GenAI.Types;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;
using Supabase.Gotrue;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
                    // Initialize session as null; Supabase.Gotrue.Session is the specific type
                    Supabase.Gotrue.Session? session = null;

                    try
                    {
                        // Try standard Supabase Auth first
                        session = await _supabase.Auth.SignIn(email, password);
                        Console.WriteLine($"Session created for: {session?.User?.Email}");
                    }
                    catch (Exception ex)
                    {
                        // Log but don't return yet; we want to try the fallback check below
                        Console.WriteLine($"Supabase Auth Attempt Failed: {ex.Message}");
                    }

                    // CASE 1: Supabase Auth Succeeded
                    if (session?.AccessToken != null)
                    {
                        var response = await _supabase.From<UserPermission>()
                            .Filter("email", Supabase.Postgrest.Constants.Operator.Equals, email)
                            .Get();

                        var userPerm = response.Models.FirstOrDefault();
                        string assignedRole = userPerm?.AppRole ?? "User"; // Default role if not found in view

                        _authStateProvider.UpdateAuthenticationState(email, assignedRole);
                        return true;
                    }

                    // CASE 2: Supabase Auth failed/missing, falling back to manual Table Verification
                    else
                    {
                        var response = await _supabase.From<Employee_FM>()
                            .Filter("email", Supabase.Postgrest.Constants.Operator.Equals, email)
                            .Get();

                        var employee = response.Models.FirstOrDefault();

                        // Check if employee exists and PersonalCode matches the entered password
                        if (employee != null && employee.PersonalCode == password)
                        {
                            Console.WriteLine($"Manual verification successful for: {employee.Designation}");

                            // Using your specific AdminUpdate provider method
                            _authStateProvider.AdminUpdateAuthenticationState(email, "Engineer");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("Invalid credentials for both Auth and Employee Table.");
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Critical Login Error: {ex.Message}");
                    return false;
                }






                //try
                //{
                //    var session; 
                //    try
                //    {
                //        session= await _supabase.Auth.SignIn(email, password);
                //        Console.WriteLine($"session: {session.ToString()}");
                //    }
                //    catch (Exception ex)
                //    {
                //        Console.WriteLine($"Login Error: {ex.Message}");
                //        //return false;
                //    }
                //    // 1. Sign In (ASYNCHRONOUS)

                //    if (session.ToString().IsNullOrEmpty()) {


                //        // 2. Query the View (ASYNCHRONOUS)
                //        // Ensure you are using .Get() and NOT .Result
                //        var response = await _supabase.From<UserPermission>()
                //            .Filter("email", Supabase.Postgrest.Constants.Operator.Equals, email)
                //            .Get();

                //        var userPerm = response.Models.FirstOrDefault();

                //        Console.WriteLine($"Usr: {userPerm.AppRole}");

                //        // 3. Determine Role
                //        string assignedRole = userPerm?.AppRole ?? "Admin";

                //        // 4. Update the Provider
                //        _authStateProvider.UpdateAuthenticationState(email, assignedRole);

                //        return true;



                //    }


                //    else
                //    {
                //        // 2. Query the View (ASYNCHRONOUS)
                //        // Ensure you are using .Get() and NOT .Result
                //        var response = await _supabase.From<Employee_FM>()
                //            .Filter("email", Supabase.Postgrest.Constants.Operator.Equals, email)
                //            .Order("priority", Supabase.Postgrest.Constants.Ordering.Ascending)
                //            .Get();

                //        var userPerm = response.Models.FirstOrDefault();

                //        Console.WriteLine($"Usr: {userPerm.Designation}");


                //        if (userPerm != null && userPerm.PersonalCode == password) {

                //            // 3. Determine Role
                //            //string assignedRole = userPerm?.Designation ?? "Engineer";

                //            // 4. Update the Provider
                //            _authStateProvider.AdminUpdateAuthenticationState(email ?? email, "Engineer");

                //            return true;

                //        }


                //        else
                //        {
                //            return false;
                //        }

                //    }




                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine($"Login Error: {ex.Message}");
                //    return false;
                //}

            }
        }

        public async Task Logout()
        {
            await _supabase.Auth.SignOut();
            _authStateProvider.NotifyLogout();
        }
    }

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


    [Supabase.Postgrest.Attributes.Table("employees_fm")]
    public class Employee_FM : BaseModel
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

        // Navigation property for self-referencing relationship
        [ForeignKey("ReportsToId")]
        public virtual Employee_FM? Manager { get; set; }
    }
}