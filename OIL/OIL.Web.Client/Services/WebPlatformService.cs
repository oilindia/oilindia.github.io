using OIL.Shared.Services;

namespace OIL.Web.Client.Services
{
    public class WebPlatformService : IAppPlatformService
    {
        // Web app doesn't have an application installation string
        public string GetAppVersion() => "1.0.0";

        // Explicitly markers this deployment context as Web
        public string GetPlatformName() => "web";

        public Task OpenUrlAsync(string url)
        {
            return Task.CompletedTask;
        }
    }
}