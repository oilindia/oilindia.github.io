using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using OIL.Services; // Ensure this contains your MAUI FormFactor
using OIL.Shared.Services;
using Supabase;

namespace OIL;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // 1. Supabase Configuration (Use the same keys as WASM)
        var supabaseUrl = "https://pmwutokmedbbphpwxafo.supabase.co";
        var supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InBtd3V0b2ttZWRiYnBocHd4YWZvIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzU0NjYxNzksImV4cCI6MjA5MTA0MjE3OX0.5CyPUDvZiFVj47HimhKuXFFuvt0noAwR3VrYly9q-og";

        // --- CRITICAL MISSING SERVICES START ---
        builder.Services.AddAuthorizationCore();
        builder.Services.AddScoped<CustomAuthStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(s =>
            s.GetRequiredService<CustomAuthStateProvider>());
        // --- CRITICAL MISSING SERVICES END ---

        builder.Services.AddScoped<Supabase.Client>(provider =>
            new Supabase.Client(supabaseUrl, supabaseKey, new SupabaseOptions { AutoConnectRealtime = true }));

        builder.Services.AddScoped<AuthService>();
        builder.Services.AddSingleton<IFormFactor, FormFactor>();
        builder.Services.AddMudServices();
        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}