using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace OIL.Shared.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
    private readonly string _nativeFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "oil_user_session.json");

    // CRITICAL: Cache the current state in-memory so Blazor routing components get an instant answer
    private Task<AuthenticationState>? _cachedStateTask;

    public CustomAuthStateProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    private bool IsNativePlatform()
    {
        try
        {
            return OperatingSystem.IsAndroid() ||
                   OperatingSystem.IsIOS() ||
                   OperatingSystem.IsWindows() ||
                   OperatingSystem.IsMacCatalyst();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PlatformCheck Error]: {ex.Message}");
            return false;
        }
    }

    private async Task SaveSessionAsync(UserSession session)
    {
        try
        {
            string jsonSession = JsonSerializer.Serialize(session);
            if (IsNativePlatform())
            {
                await File.WriteAllTextAsync(_nativeFilePath, jsonSession);
            }
            else
            {
                await _localStorage.SetItemAsync("user_session", jsonSession);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthStorage Error]: Save failed: {ex.Message}");
        }
    }

    private async Task<string?> ReadSessionAsync()
    {
        try
        {
            if (IsNativePlatform())
            {
                if (File.Exists(_nativeFilePath))
                {
                    return await File.ReadAllTextAsync(_nativeFilePath);
                }
                return string.Empty;
            }
            else
            {
                return await _localStorage.GetItemAsync<string>("user_session");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthStorage Error]: Read failed: {ex.Message}");
            return null;
        }
    }

    private async Task ClearSessionAsync()
    {
        try
        {
            if (IsNativePlatform())
            {
                if (File.Exists(_nativeFilePath)) File.Delete(_nativeFilePath);
            }
            else
            {
                await _localStorage.RemoveItemAsync("user_session");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthStorage Error]: Clear failed: {ex.Message}");
        }
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // 1. If we already have a runtime cached identity state task, return it instantly.
            // This eliminates the 1000ms disk IO latency during internal app navigation routing.
            if (_cachedStateTask != null)
            {
                return await _cachedStateTask;
            }

            // 2. Fallback to cold disk/localstorage read only on initial cold boot setup
            var userJson = await ReadSessionAsync();

            if (string.IsNullOrWhiteSpace(userJson))
            {
                _cachedStateTask = Task.FromResult(new AuthenticationState(_anonymous));
                return await _cachedStateTask;
            }

            var user = JsonSerializer.Deserialize<UserSession>(userJson);
            if (user == null)
            {
                _cachedStateTask = Task.FromResult(new AuthenticationState(_anonymous));
                return await _cachedStateTask;
            }

            var principal = CreateClaimsPrincipal(user.Email, user.Role);
            _cachedStateTask = Task.FromResult(new AuthenticationState(principal));
            return await _cachedStateTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetAuthenticationStateAsync Error]: {ex.Message}");
            return new AuthenticationState(_anonymous);
        }
    }

    public async Task AdminUpdateAuthenticationStateAsync(string email, string role)
    {
        try
        {
            var session = new UserSession { Email = email, Role = role };
            await SaveSessionAsync(session);

            var principal = CreateClaimsPrincipal(email, role);

            // Set memory cache immediately before notifying the UI
            _cachedStateTask = Task.FromResult(new AuthenticationState(principal));
            NotifyAuthenticationStateChanged(_cachedStateTask);

            Console.WriteLine($"[AdminUpdate]: In-Memory identity cached for {email}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AdminUpdate Error]: {ex.Message}");
        }
    }

    public async Task UpdateAuthenticationState(string email, string role)
    {
        try
        {
            var session = new UserSession { Email = email, Role = role };
            await SaveSessionAsync(session);

            var principal = CreateClaimsPrincipal(email, role);

            // Set memory cache immediately before notifying the UI
            _cachedStateTask = Task.FromResult(new AuthenticationState(principal));
            NotifyAuthenticationStateChanged(_cachedStateTask);

            Console.WriteLine($"[UpdateAuthentication]: In-Memory identity cached for {email}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UpdateAuthentication Error]: {ex.Message}");
        }
    }

    public async Task NotifyLogout()
    {
        try
        {
            await ClearSessionAsync();
            _cachedStateTask = Task.FromResult(new AuthenticationState(_anonymous));
            NotifyAuthenticationStateChanged(_cachedStateTask);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotifyLogout Error]: {ex.Message}");
        }
    }

    private ClaimsPrincipal CreateClaimsPrincipal(string email, string role)
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.Role, role)
        ], "CustomAuth");

        return new ClaimsPrincipal(identity);
    }


    public class UserSession
    {
        public string Email { get; set; }
        public string Role { get; set; }
    }
}