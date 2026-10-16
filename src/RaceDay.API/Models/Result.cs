using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RaceDay.API.Models
{
    public class Result
    {
        [Key]
        public int ResultID { get; set; }

        [Required]
        public int EnrolmentID { get; set; }

        [ForeignKey(nameof(EnrolmentID))]
        [JsonIgnore]
        public Enrolment? Enrolment { get; set; }

        public TimeSpan? FinishTime { get; set; }

        public int? OverallPosition { get; set; }

        public int? CategoryPosition { get; set; }

        [MaxLength(20)]
        public string RaceStatus { get; set; } = "Finished"; // Finished, DNF, DNS, Disqualified

        public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
    }
}