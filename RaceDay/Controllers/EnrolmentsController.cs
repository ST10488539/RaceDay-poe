using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.DTOs;
using RaceDay.Filters;
using RaceDay.Models;

namespace RaceDay.Controllers
{
    [ApiController]
    [Route("api")]
    [SessionAuthorize]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _db;

        public EnrolmentsController(RaceDayDbContext db)
        {
            _db = db;
        }

        private int CurrentUserId => HttpContext.Session.GetInt32("UserID")!.Value;

        private static IQueryable<EnrolmentDto> Project(IQueryable<Registration> query) =>
            query.Select(r => new EnrolmentDto
            {
                RegistrationID = r.RegistrationID,
                ParticipantID = r.ParticipantID,
                ParticipantName = r.Participant.User.FirstName + " " + r.Participant.User.LastName,
                EventID = r.Category.EventID,
                EventName = r.Category.Event.EventName,
                EventDate = r.Category.Event.EventDate,
                CategoryID = r.CategoryID,
                CategoryName = r.Category.CategoryName,
                RaceNumber = r.RaceNumber,
                RegistrationDate = r.RegistrationDate,
                RegistrationStatus = r.RegistrationStatus,
                AmountPaid = r.AmountPaid
            });

        private static int AgeOn(DateTime dateOfBirth, DateTime onDate)
        {
            int age = onDate.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > onDate.Date.AddYears(-age)) age--;
            return age;
        }

        /// <summary>
        /// Enters the logged-in participant into an event by choosing a category.
        /// </summary>
        /// <remarks>
        /// Participant only. Expects categoryID. The participant's age on the event day must
        /// fit the category's age range, and a participant can enter each event only once.
        /// Returns 201 with the enrolment (including a race number), 400 if the age does not
        /// fit or the event has passed, 401 if not logged in, 403 if the user is not a
        /// participant, 404 if the category does not exist, or 409 if already enrolled.
        /// </remarks>
        [HttpPost("enrolments")]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> Enrol(EnrolRequestDto dto)
        {
            var participant = await _db.Participants
                .FirstOrDefaultAsync(p => p.UserID == CurrentUserId);

            if (participant == null)
                return BadRequest(new { message = "No participant profile found for this account." });

            var category = await _db.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryID == dto.CategoryID);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            if (category.Event.EventDate.Date < DateTime.UtcNow.Date)
                return BadRequest(new { message = "This event has already taken place." });

            int age = AgeOn(participant.DateOfBirth, category.Event.EventDate);
            if (age < category.MinAge || (category.MaxAge != null && age > category.MaxAge))
                return BadRequest(new
                {
                    message = $"You do not meet the age requirement for this category (age on event day: {age})."
                });

            bool alreadyEntered = await _db.Registrations.AnyAsync(r =>
                r.ParticipantID == participant.ParticipantID &&
                r.Category.EventID == category.EventID);

            if (alreadyEntered)
                return Conflict(new { message = "You are already enrolled in this event." });

            int existing = await _db.Registrations
                .CountAsync(r => r.Category.EventID == category.EventID);

            var registration = new Registration
            {
                ParticipantID = participant.ParticipantID,
                CategoryID = category.CategoryID,
                RaceNumber = (101 + existing).ToString(),
                RegistrationStatus = "Registered",
                AmountPaid = 0
            };

            _db.Registrations.Add(registration);
            await _db.SaveChangesAsync();

            var result = await Project(_db.Registrations
                .Where(r => r.RegistrationID == registration.RegistrationID)).FirstAsync();

            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>
        /// Lists the logged-in participant's own enrolments.
        /// </summary>
        /// <remarks>
        /// Participant only. Returns 200 with the enrolments, 401 if not logged in,
        /// or 403 if the user is not a participant.
        /// </remarks>
        [HttpGet("enrolments/my")]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var list = await Project(_db.Registrations
                    .Where(r => r.Participant.UserID == CurrentUserId))
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            return Ok(list);
        }

        /// <summary>
        /// Lists everyone enrolled in one of the organiser's events.
        /// </summary>
        /// <remarks>
        /// Organiser only, and only for events they created. Returns 200 with the enrolments,
        /// 401 if not logged in, 403 if the user is not an organiser or not the organiser of
        /// this event, or 404 if the event does not exist.
        /// </remarks>
        [HttpGet("events/{eventId}/enrolments")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
        {
            var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == eventId);

            if (ev == null)
                return NotFound(new { message = "Event not found." });

            if (ev.OrganiserID != CurrentUserId)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You can only view enrolments for your own events." });

            var list = await Project(_db.Registrations
                    .Where(r => r.Category.EventID == eventId))
                .OrderBy(e => e.RegistrationID)
                .ToListAsync();

            return Ok(list);
        }
    }
}