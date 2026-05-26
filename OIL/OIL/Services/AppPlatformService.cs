using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using OIL.Shared.Services; //

namespace OIL.Services
{
    public class AppPlatformService : IAppPlatformService
    {
        public string GetAppVersion()
        {
            // Automatically fetches the version from your .csproj file
            return AppInfo.Current.VersionString;
        }

        public string GetPlatformName()
        {
            // This will strictly map to 'android' inside your Supabase layout
            if (DeviceInfo.Current.Platform == DevicePlatform.Android) return "android";
            return "unknown";
        }

        public async Task OpenUrlAsync(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                // Fires up the system native Android browser (Google Play link, etc.)
                await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            }
        }
    }
}