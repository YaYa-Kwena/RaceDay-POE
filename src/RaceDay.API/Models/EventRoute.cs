using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RaceDay.API.Models
{
    public class EventRoute
    {
        [Key]
        public int RouteID { get; set; }

        [Required]
        public int EventID { get; set; }

        [ForeignKey(nameof(EventID))]
        [JsonIgnore]
        public Event? Event { get; set; }

        [Required, MaxLength(100)]
        public string RouteName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal TotalDistanceKm { get; set; }

        public int ElevationGainMeters { get; set; } = 0;

        public int WaterPointsCount { get; set; } = 0;

        [MaxLength(255)]
        public string? RouteMapUrl { get; set; }
    }
}