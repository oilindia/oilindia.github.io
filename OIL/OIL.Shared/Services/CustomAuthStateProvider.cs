using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace OIL.Shared.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;

    private readonly ClaimsPrincipal _anonymous =
        new(new ClaimsIdentity());

    public CustomAuthStateProvider(
        ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
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

    public override async Task<AuthenticationState>
        GetAuthenticationStateAsync()
    {
        try
        {
            var userJson =
                await _localStorage.GetItemAsync<string>("user_session");

            if (string.IsNullOrWhiteSpace(userJson))
                return new AuthenticationState(_anonymous);

            var user =
                JsonSerializer.Deserialize<UserSession>(userJson);

            if (user == null)
                return new AuthenticationState(_anonymous);

            return new AuthenticationState(
                CreateClaimsPrincipal(user.Email, user.Role));
        }
        catch
        {
            return new AuthenticationState(_anonymous);
        }
    }

    public async Task UpdateAuthenticationState(
        string email,
        string role)
    {
        var session = new UserSession
        {
            Email = email,
            Role = role
        };

        await _localStorage.SetItemAsync(
            "user_session",
            JsonSerializer.Serialize(session));

        var principal =
            CreateClaimsPrincipal(email, role);

        NotifyAuthenticationStateChanged(
            Task.FromResult(
                new AuthenticationState(principal)));
    }

    public async Task NotifyLogout()
    {
        await _localStorage.RemoveItemAsync("user_session");

        NotifyAuthenticationStateChanged(
            Task.FromResult(
                new AuthenticationState(_anonymous)));
    }

    private ClaimsPrincipal CreateClaimsPrincipal(
        string email,
        string role)
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name,email),
            new Claim(ClaimTypes.Role,role)
        ],
        "CustomAuth");

        return new ClaimsPrincipal(identity);
    }
}

public class UserSession
{
    public string Email { get; set; }
    public string Role { get; set; }
}