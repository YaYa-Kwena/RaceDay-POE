using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RaceDay.API.Models
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required]
        public int EventID { get; set; }

        [ForeignKey(nameof(EventID))]
        [JsonIgnore]
        public Event? Event { get; set; }

        [Required, MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal DistanceKm { get; set; }

        [Required]
        [Column(TypeName = "decimal(8,2)")]
        public decimal EntryFeeZAR { get; set; } = 0.00m;

        public int MinAge { get; set; } = 0;

        public int MaxAge { get; set; } = 120;

        [MaxLength(10)]
        public string GenderRestriction { get; set; } = "Open"; // Male, Female, Open

        [JsonIgnore]
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}