using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class CreateEventRequestDto
    {
        [Required]
        public string EventName { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string EventType { get; set; } = "Running"; // Running, Walking, Cycling

        [Required]
        public decimal DistanceKm { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public string LocationCity { get; set; } = string.Empty;

        [Required]
        public string LocationProvince { get; set; } = string.Empty;

        [Required]
        public int MaxCapacity { get; set; }

        [Required]
        public DateTime RegistrationDeadline { get; set; }
    }

    public class UpdateEventRequestDto
    {
        [Required]
        public string EventName { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string EventType { get; set; } = "Running";

        [Required]
        public decimal DistanceKm { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public string LocationCity { get; set; } = string.Empty;

        [Required]
        public string LocationProvince { get; set; } = string.Empty;

        [Required]
        public int MaxCapacity { get; set; }

        [Required]
        public DateTime RegistrationDeadline { get; set; }

        public bool IsActive { get; set; } = true;
    }
}