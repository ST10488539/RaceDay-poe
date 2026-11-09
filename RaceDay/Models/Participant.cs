using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Participant
    {
        public int ParticipantID { get; set; }
        public int UserID { get; set; }

        [Column(TypeName = "date")]
        public DateTime DateOfBirth { get; set; }

        [Required, MaxLength(10)]
        public string Gender { get; set; } = string.Empty;   // MALE, FEMALE or OTHER

        [MaxLength(200)]
        public string? EmergencyContact { get; set; }

        public string? MedicalNotes { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    }
}