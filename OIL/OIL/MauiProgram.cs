using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using MudBlazor;
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

        builder.Services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
            config.PopoverOptions.ThrowOnDuplicateProvider = false;
        });


        // 1. Essential MudBlazor Services
        //builder.Services.AddMudServices();
        builder.Services.AddMudBlazorDialog();
        builder.Services.AddMudBlazorSnackbar();

        // 2. Hybrid WebView Service
        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddBlazoredLocalStorage();

        //builder.RootComponents.Add<SharedProject.App>("app");

        // Use AddSingleton for Auth state in MAUI
        builder.Services.AddAuthorizationCore();
        builder.Services.AddScoped<CustomAuthStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(s =>
            s.GetRequiredService<CustomAuthStateProvider>());

        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<IFormFactor, FormFactor>();


        builder.Services.AddScoped<AttendanceService>();
        builder.Services.AddScoped<GeminiService>();

        //builder.Services.AddScoped<DailyDieselPriceService>();

        // ---- ADD THESE TWO LINES FOR VERSIONING ----
        builder.Services.AddSingleton<IAppPlatformService, AppPlatformService>();
        builder.Services.AddTransient<VersionCheckerService>();


        //// Register Supabase WITHOUT calling BuildServiceProvider()
        //builder.Services.AddSingleton(async sp =>
        //{
        //    var client = new Supabase.Client(supabaseUrl, supabaseKey, new SupabaseOptions
        //    {
        //        AutoRefreshToken = true,
        //        AutoConnectRealtime = true
        //    });

        //    await client.InitializeAsync();
        //    return client;
        //});


        // Supabase
        builder.Services.AddSingleton<Supabase.Client>(sp =>
        {
            var client = new Supabase.Client(
                supabaseUrl,
                supabaseKey,
                new SupabaseOptions
                {
                    AutoRefreshToken = true,
                    AutoConnectRealtime = true
                });

            //client.InitializeAsync().Wait();

            return client;
        });


       




        //builder.Services.AddMauiBlazorWebView();
        //builder.Services.AddScoped<GeminiService>();


#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}