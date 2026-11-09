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
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _db;

        public CategoriesController(RaceDayDbContext db)
        {
            _db = db;
        }

        private int CurrentUserId => HttpContext.Session.GetInt32("UserID")!.Value;

        private static CategoryDto ToDto(Category c) => new CategoryDto
        {
            CategoryID = c.CategoryID,
            EventID = c.EventID,
            CategoryName = c.CategoryName,
            Distance = c.Distance,
            MinAge = c.MinAge,
            MaxAge = c.MaxAge,
            Description = c.Description
        };

        /// <summary>
        /// Lists the categories available for an event.
        /// </summary>
        /// <remarks>
        /// Requires login (Organiser or Participant).
        /// Returns 200 with the categories, 401 if not logged in, or 404 if the event does not exist.
        /// </remarks>
        [HttpGet("events/{eventId}/categories")]
        public async Task<IActionResult> GetCategoriesForEvent(int eventId)
        {
            if (!await _db.Events.AnyAsync(e => e.EventID == eventId))
                return NotFound(new { message = "Event not found." });

            var categories = await _db.Categories
                .Where(c => c.EventID == eventId)
                .OrderBy(c => c.Distance).ThenBy(c => c.MinAge)
                .ToListAsync();

            return Ok(categories.Select(ToDto));
        }

        /// <summary>
        /// Returns one category by its id.
        /// </summary>
        /// <remarks>
        /// Requires login (Organiser or Participant).
        /// Returns 200 with the category, 401 if not logged in, or 404 if it does not exist.
        /// </remarks>
        [HttpGet("categories/{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _db.Categories.FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            return Ok(ToDto(category));
        }

        /// <summary>
        /// Adds a category (for example a distance or age group) to an event.
        /// </summary>
        /// <remarks>
        /// Organiser only, and only for events they created.
        /// Expects categoryName, distance and minAge; maxAge and description are optional.
        /// Returns 201 with the new category, 400 for invalid data, 401 if not logged in,
        /// 403 if the user is not an organiser or not the organiser of this event,
        /// or 404 if the event does not exist.
        /// </remarks>
        [HttpPost("events/{eventId}/categories")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> CreateCategory(int eventId, CategoryRequestDto dto)
        {
            var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == eventId);

            if (ev == null)
                return NotFound(new { message = "Event not found." });

            if (ev.OrganiserID != CurrentUserId)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You can only add categories to your own events." });

            if (dto.MaxAge != null && dto.MaxAge < dto.MinAge)
                return BadRequest(new { message = "Maximum age cannot be less than minimum age." });

            var category = new Category
            {
                EventID = eventId,
                CategoryName = dto.CategoryName.Trim(),
                Distance = dto.Distance,
                MinAge = dto.MinAge,
                MaxAge = dto.MaxAge,
                Description = dto.Description
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.CategoryID }, ToDto(category));
        }

        /// <summary>
        /// Updates a category. Only the organiser of the event can change it.
        /// </summary>
        /// <remarks>
        /// Organiser only. Expects the same fields as creating a category.
        /// Returns 200 with the updated category, 400 for invalid data, 401 if not logged in,
        /// 403 if the user is not the organiser of the event, or 404 if the category does not exist.
        /// </remarks>
        [HttpPut("categories/{id}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> UpdateCategory(int id, CategoryRequestDto dto)
        {
            var category = await _db.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            if (category.Event.OrganiserID != CurrentUserId)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You can only change categories of your own events." });

            if (dto.MaxAge != null && dto.MaxAge < dto.MinAge)
                return BadRequest(new { message = "Maximum age cannot be less than minimum age." });

            category.CategoryName = dto.CategoryName.Trim();
            category.Distance = dto.Distance;
            category.MinAge = dto.MinAge;
            category.MaxAge = dto.MaxAge;
            category.Description = dto.Description;

            await _db.SaveChangesAsync();

            return Ok(ToDto(category));
        }

        /// <summary>
        /// Deletes a category. Only the organiser of the event can delete it.
        /// </summary>
        /// <remarks>
        /// Organiser only. Returns 204 on success, 401 if not logged in,
        /// 403 if the user is not the organiser of the event, 404 if it does not exist,
        /// or 409 if participants are already enrolled in the category.
        /// </remarks>
        [HttpDelete("categories/{id}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _db.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            if (category.Event.OrganiserID != CurrentUserId)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You can only delete categories of your own events." });

            if (await _db.Registrations.AnyAsync(r => r.CategoryID == id))
                return Conflict(new { message = "This category has enrolments and cannot be deleted." });

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}