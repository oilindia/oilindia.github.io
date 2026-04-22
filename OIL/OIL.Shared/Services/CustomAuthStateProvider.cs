using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace OIL.Shared.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        private ClaimsPrincipal _currentUser;

        public CustomAuthStateProvider() => _currentUser = _anonymous;

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => Task.FromResult(new AuthenticationState(_currentUser));


        public void UpdateAuthenticationState(string email, string role)
        {
            var identity = new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.Name, email),
        new Claim(ClaimTypes.Role, role),
    }, "SupabaseAuth");

            
            var user = new ClaimsPrincipal(identity);
            // We wrap the result in a Task.FromResult to avoid "waiting on monitors"
            var authState = Task.FromResult(new AuthenticationState(user));

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public void AdminUpdateAuthenticationState(string email, string role)
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

        public void NotifyLogout()
        {
            _currentUser = _anonymous;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}