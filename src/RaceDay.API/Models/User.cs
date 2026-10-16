using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RaceDay.API.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string UserRole { get; set; } = "Participant"; // Organiser or Participant

        [MaxLength(10)]
        public string? Gender { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [MaxLength(100)]
        public string? EmergencyContactName { get; set; }

        [MaxLength(15)]
        public string? EmergencyContactPhone { get; set; }

        [MaxLength(500)]
        public string MedicalConditions { get; set; } = "None";

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public ICollection<Event> OrganisedEvents { get; set; } = new List<Event>();

        [JsonIgnore]
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}