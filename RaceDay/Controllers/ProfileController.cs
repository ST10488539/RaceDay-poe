using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.DTOs;
using RaceDay.Filters;
using RaceDay.Models;
using RaceDay.Services;

namespace RaceDay.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [SessionAuthorize]
    public class ProfileController : ControllerBase
    {
        private static readonly string[] ValidGenders = { "MALE", "FEMALE", "OTHER" };

        private readonly RaceDayDbContext _db;
        private readonly IBlobStorageService _blobs;

        public ProfileController(RaceDayDbContext db, IBlobStorageService blobs)
        {
            _db = db;
            _blobs = blobs;
        }

        private int CurrentUserId => HttpContext.Session.GetInt32("UserID")!.Value;

        private static ProfileDto ToDto(User user) => new ProfileDto
        {
            UserID = user.UserID,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            Phone = user.Phone,
            ProfilePictureURL = user.ProfilePictureURL,
            DateCreated = user.DateCreated,
            DateOfBirth = user.Participant?.DateOfBirth,
            Gender = user.Participant?.Gender,
            EmergencyContact = user.Participant?.EmergencyContact,
            MedicalNotes = user.Participant?.MedicalNotes
        };

        /// <summary>
        /// Returns the profile of the logged-in user (Organiser or Participant).
        /// </summary>
        /// <remarks>
        /// Requires login. Returns 200 with the profile (participant details are included
        /// for participants), 401 if not logged in, or 404 if the account no longer exists.
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var user = await _db.Users
                .Include(u => u.Participant)
                .FirstOrDefaultAsync(u => u.UserID == CurrentUserId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(ToDto(user));
        }

        /// <summary>
        /// Updates the profile of the logged-in user.
        /// </summary>
        /// <remarks>
        /// Requires login. Expects firstName, lastName and optionally phone.
        /// Participants must also send dateOfBirth and gender (MALE, FEMALE or OTHER),
        /// and may send emergencyContact and medicalNotes.
        /// Email, password and role cannot be changed here.
        /// Returns 200 with the updated profile, 400 for invalid data, or 401 if not logged in.
        /// </remarks>
        [HttpPut]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
        {
            var user = await _db.Users
                .Include(u => u.Participant)
                .FirstOrDefaultAsync(u => u.UserID == CurrentUserId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            if (user.Role == "Participant" && user.Participant != null)
            {
                string? gender = dto.Gender?.Trim().ToUpper();

                if (dto.DateOfBirth == null)
                    return BadRequest(new { message = "Date of birth is required for participants." });

                if (gender == null || !ValidGenders.Contains(gender))
                    return BadRequest(new { message = "Gender must be MALE, FEMALE or OTHER." });

                user.Participant.DateOfBirth = dto.DateOfBirth.Value;
                user.Participant.Gender = gender;
                user.Participant.EmergencyContact = dto.EmergencyContact;
                user.Participant.MedicalNotes = dto.MedicalNotes;
            }

            user.FirstName = dto.FirstName.Trim();
            user.LastName = dto.LastName.Trim();
            user.Phone = dto.Phone;

            await _db.SaveChangesAsync();

            return Ok(ToDto(user));
        }

        /// <summary>
        /// Uploads or replaces the logged-in user's profile picture.
        /// </summary>
        /// <remarks>
        /// Requires login (Organiser or Participant). Send multipart/form-data with one file field
        /// named "file" (JPG, PNG or WEBP, up to 5 MB). The image is stored in Azure Blob Storage.
        /// Returns 200 with the updated profile, 400 if the file is not a valid image,
        /// 401 if not logged in, or 404 if the account no longer exists.
        /// </remarks>
        [HttpPost("picture")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            var user = await _db.Users
                .Include(u => u.Participant)
                .FirstOrDefaultAsync(u => u.UserID == CurrentUserId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            string? error = _blobs.Validate(file);
            if (error != null)
                return BadRequest(new { message = error });

            user.ProfilePictureURL = await _blobs.UploadImageAsync(file, "profiles");
            await _db.SaveChangesAsync();

            return Ok(ToDto(user));
        }
    }
}
