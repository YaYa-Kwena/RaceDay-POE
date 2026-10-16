using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class CreateCategoryRequestDto
    {
        [Required]
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        public decimal DistanceKm { get; set; }

        [Required]
        public decimal EntryFeeZAR { get; set; }

        public int MinAge { get; set; } = 0;

        public int MaxAge { get; set; } = 120;

        public string GenderRestriction { get; set; } = "Open"; // Male, Female, Open
    }

    public class UpdateCategoryRequestDto
    {
        [Required]
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        public decimal DistanceKm { get; set; }

        [Required]
        public decimal EntryFeeZAR { get; set; }

        public int MinAge { get; set; } = 0;

        public int MaxAge { get; set; } = 120;

        public string GenderRestriction { get; set; } = "Open";
    }

    public class CreateRouteRequestDto
    {
        [Required]
        public string RouteName { get; set; } = string.Empty;

        [Required]
        public decimal TotalDistanceKm { get; set; }

        public int ElevationGainMeters { get; set; } = 0;

        public int WaterPointsCount { get; set; } = 0;

        public string? RouteMapUrl { get; set; }
    }

    public class CreateEnrolmentRequestDto
    {
        [Required]
        public int EventId { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }

    public class CaptureResultRequestDto
    {
        public TimeSpan? FinishTime { get; set; }

        public int? OverallPosition { get; set; }

        public int? CategoryPosition { get; set; }

        [Required]
        public string RaceStatus { get; set; } = "Finished"; // Finished, DNF, DNS, Disqualified
    }
}