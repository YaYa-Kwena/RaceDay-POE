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
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Enrols a participant into a specific event category (Participant only).
        /// </summary>
        [HttpPost("api/enrolments")]
        [SwaggerOperation(
            Summary = "Enrols a participant into a specific event category.",
            Description = "Restricted to the Participant role. Records the link between the logged-in Participant, the Event, and the selected Category.")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateEnrolment([FromBody] CreateEnrolmentRequestDto dto)
        {
            if (!SessionAuthHelper.IsAuthenticated(HttpContext.Session))
            {
                return Unauthorized(new { message = "Authentication required. Please log in." });
            }

            if (!SessionAuthHelper.IsParticipant(HttpContext.Session))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. Only Participants can enrol in events." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var raceEvent = await _context.Events.FindAsync(dto.EventId);
            if (raceEvent == null)
            {
                return NotFound(new { message = $"Event with ID {dto.EventId} was not found." });
            }

            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category == null)
            {
                return NotFound(new { message = $"Category with ID {dto.CategoryId} was not found." });
            }

            if (category.EventID != dto.EventId)
            {
                return BadRequest(new { message = "The selected category does not belong to the specified event." });
            }

            var participantId = SessionAuthHelper.GetUserId(HttpContext.Session)!.Value;

            var alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e => e.ParticipantID == participantId && e.EventID == dto.EventId);

            if (alreadyEnrolled)
            {
                return Conflict(new { message = "Participant is already enrolled in this event." });
            }

            var maxRaceNumber = await _context.Enrolments.AnyAsync()
                ? await _context.Enrolments.MaxAsync(e => e.RaceNumber)
                : 1000;

            var enrolment = new Enrolment
            {
                ParticipantID = participantId,
                EventID = dto.EventId,
                CategoryID = dto.CategoryId,
                RaceNumber = maxRaceNumber + 1,
                PaymentStatus = "Paid",
                EnrolmentDate = DateTime.UtcNow
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, enrolment);
        }

        /// <summary>
        /// Retrieves all event enrolments for the logged-in participant (Participant only).
        /// </summary>
        [HttpGet("api/enrolments/me")]
        [SwaggerOperation(
            Summary = "Retrieves all event enrolments for the logged-in participant.",
            Description = "Restricted to the Participant role. Returns all enrolments and associated event/category details for the current user.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyEnrolments()
        {
            if (!SessionAuthHelper.IsAuthenticated(HttpContext.Session))
            {
                return Unauthorized(new { message = "Authentication required. Please log in." });
            }

            if (!SessionAuthHelper.IsParticipant(HttpContext.Session))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. Only Participants can view personal enrolments." });
            }

            var participantId = SessionAuthHelper.GetUserId(HttpContext.Session)!.Value;

            var enrolments = await _context.Enrolments
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Include(e => e.Result)
                .Where(e => e.ParticipantID == participantId)
                .ToListAsync();

            return Ok(enrolments);
        }

        /// <summary>
        /// Retrieves all enrolments for a specific event (Organiser only).
        /// </summary>
        [HttpGet("api/events/{eventId:int}/enrolments")]
        [SwaggerOperation(
            Summary = "Retrieves all enrolments for a specific event managed by the organiser.",
            Description = "Restricted to the Organiser role. Returns all participant enrolments for the specified EventID.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEnrolmentsForEvent(int eventId)
        {
            if (!SessionAuthHelper.IsAuthenticated(HttpContext.Session))
            {
                return Unauthorized(new { message = "Authentication required. Please log in." });
            }

            if (!SessionAuthHelper.IsOrganiser(HttpContext.Session))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. Only Organisers can view event enrolments." });
            }

            var eventExists = await _context.Events.AnyAsync(e => e.EventID == eventId);
            if (!eventExists)
            {
                return NotFound(new { message = $"Event with ID {eventId} was not found." });
            }

            var enrolments = await _context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .Include(e => e.Result)
                .Where(e => e.EventID == eventId)
                .ToListAsync();

            return Ok(enrolments);
        }
    }
}