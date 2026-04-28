using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage; // Recommended: install 'Blazored.LocalStorage'

namespace OIL.Shared.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthStateProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // Look for saved user data in the browser's local storage
                var userJson = await _localStorage.GetItemAsync<string>("user_session");

                if (string.IsNullOrEmpty(userJson))
                    return new AuthenticationState(_anonymous);

                var userData = JsonSerializer.Deserialize<UserSession>(userJson);
                var userPrincipal = CreateClaimsPrincipal(userData.Email, userData.Role);

                return new AuthenticationState(userPrincipal);
            }
            catch
            {
                return new AuthenticationState(_anonymous);
            }
        }

        public void AdminUpdateAuthenticationState(string email, string role)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, email),
                    new Claim(ClaimTypes.Role, role)
                };

                var identity = new ClaimsIdentity(claims, "CustomAuth");
                var user = new ClaimsPrincipal(identity);

                // We wrap the result in a Task.FromResult to avoid "waiting on monitors"
                var authState = Task.FromResult(new AuthenticationState(user));

                NotifyAuthenticationStateChanged(authState);
            }
            catch (Exception ex)
            {
                
            }
        }

        public async Task UpdateAuthenticationState(string email, string role)
        {
            try
            {
                var userSession = new UserSession { Email = email, Role = role };

            // 1. Persist the user to LocalStorage so it survives refreshes/back button
            await _localStorage.SetItemAsync("user_session", JsonSerializer.Serialize(userSession));

            // 2. Create the principal and notify the UI
            var user = CreateClaimsPrincipal(email, role);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));

            }
            catch (Exception ex)
            {

            }
        }

        public async Task NotifyLogout()
        {
            try
            {
                // Clear storage and reset
                await _localStorage.RemoveItemAsync("user_session");
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));


            }
            catch (Exception ex)
            {
                
            }
}

        private ClaimsPrincipal CreateClaimsPrincipal(string email, string role)
        {
            try
            {
                var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, role),
            }, "SupabaseAuth");

            return new ClaimsPrincipal(identity);

            }
            catch (Exception ex)
            {
                return null;
            }

        }
    }

    public class UserSession
    {
        public string Email { get; set; }
        public string Role { get; set; }
    }



    //public class CustomAuthStateProvider : AuthenticationStateProvider
    //{
    //    private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());
    //    private ClaimsPrincipal _currentUser;

    //    public CustomAuthStateProvider() => _currentUser = _anonymous;

    //    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    //        => Task.FromResult(new AuthenticationState(_currentUser));


    //    public void UpdateAuthenticationState(string email, string role)
    //    {
    //        var identity = new ClaimsIdentity(new[]
    //        {
    //    new Claim(ClaimTypes.Name, email),
    //    new Claim(ClaimTypes.Role, role),
    //}, "SupabaseAuth");


    //        var user = new ClaimsPrincipal(identity);
    //        // We wrap the result in a Task.FromResult to avoid "waiting on monitors"
    //        var authState = Task.FromResult(new AuthenticationState(user));

    //        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    //    }

    //    public void AdminUpdateAuthenticationState(string email, string role)
    //    {
    //        var claims = new List<Claim>
    //        {
    //            new Claim(ClaimTypes.Name, email),
    //            new Claim(ClaimTypes.Role, role)
    //        };

    //        var identity = new ClaimsIdentity(claims, "CustomAuth");
    //        var user = new ClaimsPrincipal(identity);

    //        // We wrap the result in a Task.FromResult to avoid "waiting on monitors"
    //        var authState = Task.FromResult(new AuthenticationState(user));

    //        NotifyAuthenticationStateChanged(authState);
    //    }

    //    public void NotifyLogout()
    //    {
    //        _currentUser = _anonymous;
    //        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    //    }
    //}
}