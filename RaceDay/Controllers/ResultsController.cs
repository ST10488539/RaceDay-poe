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
    public class ResultsController : ControllerBase
    {
        private static readonly string[] ValidStatuses = { "OFFICIAL", "PROVISIONAL", "DISQUALIFIED" };

        private readonly RaceDayDbContext _db;

        public ResultsController(RaceDayDbContext db)
        {
            _db = db;
        }

        private int CurrentUserId => HttpContext.Session.GetInt32("UserID")!.Value;

        private static IQueryable<ResultDto> Project(IQueryable<Result> query) =>
            query.Select(r => new ResultDto
            {
                ResultID = r.ResultID,
                RegistrationID = r.RegistrationID,
                ParticipantName = r.Registration.Participant.User.FirstName + " " +
                                  r.Registration.Participant.User.LastName,
                EventID = r.Registration.Category.EventID,
                EventName = r.Registration.Category.Event.EventName,
                EventDate = r.Registration.Category.Event.EventDate,
                CategoryName = r.Registration.Category.CategoryName,
                RaceNumber = r.Registration.RaceNumber,
                FinishTime = r.FinishTime,
                ChipTime = r.ChipTime,
                OverallPosition = r.OverallPosition,
                CategoryPosition = r.CategoryPosition,
                Pace = r.Pace,
                ResultStatus = r.ResultStatus
            });

        // Pace in minutes per kilometre, using the chip time when there is one
        private static decimal CalculatePace(TimeSpan time, decimal distanceKm) =>
            Math.Round((decimal)time.TotalMinutes / distanceKm, 2);

        private Task<bool> PositionTaken(int eventId, int overallPosition, int? excludeResultId) =>
            _db.Results.AnyAsync(r =>
                r.Registration.Category.EventID == eventId &&
                r.OverallPosition == overallPosition &&
                r.ResultID != excludeResultId);

        /// <summary>
        /// Records the finish time and positions for one enrolment.
        /// </summary>
        /// <remarks>
        /// Organiser only, and only for events they created. Expects registrationID, finishTime
        /// (hh:mm:ss), overallPosition and categoryPosition; chipTime and resultStatus
        /// (OFFICIAL, PROVISIONAL or DISQUALIFIED) are optional. The pace is calculated automatically.
        /// Returns 201 with the result, 400 for invalid data, 401 if not logged in,
        /// 403 if the user is not an organiser or not the organiser of this event,
        /// 404 if the enrolment does not exist, or 409 if the enrolment already has a result
        /// or the overall position is already taken in this event.
        /// </remarks>
        [HttpPost("results")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> CreateResult(ResultCreateDto dto)
        {
            var registration = await _db.Registrations
                .Include(r => r.Category).ThenInclude(c => c.Event)
                .FirstOrDefaultAsync(r => r.RegistrationID == dto.RegistrationID);

            if (registration == null)
                return NotFound(new { message = "Enrolment not found." });

            if (registration.Category.Event.OrganiserID != CurrentUserId)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You can only record results for your own events." });

            if (dto.FinishTime <= TimeSpan.Zero)
                return BadRequest(new { message = "Finish time must be greater than zero." });

            string status = (dto.ResultStatus ?? "OFFICIAL").Trim().ToUpper();
            if (!ValidStatuses.Contains(status))
                return BadRequest(new { message = "Result status must be OFFICIAL, PROVISIONAL or DISQUALIFIED." });

            if (await _db.Results.AnyAsync(r => r.RegistrationID == dto.RegistrationID))
                return Conflict(new { message = "This enrolment already has a result." });

            if (await PositionTaken(registration.Category.EventID, dto.OverallPosition, null))
                return Conflict(new { message = "That overall position is already taken in this event." });

            var result = new Result
            {
                RegistrationID = dto.RegistrationID,
                FinishTime = dto.FinishTime,
                ChipTime = dto.ChipTime,
                OverallPosition = dto.OverallPosition,
                CategoryPosition = dto.CategoryPosition,
                Pace = CalculatePace(dto.ChipTime ?? dto.FinishTime, registration.Category.Distance),
                ResultStatus = status
            };

            _db.Results.Add(result);
            await _db.SaveChangesAsync();

            var created = await Project(_db.Results.Where(r => r.ResultID == result.ResultID)).FirstAsync();
            return StatusCode(StatusCodes.Status201Created, created);
        }

        /// <summary>
        /// Corrects an existing result.
        /// </summary>
        /// <remarks>
        /// Organiser only, and only for events they created. Expects finishTime, overallPosition
        /// and categoryPosition; chipTime and resultStatus are optional.
        /// Returns 200 with the updated result, 400 for invalid data, 401 if not logged in,
        /// 403 if the user is not the organiser of this event, 404 if the result does not exist,
        /// or 409 if the overall position is already taken in this event.
        /// </remarks>
        [HttpPut("results/{id}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> UpdateResult(int id, ResultUpdateDto dto)
        {
            var result = await _db.Results
                .Include(r => r.Registration).ThenInclude(g => g.Category).ThenInclude(c => c.Event)
                .FirstOrDefaultAsync(r => r.ResultID == id);

            if (result == null)
                return NotFound(new { message = "Result not found." });

            var category = result.Registration.Category;

            if (category.Event.OrganiserID != CurrentUserId)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You can only change results for your own events." });

            if (dto.FinishTime <= TimeSpan.Zero)
                return BadRequest(new { message = "Finish time must be greater than zero." });

            string status = (dto.ResultStatus ?? "OFFICIAL").Trim().ToUpper();
            if (!ValidStatuses.Contains(status))
                return BadRequest(new { message = "Result status must be OFFICIAL, PROVISIONAL or DISQUALIFIED." });

            if (await PositionTaken(category.EventID, dto.OverallPosition, id))
                return Conflict(new { message = "That overall position is already taken in this event." });

            result.FinishTime = dto.FinishTime;
            result.ChipTime = dto.ChipTime;
            result.OverallPosition = dto.OverallPosition;
            result.CategoryPosition = dto.CategoryPosition;
            result.Pace = CalculatePace(dto.ChipTime ?? dto.FinishTime, category.Distance);
            result.ResultStatus = status;

            await _db.SaveChangesAsync();

            var updated = await Project(_db.Results.Where(r => r.ResultID == id)).FirstAsync();
            return Ok(updated);
        }

        /// <summary>
        /// Lists all results for one of the organiser's events, in finishing order.
        /// </summary>
        /// <remarks>
        /// Organiser only, and only for events they created. Returns 200 with the results,
        /// 401 if not logged in, 403 if the user is not an organiser or not the organiser of
        /// this event, or 404 if the event does not exist.
        /// </remarks>
        [HttpGet("events/{eventId}/results")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == eventId);

            if (ev == null)
                return NotFound(new { message = "Event not found." });

            if (ev.OrganiserID != CurrentUserId)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You can only view results for your own events." });

            var list = await Project(_db.Results
                    .Where(r => r.Registration.Category.EventID == eventId))
                .OrderBy(r => r.OverallPosition)
                .ToListAsync();

            return Ok(list);
        }

        /// <summary>
        /// Lists the logged-in participant's own results.
        /// </summary>
        /// <remarks>
        /// Participant only. Returns 200 with the participant's results (an empty list if there
        /// are none yet), 401 if not logged in, or 403 if the user is not a participant.
        /// </remarks>
        [HttpGet("results/my")]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            var list = await Project(_db.Results
                    .Where(r => r.Registration.Participant.UserID == CurrentUserId))
                .OrderBy(r => r.EventDate)
                .ToListAsync();

            return Ok(list);
        }
    }
}