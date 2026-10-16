using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Helpers;
using Swashbuckle.AspNetCore.Annotations;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/profiles")]
    [Produces("application/json")]
    public class ProfilesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ProfilesController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves the logged-in user's profile details.
        /// </summary>
        [HttpGet("me")]
        [SwaggerOperation(
            Summary = "Retrieves the logged-in user's profile details.",
            Description = "Accessible to any authenticated user (Organiser or Participant) via active session.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = SessionAuthHelper.GetUserId(HttpContext.Session);
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "Authentication required. Please log in." });
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return NotFound(new { message = "User profile not found." });
            }

            return Ok(user);
        }

        /// <summary>
        /// Updates the logged-in user's profile information.
        /// </summary>
        [HttpPut("me")]
        [SwaggerOperation(
            Summary = "Updates the logged-in user's profile information.",
            Description = "Allows any authenticated Organiser or Participant to update their phone number, emergency contact details, and medical conditions.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = SessionAuthHelper.GetUserId(HttpContext.Session);
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "Authentication required. Please log in." });
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return NotFound(new { message = "User profile not found." });
            }

            if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;
            if (dto.EmergencyContactName != null) user.EmergencyContactName = dto.EmergencyContactName;
            if (dto.EmergencyContactPhone != null) user.EmergencyContactPhone = dto.EmergencyContactPhone;
            if (dto.MedicalConditions != null) user.MedicalConditions = dto.MedicalConditions;

            await _context.SaveChangesAsync();

            return Ok(user);
        }
    }
}