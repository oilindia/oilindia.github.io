using MudBlazor;
using System;
using System.Collections.Generic;
using System.Text;

namespace OIL.Shared.Pages
{
    // In OIL.Shared/Themes/OilTheme.cs
    public static class OilTheme
    {
        public static MudTheme DefaultTheme = new MudTheme()
        {
            PaletteLight = new PaletteLight()
            {
                Primary = "#D32F2F",
                Secondary = "#B71C1C",
                AppbarBackground = "#D32F2F",
                Background = "#F5F5F5",
                Surface = "#FFFFFF",
                ActionDefault = "#D32F2F"
            }
        };
    }
}