using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using RaceDay.API.Helpers;
using Xunit;

namespace RaceDay.Tests
{
    public class AuthControllerTests : TestBase
    {
        [Fact]
        public async Task Register_ValidParticipant_Returns201CreatedAndHashesPassword()
        {
            // Arrange
            using var context = CreateSeededDbContext();
            var controller = new AuthController(context);
            AttachSession(controller);

            var dto = new RegisterRequestDto
            {
                FirstName = "Kagiso",
                LastName = "Rabada",
                Email = "kagiso@raceday.co.za",
                Password = "SecurePassword123!",
                Role = "Participant",
                Gender = "Male",
                DateOfBirth = new DateTime(1995, 5, 25),
                PhoneNumber = "0829998888"
            };

            // Act
            var result = await controller.Register(dto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status201Created, result.StatusCode);

            var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "kagiso@raceday.co.za");
            Assert.NotNull(savedUser);
            Assert.Equal("Participant", savedUser.UserRole);
            Assert.NotEqual("SecurePassword123!", savedUser.PasswordHash); // Verify password is hashed
            Assert.True(PasswordHasher.VerifyPassword("SecurePassword123!", savedUser.PasswordHash));
        }

        [Fact]
        public async Task Register_DuplicateEmail_Returns409Conflict()
        {
            // Arrange
            using var context = CreateSeededDbContext();
            var controller = new AuthController(context);
            AttachSession(controller);

            // "sipho@raceday.co.za" already exists in seeded data
            var dto = new RegisterRequestDto
            {
                FirstName = "Sipho",
                LastName = "Duplicate",
                Email = "sipho@raceday.co.za",
                Password = "Password123!",
                Role = "Organiser",
                DateOfBirth = new DateTime(1980, 5, 15)
            };

            // Act
            var result = await controller.Register(dto);

            // Assert
            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task Register_InvalidRole_Returns400BadRequest()
        {
            // Arrange
            using var context = CreateSeededDbContext();
            var controller = new AuthController(context);
            AttachSession(controller);

            var dto = new RegisterRequestDto
            {
                FirstName = "Invalid",
                LastName = "RoleUser",
                Email = "invalidrole@raceday.co.za",
                Password = "Password123!",
                Role = "SuperAdmin", // Invalid role
                DateOfBirth = new DateTime(1990, 1, 1)
            };

            // Act
            var result = await controller.Register(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_ValidCredentials_Returns200OkAndSetsServerSession()
        {
            // Arrange
            using var context = CreateSeededDbContext();
            var controller = new AuthController(context);
            AttachSession(controller);

            var loginDto = new LoginRequestDto
            {
                Email = "sipho@raceday.co.za",
                Password = "Password123!"
            };

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.True(SessionAuthHelper.IsAuthenticated(controller.HttpContext.Session));
            Assert.True(SessionAuthHelper.IsOrganiser(controller.HttpContext.Session));
            Assert.Equal(1, SessionAuthHelper.GetUserId(controller.HttpContext.Session));
        }

        [Fact]
        public async Task Login_WrongPassword_Returns401Unauthorized()
        {
            // Arrange
            using var context = CreateSeededDbContext();
            var controller = new AuthController(context);
            AttachSession(controller);

            var loginDto = new LoginRequestDto
            {
                Email = "sipho@raceday.co.za",
                Password = "WrongPassword999!"
            };

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.False(SessionAuthHelper.IsAuthenticated(controller.HttpContext.Session));
        }
    }
}