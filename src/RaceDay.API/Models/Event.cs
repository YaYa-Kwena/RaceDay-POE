using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Event
    {
        [Key]
        public int EventID { get; set; }

        [Required]
        public int OrganiserID { get; set; }

        [ForeignKey(nameof(OrganiserID))]
        public User? Organiser { get; set; }

        [Required, MaxLength(150)]
        public string EventName { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string EventType { get; set; } = "Running"; // Running, Walking, Cycling

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal DistanceKm { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required, MaxLength(100)]
        public string LocationCity { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string LocationProvince { get; set; } = string.Empty;

        [Required]
        public int MaxCapacity { get; set; }

        [Required]
        public DateTime RegistrationDeadline { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<EventRoute> EventRoutes { get; set; } = new List<EventRoute>();
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}