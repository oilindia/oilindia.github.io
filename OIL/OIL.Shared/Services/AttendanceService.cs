using System;
using System.Collections.Generic;
using System.Text;
using static OIL.Shared.Pages.JOB.JobAssign;

namespace OIL.Shared.Services
{
    public class AttendanceService
    {
        public List<Member> Engineers { get; set; } = new();
        public List<Member> Drivers { get; set; } = new();
        public Dictionary<string, string> AttendanceCache { get; set; } = new();
        public Dictionary<string, string> RemarksCache { get; set; } = new();
        public bool IsDataReady { get; set; }
        public string ErrorMessage { get; set; }

        public void ResetState()
        {
            IsDataReady = false;
            AttendanceCache.Clear();
            RemarksCache.Clear();
        }

        public async Task InitializeAsync(Supabase.Client client)
        {
            if (IsDataReady) return;
            try
            {
                var engTask = client.From<EmployeeEngineerModel>().Get();
                var drvTask = client.From<DriverModel>().Get();
                await Task.WhenAll(engTask, drvTask);

                Engineers = engTask.Result.Models.Select(e => new Member { FullName = e.FullName, UniqueId = e.EmpCode, Role = "Engineer" }).ToList();
                Drivers = drvTask.Result.Models.Select(d => new Member { FullName = d.FullName, UniqueId = d.Id.ToString(), Role = "Driver", DbId = d.Id, VehicleNumber = d.VehicleNumber }).ToList();

                var start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).ToString("yyyy-MM-dd");
                var end = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd");

                var attRes = await client.From<LatestAttendanceViewModel>()
                    .Filter("work_date", Supabase.Postgrest.Constants.Operator.GreaterThanOrEqual, start)
                    .Filter("work_date", Supabase.Postgrest.Constants.Operator.LessThanOrEqual, end)
                    .Get();

                AttendanceCache.Clear();
                RemarksCache.Clear();
                foreach (var r in attRes.Models)
                {
                    var key = $"{(r.EmployeeCode ?? r.DriverId.ToString())}_{r.WorkDate:yyyyMMdd}";
                    AttendanceCache[key] = r.Status ?? "";
                    RemarksCache[key] = r.Reason ?? "";
                }

                IsDataReady = true;
                ErrorMessage = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}
