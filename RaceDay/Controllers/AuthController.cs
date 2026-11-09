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
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private static readonly string[] ValidGenders = { "MALE", "FEMALE", "OTHER" };

        private readonly RaceDayDbContext _db;
        private readonly IPasswordService _passwords;

        public AuthController(RaceDayDbContext db, IPasswordService passwords)
        {
            _db = db;
            _passwords = passwords;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            string role;
            if (dto.Role.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
                role = "Organiser";
            else if (dto.Role.Equals("Participant", StringComparison.OrdinalIgnoreCase))
                role = "Participant";
            else
                return BadRequest(new { message = "Role must be Organiser or Participant." });

            string? gender = dto.Gender?.Trim().ToUpper();

            if (role == "Participant")
            {
                if (dto.DateOfBirth == null)
                    return BadRequest(new { message = "Date of birth is required for participants." });

                if (gender == null || !ValidGenders.Contains(gender))
                    return BadRequest(new { message = "Gender must be MALE, FEMALE or OTHER." });
            }

            string email = dto.Email.Trim().ToLower();
            if (await _db.Users.AnyAsync(u => u.Email == email))
                return Conflict(new { message = "That email is already registered." });

            var user = new User
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = email,
                PasswordHash = _passwords.Hash(dto.Password),
                Role = role,
                Phone = dto.Phone
            };

            if (role == "Participant")
            {
                user.Participant = new Participant
                {
                    DateOfBirth = dto.DateOfBirth!.Value,
                    Gender = gender!,
                    EmergencyContact = dto.EmergencyContact,
                    MedicalNotes = dto.MedicalNotes
                };
            }

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created,
                new { userId = user.UserID, email = user.Email, role = user.Role });
        }

      
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            string email = dto.Email.Trim().ToLower();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !_passwords.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid email or password." });

            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("Role", user.Role);

            return Ok(new
            {
                message = "Login successful.",
                userId = user.UserID,
                name = $"{user.FirstName} {user.LastName}",
                role = user.Role
            });
        }

        [HttpPost("logout")]
        [SessionAuthorize]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out." });
        }

        
        [HttpGet("me")]
        [SessionAuthorize]
        public IActionResult Me()
        {
            return Ok(new
            {
                userId = HttpContext.Session.GetInt32("UserID"),
                role = HttpContext.Session.GetString("Role")
            });
        }
    }
}