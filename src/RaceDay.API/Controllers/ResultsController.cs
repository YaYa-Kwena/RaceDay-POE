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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Captures the finish time and position for a participant's enrolment (Organiser only).
        /// </summary>
        [HttpPost("api/enrolments/{enrolmentId:int}/results")]
        [SwaggerOperation(
            Summary = "Captures the finish time and position for a participant's enrolment.",
            Description = "Restricted to the Organiser role. Records official finish time, overall position, category position, and race status.")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CaptureResult(int enrolmentId, [FromBody] CaptureResultRequestDto dto)
        {
            if (!SessionAuthHelper.IsAuthenticated(HttpContext.Session))
            {
                return Unauthorized(new { message = "Authentication required. Please log in." });
            }

            if (!SessionAuthHelper.IsOrganiser(HttpContext.Session))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. Only Organisers can capture race results." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var enrolment = await _context.Enrolments
                .Include(e => e.Result)
                .FirstOrDefaultAsync(e => e.EnrolmentID == enrolmentId);

            if (enrolment == null)
            {
                return NotFound(new { message = $"Enrolment with ID {enrolmentId} was not found." });
            }

            if (enrolment.Result != null)
            {
                return Conflict(new { message = "A result has already been captured for this enrolment." });
            }

            var result = new Result
            {
                EnrolmentID = enrolmentId,
                FinishTime = dto.FinishTime,
                OverallPosition = dto.OverallPosition,
                CategoryPosition = dto.CategoryPosition,
                RaceStatus = dto.RaceStatus,
                CapturedAt = DateTime.UtcNow
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>
        /// Updates an existing captured result (Organiser only).
        /// </summary>
        [HttpPut("api/results/{id:int}")]
        [SwaggerOperation(
            Summary = "Updates an existing captured result.",
            Description = "Restricted to the Organiser role. Modifies finish time, positions, or status for an existing result record.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateResult(int id, [FromBody] CaptureResultRequestDto dto)
        {
            if (!SessionAuthHelper.IsAuthenticated(HttpContext.Session))
            {
                return Unauthorized(new { message = "Authentication required. Please log in." });
            }

            if (!SessionAuthHelper.IsOrganiser(HttpContext.Session))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. Only Organisers can update race results." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _context.Results.FindAsync(id);
            if (result == null)
            {
                return NotFound(new { message = $"Result with ID {id} was not found." });
            }

            result.FinishTime = dto.FinishTime;
            result.OverallPosition = dto.OverallPosition;
            result.CategoryPosition = dto.CategoryPosition;
            result.RaceStatus = dto.RaceStatus;

            await _context.SaveChangesAsync();

            return Ok(result);
        }

        /// <summary>
        /// Retrieves all race results and performance history for the logged-in participant (Participant only).
        /// </summary>
        [HttpGet("api/results/me")]
        [SwaggerOperation(
            Summary = "Retrieves all race results and performance history for the logged-in participant.",
            Description = "Restricted to the Participant role. Returns all captured race results linked to the participant's enrolments.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyResults()
        {
            if (!SessionAuthHelper.IsAuthenticated(HttpContext.Session))
            {
                return Unauthorized(new { message = "Authentication required. Please log in." });
            }

            if (!SessionAuthHelper.IsParticipant(HttpContext.Session))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Forbidden. Only Participants can view personal race results." });
            }

            var participantId = SessionAuthHelper.GetUserId(HttpContext.Session)!.Value;

            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Event)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Category)
                .Where(r => r.Enrolment != null && r.Enrolment.ParticipantID == participantId)
                .Select(r => new
                {
                    r.ResultID,
                    r.EnrolmentID,
                    EventName = r.Enrolment!.Event != null ? r.Enrolment.Event.EventName : string.Empty,
                    CategoryName = r.Enrolment.Category != null ? r.Enrolment.Category.CategoryName : string.Empty,
                    r.Enrolment.RaceNumber,
                    r.FinishTime,
                    r.OverallPosition,
                    r.CategoryPosition,
                    r.RaceStatus,
                    r.CapturedAt
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}