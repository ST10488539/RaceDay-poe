using System.ComponentModel.DataAnnotations;

namespace RaceDay.DTOs
{
    public class RegisterDto
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(8), MaxLength(100)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        /// <summary>Organiser or Participant</summary>
        [Required]
        public string Role { get; set; } = string.Empty;

        // Participants only
        public DateTime? DateOfBirth { get; set; }

        /// <summary>MALE, FEMALE or OTHER</summary>
        public string? Gender { get; set; }

        [MaxLength(200)]
        public string? EmergencyContact { get; set; }

        public string? MedicalNotes { get; set; }
    }

    public class LoginDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}