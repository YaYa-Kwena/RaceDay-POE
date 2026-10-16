using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Enrolment
    {
        [Key]
        public int EnrolmentID { get; set; }

        [Required]
        public int ParticipantID { get; set; }

        [ForeignKey(nameof(ParticipantID))]
        public User? Participant { get; set; }

        [Required]
        public int EventID { get; set; }

        [ForeignKey(nameof(EventID))]
        public Event? Event { get; set; }

        [Required]
        public int CategoryID { get; set; }

        [ForeignKey(nameof(CategoryID))]
        public Category? Category { get; set; }

        public int RaceNumber { get; set; }

        [MaxLength(20)]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Refunded

        public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

        // Optional 1:0..1 relationship (Enrolment can exist before Result is recorded)
        public Result? Result { get; set; }
    }
}