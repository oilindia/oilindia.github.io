using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
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

        // 1. Supabase Configuration
        // Note: In production, consider using Microsoft.Extensions.Configuration 
        // or a secure storage approach for these keys.
        var supabaseUrl = "https://pmwutokmedbbphpwxafo.supabase.co";
        var supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InBtd3V0b2ttZWRiYnBocHd4YWZvIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzU0NjYxNzksImV4cCI6MjA5MTA0MjE3OX0.5CyPUDvZiFVj47HimhKuXFFuvt0noAwR3VrYly9q-og"; // Your key

        builder.Services.AddMudServices();

        builder.Services.AddMauiBlazorWebView();

        // Use AddSingleton for Auth state in MAUI
        builder.Services.AddAuthorizationCore();
        builder.Services.AddSingleton<CustomAuthStateProvider>();
        builder.Services.AddSingleton<AuthenticationStateProvider>(s =>
            s.GetRequiredService<CustomAuthStateProvider>());

        // Register Supabase WITHOUT calling BuildServiceProvider()
        builder.Services.AddSingleton(async sp =>
        {
            var client = new Supabase.Client(supabaseUrl, supabaseKey, new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            });

            await client.InitializeAsync();
            return client;
        });

        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<IFormFactor, FormFactor>();

        

        
        //builder.Services.AddMauiBlazorWebView();
        //builder.Services.AddScoped<GeminiService>();
      

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}