using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OIL.Shared.Models
{
    [Table("employees")]
    public class EmployeeModel : BaseModel
    {
        [PrimaryKey("email")]
        public string Email { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("designation")]
        public string Designation { get; set; }

        [Column("location")]
        public string Location { get; set; }

        [Column("monthly_cost")]
        public double MonthlyCost { get; set; }

        [Column("productivity_score")]
        public int ProductivityScore { get; set; }

        [Column("reporting_to_email")]
        public string ReportingToEmail { get; set; }
    }
}
