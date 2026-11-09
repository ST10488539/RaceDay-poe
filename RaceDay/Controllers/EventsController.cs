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
    [Route("api/events")]
    [SessionAuthorize]
    public class EventsController : ControllerBase
    {
        private static readonly string[] ValidTypes = { "RUN", "WALK", "CYCLE" };

        private readonly RaceDayDbContext _db;
        private readonly IBlobStorageService _blobs;

        public EventsController(RaceDayDbContext db, IBlobStorageService blobs)
        {
            _db = db;
            _blobs = blobs;
        }

        private int CurrentUserId => HttpContext.Session.GetInt32("UserID")!.Value;

        private static EventDto ToDto(Event e) => new EventDto
        {
            EventID = e.EventID,
            EventName = e.EventName,
            EventDate = e.EventDate,
            Location = e.Location,
            Description = e.Description,
            Distance = e.Distance,
            EventType = e.EventType,
            ImageURL = e.ImageURL,
            OrganiserID = e.OrganiserID,
            OrganiserName = $"{e.Organiser.FirstName} {e.Organiser.LastName}"
        };

        /// <summary>
        /// Lists all events, soonest first.
        /// </summary>
        /// <remarks>
        /// Requires login (Organiser or Participant).
        /// Returns 200 with the list of events, or 401 if not logged in.
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _db.Events
                .Include(e => e.Organiser)
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            return Ok(events.Select(ToDto));
        }

        /// <summary>
        /// Returns one event by its id.
        /// </summary>
        /// <remarks>
        /// Requires login (Organiser or Participant).
        /// Returns 200 with the event, 401 if not logged in, or 404 if the event does not exist.
        /// </remarks>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var ev = await _db.Events
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (ev == null)
                return NotFound(new { message = "Event not found." });

            return Ok(ToDto(ev));
        }

        /// <summary>
        /// Creates a new event owned by the logged-in organiser.
        /// </summary>
        /// <remarks>
        /// Organiser only. Expects eventName, eventDate, location, distance and eventType
        /// (RUN, WALK or CYCLE); description and imageURL are optional.
        /// Returns 201 with the new event, 400 for invalid data, 401 if not logged in,
        /// or 403 if the user is not an organiser.
        /// </remarks>
        [HttpPost]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> CreateEvent(EventRequestDto dto)
        {
            string type = dto.EventType.Trim().ToUpper();
            if (!ValidTypes.Contains(type))
                return BadRequest(new { message = "Event type must be RUN, WALK or CYCLE." });

            var ev = new Event
            {
                EventName = dto.EventName.Trim(),
                EventDate = dto.EventDate.Date,
                Location = dto.Location.Trim(),
                Description = dto.Description,
                Distance = dto.Distance,
                EventType = type,
                ImageURL = dto.ImageURL,
                OrganiserID = CurrentUserId
            };

            _db.Events.Add(ev);
            await _db.SaveChangesAsync();
            await _db.Entry(ev).Reference(e => e.Organiser).LoadAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = ev.EventID }, ToDto(ev));
        }

        /// <summary>
        /// Updates an event. Only the organiser who created it can change it.
        /// </summary>
        /// <remarks>
        /// Organiser only. Expects the same fields as creating an event. An existing banner image
        /// is kept unless a new imageURL is sent.
        /// Returns 200 with the updated event, 400 for invalid data, 401 if not logged in,
        /// 403 if the user is not the organiser of this event, or 404 if it does not exist.
        /// </remarks>
        [HttpPut("{id}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> UpdateEvent(int id, EventRequestDto dto)
        {
            var ev = await _db.Events
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (ev == null)
                return NotFound(new { message = "Event not found." });

            if (ev.OrganiserID != CurrentUserId)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You can only change your own events." });

            string type = dto.EventType.Trim().ToUpper();
            if (!ValidTypes.Contains(type))
                return BadRequest(new { message = "Event type must be RUN, WALK or CYCLE." });

            ev.EventName = dto.EventName.Trim();
            ev.EventDate = dto.EventDate.Date;
            ev.Location = dto.Location.Trim();
            ev.Description = dto.Description;
            ev.Distance = dto.Distance;
            ev.EventType = type;

            if (dto.ImageURL != null)
                ev.ImageURL = dto.ImageURL;

            await _db.SaveChangesAsync();

            return Ok(ToDto(ev));
        }

        /// <summary>
        /// Uploads or replaces the banner image of an event.
        /// </summary>
        /// <remarks>
        /// Organiser only, and only for events they created. Send multipart/form-data with one file
        /// field named "file" (JPG, PNG or WEBP, up to 5 MB). The image is stored in Azure Blob Storage.
        /// Returns 200 with the updated event, 400 if the file is not a valid image, 401 if not logged in,
        /// 403 if the user is not the organiser of this event, or 404 if the event does not exist.
        /// </remarks>
        [HttpPost("{id}/image")]
        [SessionAuthorize("Organiser")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadEventImage(int id, IFormFile file)
        {
            var ev = await _db.Events
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (ev == null)
                return NotFound(new { message = "Event not found." });

            if (ev.OrganiserID != CurrentUserId)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You can only change images for your own events." });

            string? error = _blobs.Validate(file);
            if (error != null)
                return BadRequest(new { message = error });

            ev.ImageURL = await _blobs.UploadImageAsync(file, "events");
            await _db.SaveChangesAsync();

            return Ok(ToDto(ev));
        }

        /// <summary>
        /// Deletes an event. Only the organiser who created it can delete it.
        /// </summary>
        /// <remarks>
        /// Organiser only. Returns 204 on success, 401 if not logged in,
        /// 403 if the user is not the organiser of this event, 404 if it does not exist,
        /// or 409 if participants are already enrolled in the event.
        /// </remarks>
        [HttpDelete("{id}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == id);

            if (ev == null)
                return NotFound(new { message = "Event not found." });

            if (ev.OrganiserID != CurrentUserId)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You can only delete your own events." });

            bool hasEnrolments = await _db.Registrations
                .AnyAsync(r => r.Category.EventID == id);

            if (hasEnrolments)
                return Conflict(new { message = "This event has enrolments and cannot be deleted." });

            _db.Events.Remove(ev);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
