using System.ComponentModel.DataAnnotations;

namespace RaceDay.Models
{
    public class User
    {
        public int UserID { get; set; }

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Role { get; set; } = "Participant";   // Organiser or Participant

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(500)]
        public string? ProfilePictureURL { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public Participant? Participant { get; set; }
        public ICollection<Event> OrganisedEvents { get; set; } = new List<Event>();
    }
}
