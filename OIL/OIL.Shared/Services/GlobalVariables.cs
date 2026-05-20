using System;
using System.Collections.Generic;
using System.Text;

namespace OIL.Shared.Services
{
    public static class GlobalVariables
    {
        public static string GlobalLatestDieselPrice { get; set; } = "0.00";
        public static string GlobalLatestDieselPriceDate { get; set; } = "";
        public static string GlobalCurrentUserEmail { get; set; } = "";
        public static string GlobalCurrentUserRole { get; set; } = "";
        public static string GlobalCurrentUserName { get; set; } = "";
        public static string GlobalCurrentUserID { get; set; } ="";
        public static string GlobalCurrentUserTok { get; set; } = "";
        //public static string LatestDieselPriceDate { get; set; } = "";
        //public static string LatestDieselPriceDate { get; set; } = "";
        //public static string LatestDieselPriceDate { get; set; } = "";

    }
}
