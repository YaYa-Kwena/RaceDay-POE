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
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public AuthController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Registers a new Organiser or Participant with a hashed password.
        /// </summary>
        [HttpPost("register")]
        [SwaggerOperation(
            Summary = "Registers a new Organiser or Participant.",
            Description = "Creates a new user account in the database after hashing the password. Valid roles are 'Organiser' or 'Participant'.")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!string.Equals(dto.Role, "Organiser", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(dto.Role, "Participant", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Role must be either 'Organiser' or 'Participant'." });
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (existingUser != null)
            {
                return Conflict(new { message = "A user with this email address already exists." });
            }

            var normalizedRole = string.Equals(dto.Role, "Organiser", StringComparison.OrdinalIgnoreCase)
                ? "Organiser"
                : "Participant";

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = PasswordHasher.HashPassword(dto.Password),
                UserRole = normalizedRole,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                PhoneNumber = dto.PhoneNumber,
                RegistrationDate = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, new
            {
                message = "User registered successfully.",
                userId = user.UserID,
                email = user.Email,
                role = user.UserRole
            });
        }

        /// <summary>
        /// Authenticates a user and creates a server-side session storing UserID and UserRole.
        /// </summary>
        [HttpPost("login")]
        [SwaggerOperation(
            Summary = "Authenticates a user and starts a server-side session.",
            Description = "Verifies email and hashed password credentials, then stores the UserID and UserRole in the server-side session for subsequent requests.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (user == null || !PasswordHasher.VerifyPassword(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            SessionAuthHelper.SetUserSession(HttpContext.Session, user.UserID, user.UserRole, user.Email);

            return Ok(new
            {
                message = "Login successful. Server-side session established.",
                userId = user.UserID,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                role = user.UserRole
            });
        }

        /// <summary>
        /// Clears the active server-side session for the logged-in user.
        /// </summary>
        [HttpPost("logout")]
        [SwaggerOperation(
            Summary = "Logs out the current user and clears the server-side session.",
            Description = "Requires an active authenticated session. Clears all session keys.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Logout()
        {
            if (!SessionAuthHelper.IsAuthenticated(HttpContext.Session))
            {
                return Unauthorized(new { message = "No active session found. Please log in first." });
            }

            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out successfully. Session cleared." });
        }
    }
}