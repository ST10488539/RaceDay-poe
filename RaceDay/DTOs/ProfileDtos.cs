using System.ComponentModel.DataAnnotations;

namespace RaceDay.DTOs
{
    public class ProfileDto
    {
        public int UserID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? ProfilePictureURL { get; set; }
        public DateTime DateCreated { get; set; }

        // Participants only
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? EmergencyContact { get; set; }
        public string? MedicalNotes { get; set; }
    }

    public class UpdateProfileDto
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        // Participants only
        public DateTime? DateOfBirth { get; set; }

        /// <summary>MALE, FEMALE or OTHER</summary>
        public string? Gender { get; set; }

        [MaxLength(200)]
        public string? EmergencyContact { get; set; }

        public string? MedicalNotes { get; set; }
    }
}
