using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Helpers;
using RaceDay.API.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all categories for a specific event.
        /// </summary>
        [HttpGet("api/events/{eventId:int}/categories")]
        [SwaggerOperation(
            Summary = "Retrieves all categories for a specific event.",
            Description = "Returns all age and distance categories linked to the specified EventID.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategoriesByEvent(int eventId)
        {
            var eventExists = await _context.Events.AnyAsync(e => e.EventID == eventId);
            if (!eventExists)
            {
                return NotFound(new { message = $"Event with ID {eventId} was not found." });
            }

            var categories = await _context.Categories
                .Where(c => c.EventID == eventId)
                .ToListAsync();

            return Ok(categories);
        }

        /// <summary>
        /// Creates a new category for a specific event (Organiser only).
        /// </summary>
        [HttpPost("api/events/{eventId:int}/categories")]
        [SwaggerOperation(
            Summary = "Creates a new category for a specific event.",
            Description = "Restricted to the Organiser role. Creates a new distance or age category for an event.")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateCategory(int eventId, [FromBody] CreateCategoryRequestDto dto)
        {
            if (!SessionAuthHelper.IsAuthenticated(HttpContext.Session))
            {
                return Unauthorized(new { message = "Authentication required. Please log in." });
            }

            if (!SessionAuthHelper.IsOrganiser(HttpContext.Session))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. Only Organisers can create categories." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var eventExists = await _context.Events.AnyAsync(e => e.EventID == eventId);
            if (!eventExists)
            {
                return NotFound(new { message = $"Event with ID {eventId} was not found." });
            }

            var category = new Category
            {
                EventID = eventId,
                CategoryName = dto.CategoryName,
                DistanceKm = dto.DistanceKm,
                EntryFeeZAR = dto.EntryFeeZAR,
                MinAge = dto.MinAge,
                MaxAge = dto.MaxAge,
                GenderRestriction = dto.GenderRestriction
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, category);
        }

        /// <summary>
        /// Updates an existing event category (Organiser only).
        /// </summary>
        [HttpPut("api/categories/{id:int}")]
        [SwaggerOperation(
            Summary = "Updates an existing event category.",
            Description = "Restricted to the Organiser role. Updates category details for the specified CategoryID.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequestDto dto)
        {
            if (!SessionAuthHelper.IsAuthenticated(HttpContext.Session))
            {
                return Unauthorized(new { message = "Authentication required. Please log in." });
            }

            if (!SessionAuthHelper.IsOrganiser(HttpContext.Session))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. Only Organisers can update categories." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new { message = $"Category with ID {id} was not found." });
            }

            category.CategoryName = dto.CategoryName;
            category.DistanceKm = dto.DistanceKm;
            category.EntryFeeZAR = dto.EntryFeeZAR;
            category.MinAge = dto.MinAge;
            category.MaxAge = dto.MaxAge;
            category.GenderRestriction = dto.GenderRestriction;

            await _context.SaveChangesAsync();

            return Ok(category);
        }

        /// <summary>
        /// Deletes a specific event category (Organiser only).
        /// </summary>
        [HttpDelete("api/categories/{id:int}")]
        [SwaggerOperation(
            Summary = "Deletes a specific event category.",
            Description = "Restricted to the Organiser role. Removes the category with the specified CategoryID.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!SessionAuthHelper.IsAuthenticated(HttpContext.Session))
            {
                return Unauthorized(new { message = "Authentication required. Please log in." });
            }

            if (!SessionAuthHelper.IsOrganiser(HttpContext.Session))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. Only Organisers can delete categories." });
            }

            var category = await _context.Categories
                .Include(c => c.Enrolments)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound(new { message = $"Category with ID {id} was not found." });
            }

            if (category.Enrolments.Any())
            {
                _context.Enrolments.RemoveRange(category.Enrolments);
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}