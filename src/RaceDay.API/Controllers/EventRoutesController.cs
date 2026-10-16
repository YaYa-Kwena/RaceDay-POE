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
    [Route("api/events/{eventId:int}/routes")]
    [Produces("application/json")]
    public class EventRoutesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventRoutesController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves routes for a specific event.
        /// </summary>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Retrieves routes for a specific event.",
            Description = "Returns route distance, elevation gain, and water point details for the specified EventID.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRoutesByEvent(int eventId)
        {
            var eventExists = await _context.Events.AnyAsync(e => e.EventID == eventId);
            if (!eventExists)
            {
                return NotFound(new { message = $"Event with ID {eventId} was not found." });
            }

            var routes = await _context.EventRoutes
                .Where(r => r.EventID == eventId)
                .ToListAsync();

            return Ok(routes);
        }

        /// <summary>
        /// Creates a new route for an event (Organiser only).
        /// </summary>
        [HttpPost]
        [SwaggerOperation(
            Summary = "Creates a new route for an event.",
            Description = "Restricted to the Organiser role. Adds a route with distance, elevation gain, and water point metadata to an event.")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateRoute(int eventId, [FromBody] CreateRouteRequestDto dto)
        {
            if (!SessionAuthHelper.IsAuthenticated(HttpContext.Session))
            {
                return Unauthorized(new { message = "Authentication required. Please log in." });
            }

            if (!SessionAuthHelper.IsOrganiser(HttpContext.Session))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. Only Organisers can create event routes." });
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

            var route = new EventRoute
            {
                EventID = eventId,
                RouteName = dto.RouteName,
                TotalDistanceKm = dto.TotalDistanceKm,
                ElevationGainMeters = dto.ElevationGainMeters,
                WaterPointsCount = dto.WaterPointsCount,
                RouteMapUrl = dto.RouteMapUrl
            };

            _context.EventRoutes.Add(route);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, route);
        }
    }
}