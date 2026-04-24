using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using OIL.Shared.Services;
using OIL.Shared.Services;
using OIL.Web.Client;
using OIL.Web.Client.Services;
using Supabase;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// 1. Load Supabase configuration from appsettings.json
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseKey = builder.Configuration["Supabase:Key"];

if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseKey))
{
    throw new Exception("Supabase URL or Key is missing in appsettings.json");
}


builder.Services.AddBlazoredLocalStorage();


//builder.Services.AddScoped(sp => new Client(supabaseUrl, supabaseKey, new SupabaseOptions
//{
//    AutoRefreshToken = true,
//    AutoConnectRealtime = true,
//    // This ensures the key is included in REST headers
//    Headers = new Dictionary<string, string> { { "apikey", supabaseKey! } }
//}));
// Use a single registration
builder.Services.AddSingleton(provider =>
    new Supabase.Client(
        supabaseUrl!,
        supabaseKey!,
        new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = true,
            // This ensures the session stays in LocalStorage
            SessionHandler = new LocalSessionPersistence(
            builder.Services.BuildServiceProvider().GetRequiredService<ILocalStorageService>())
            // This ensures the session is saved to a persistent store
            //SessionHandler = new Supabase.Gotrue.Interfaces.DefaultSessionHandler()
            // Removed the IGotrueSessionHandler line entirely
        }
    )
);




// Ensure these lines are in your Program.cs or MauiProgram.cs
builder.Services.AddAuthorizationCore(); // REQUIRED
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<GeminiService>();

// 4. MudBlazor Services
builder.Services.AddMudServices();






// 1. Root Component Setup
// App.razor is the entry point which now contains <Routes />
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 2. HTTP & Network Setup
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// 3. Platform-Specific Services
// This implementation of IFormFactor tells the RCL we are in a Web/WASM environment
builder.Services.AddSingleton<IFormFactor, FormFactor>();

// 4. MudBlazor UI Services
// Adds Dialog, Snackbar, ResizeListener, and more
builder.Services.AddMudServices();

// Add Configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

await builder.Build().RunAsync();