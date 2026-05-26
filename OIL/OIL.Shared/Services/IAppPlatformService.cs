using System;
using System.Collections.Generic;
using System.Text;

namespace OIL.Shared.Services
{
    public interface IAppPlatformService
    {
        string GetAppVersion();
        string GetPlatformName();
        Task OpenUrlAsync(string url);
    }
}
