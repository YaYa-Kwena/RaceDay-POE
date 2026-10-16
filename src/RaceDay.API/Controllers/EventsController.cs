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
    [Route("api/events")]
    [Produces("application/json")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of all upcoming RaceDay events.
        /// </summary>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Retrieves a list of all upcoming RaceDay events.",
            Description = "Returns all active RaceDay events including their categories and routes.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _context.Events
                .Include(e => e.Categories)
                .Include(e => e.EventRoutes)
                .ToListAsync();

            return Ok(events);
        }

        /// <summary>
        /// Returns the details of a specific event using the event ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Returns the details of a specific event using the event ID.",
            Description = "Retrieves full event details, categories, and route metadata for the specified EventID.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventById(int id)
        {
            var raceEvent = await _context.Events
                .Include(e => e.Categories)
                .Include(e => e.EventRoutes)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (raceEvent == null)
            {
                return NotFound(new { message = $"Event with ID {id} was not found." });
            }

            return Ok(raceEvent);
        }

        /// <summary>
        /// Creates a new RaceDay event (Organiser only).
        /// </summary>
        [HttpPost]
        [SwaggerOperation(
            Summary = "Creates a new RaceDay event.",
            Description = "Restricted to the Organiser role. Captures event name, description, event type (Running, Walking, Cycling), distance, date, location, capacity, and deadline.")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequestDto dto)
        {
            if (!SessionAuthHelper.IsAuthenticated(HttpContext.Session))
            {
                return Unauthorized(new { message = "Authentication required. Please log in." });
            }

            if (!SessionAuthHelper.IsOrganiser(HttpContext.Session))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. Only Organisers can create events." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var validTypes = new[] { "Running", "Walking", "Cycling" };
            if (!validTypes.Contains(dto.EventType, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "EventType must be 'Running', 'Walking', or 'Cycling'." });
            }

            var organiserId = SessionAuthHelper.GetUserId(HttpContext.Session)!.Value;

            var newEvent = new Event
            {
                OrganiserID = organiserId,
                EventName = dto.EventName,
                Description = dto.Description,
                EventType = dto.EventType,
                DistanceKm = dto.DistanceKm,
                EventDate = dto.EventDate,
                StartTime = dto.StartTime,
                LocationCity = dto.LocationCity,
                LocationProvince = dto.LocationProvince,
                MaxCapacity = dto.MaxCapacity,
                RegistrationDeadline = dto.RegistrationDeadline,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventById), new { id = newEvent.EventID }, newEvent);
        }

        /// <summary>
        /// Updates an existing event's details (Organiser only).
        /// </summary>
        [HttpPut("{id:int}")]
        [SwaggerOperation(
            Summary = "Updates an existing event's details.",
            Description = "Restricted to the Organiser role. Updates event details for the specified EventID.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventRequestDto dto)
        {
            if (!SessionAuthHelper.IsAuthenticated(HttpContext.Session))
            {
                return Unauthorized(new { message = "Authentication required. Please log in." });
            }

            if (!SessionAuthHelper.IsOrganiser(HttpContext.Session))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. Only Organisers can update events." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingEvent = await _context.Events.FindAsync(id);
            if (existingEvent == null)
            {
                return NotFound(new { message = $"Event with ID {id} was not found." });
            }

            existingEvent.EventName = dto.EventName;
            existingEvent.Description = dto.Description;
            existingEvent.EventType = dto.EventType;
            existingEvent.DistanceKm = dto.DistanceKm;
            existingEvent.EventDate = dto.EventDate;
            existingEvent.StartTime = dto.StartTime;
            existingEvent.LocationCity = dto.LocationCity;
            existingEvent.LocationProvince = dto.LocationProvince;
            existingEvent.MaxCapacity = dto.MaxCapacity;
            existingEvent.RegistrationDeadline = dto.RegistrationDeadline;
            existingEvent.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return Ok(existingEvent);
        }

        /// <summary>
        /// Deletes a specific event (Organiser only).
        /// </summary>
        [HttpDelete("{id:int}")]
        [SwaggerOperation(
            Summary = "Deletes a specific event.",
            Description = "Restricted to the Organiser role. Removes the specified event and its associated categories, routes, and enrolments.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            if (!SessionAuthHelper.IsAuthenticated(HttpContext.Session))
            {
                return Unauthorized(new { message = "Authentication required. Please log in." });
            }

            if (!SessionAuthHelper.IsOrganiser(HttpContext.Session))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. Only Organisers can delete events." });
            }

            var existingEvent = await _context.Events
                .Include(e => e.Enrolments)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (existingEvent == null)
            {
                return NotFound(new { message = $"Event with ID {id} was not found." });
            }

            if (existingEvent.Enrolments.Any())
            {
                _context.Enrolments.RemoveRange(existingEvent.Enrolments);
            }

            _context.Events.Remove(existingEvent);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}