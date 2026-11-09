using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Registration
    {
        public int RegistrationID { get; set; }
        public int ParticipantID { get; set; }
        public int CategoryID { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string? RaceNumber { get; set; }

        [Required, MaxLength(30)]
        public string RegistrationStatus { get; set; } = "Registered";

        [Column(TypeName = "decimal(10,2)")]
        public decimal AmountPaid { get; set; }

        public Participant Participant { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public Result? Result { get; set; }
    }
}