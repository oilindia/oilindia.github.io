using Supabase; // Ensure your Supabase NuGet package is accessible here
using Supabase.Postgrest;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Threading.Tasks;
namespace OIL.Shared.Services;

public class VersionCheckerService
{
    private readonly IAppPlatformService _platformService;
    private readonly Supabase.Client _supabaseClient;

    public VersionCheckerService(IAppPlatformService platformService, Supabase.Client supabaseClient)
    {
        _platformService = platformService;
        _supabaseClient = supabaseClient;
    }

    public async Task<UpdateCheckResult> CheckForUpdatesAsync()
    {
        try
        {
            string currentPlatform = _platformService.GetPlatformName();

            // Web apps are always up to date. Short-circuit here.
            if (currentPlatform == "web")
            {
                return new UpdateCheckResult { Status = UpdateStatus.UpToDate };
            }

            string currentVersionStr = _platformService.GetAppVersion();

            // Fetch the platform row from Supabase
            var response = await _supabaseClient
                .From<AppVersionModel>()
                .Filter("platform", Supabase.Postgrest.Constants.Operator.Equals, currentPlatform)
                .Single();

            if (response == null) return new UpdateCheckResult { Status = UpdateStatus.UpToDate };

            var currentVersion = new Version(currentVersionStr);
            var latestVersion = new Version(response.LatestVersion);
            var minRequiredVersion = new Version(response.MinRequiredVersion);

            if (currentVersion < minRequiredVersion)
            {
                return new UpdateCheckResult
                {
                    Status = UpdateStatus.ForceUpdate,
                    UpdateUrl = response.UpdateUrl
                };
            }
            else if (currentVersion < latestVersion)
            {
                return new UpdateCheckResult
                {
                    Status = UpdateStatus.FlexibleUpdate,
                    UpdateUrl = response.UpdateUrl
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Version check failed gracefully: {ex.Message}");
        }

        return new UpdateCheckResult { Status = UpdateStatus.UpToDate };
    }
}

[Table("app_versions")]
public class AppVersionModel : BaseModel
{
    [PrimaryKey("id", false)] // false indicates it is an auto-generated identity column
    public long Id { get; set; }

    [Column("platform")]
    public string Platform { get; set; }

    [Column("latest_version")]
    public string LatestVersion { get; set; }

    [Column("min_required_version")]
    public string MinRequiredVersion { get; set; }

    [Column("update_url")]
    public string UpdateUrl { get; set; }

    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }
}

public class UpdateCheckResult
{
    public UpdateStatus Status { get; set; }
    public string UpdateUrl { get; set; }
}

public enum UpdateStatus
{
    /// <summary>
    /// The app version matches or exceeds the latest version on the server.
    /// </summary>
    UpToDate,

    /// <summary>
    /// A new version exists, but the user can dismiss the modal and choose to update later.
    /// </summary>
    FlexibleUpdate,

    /// <summary>
    /// The user's version is lower than the minimum required version. They are blocked from entry.
    /// </summary>
    ForceUpdate
}