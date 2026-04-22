using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OIL.Shared.Models
{
    [Table("jobs")] // Must match your table name in Supabase exactly
    public class JobModel : BaseModel
    {
        [PrimaryKey("id", false)] // false means the DB handles auto-increment
        public long Id { get; set; }

        [Column("job_name")]
        public string JobName { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("location")]
        public string Location { get; set; }

        [Column("priority")]
        public string Priority { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("reported_by_email")]
        public string ReportedByEmail { get; set; }
    }
}