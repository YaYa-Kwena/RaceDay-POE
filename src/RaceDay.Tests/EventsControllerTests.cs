using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests
{
    public class EventsControllerTests : TestBase
    {
        [Fact]
        public async Task CreateEvent_AuthenticatedOrganiser_Returns201Created()
        {
            // Arrange
            using var context = CreateSeededDbContext();
            var controller = new EventsController(context);
            AttachSession(controller, userId: 1, role: "Organiser", email: "sipho@raceday.co.za");

            var dto = new CreateEventRequestDto
            {
                EventName = "Two Oceans Ultra Marathon",
                Description = "World's most beautiful marathon in Cape Town.",
                EventType = "Running",
                DistanceKm = 56.00m,
                EventDate = new DateTime(2026, 4, 11),
                StartTime = new TimeSpan(5, 15, 0),
                LocationCity = "Cape Town",
                LocationProvince = "Western Cape",
                MaxCapacity = 16000,
                RegistrationDeadline = new DateTime(2026, 3, 15)
            };

            // Act
            var result = await controller.CreateEvent(dto) as CreatedAtActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status201Created, result.StatusCode);

            var createdEvent = Assert.IsType<Event>(result.Value);
            Assert.Equal("Two Oceans Ultra Marathon", createdEvent.EventName);
            Assert.Equal(56.00m, createdEvent.DistanceKm);
            Assert.Equal(1, createdEvent.OrganiserID);
        }

        [Fact]
        public async Task CreateEvent_UnauthenticatedRequest_Returns401Unauthorized()
        {
            // Arrange
            using var context = CreateSeededDbContext();
            var controller = new EventsController(context);
            AttachSession(controller); // Unauthenticated session

            var dto = new CreateEventRequestDto
            {
                EventName = "Unauthorized Event",
                Description = "Should fail with 401.",
                EventType = "Walking",
                DistanceKm = 10.0m,
                EventDate = DateTime.UtcNow.AddDays(30),
                StartTime = new TimeSpan(7, 0, 0),
                LocationCity = "Pretoria",
                LocationProvince = "Gauteng",
                MaxCapacity = 500,
                RegistrationDeadline = DateTime.UtcNow.AddDays(15)
            };

            // Act
            var result = await controller.CreateEvent(dto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task CreateEvent_ParticipantRole_Returns403Forbidden()
        {
            // Arrange
            using var context = CreateSeededDbContext();
            var controller = new EventsController(context);
            // Logged in as Participant (User 3: Thabo) attempting Organiser functionality
            AttachSession(controller, userId: 3, role: "Participant", email: "thabo@gmail.com");

            var dto = new CreateEventRequestDto
            {
                EventName = "Participant Forbidden Event",
                Description = "Participants cannot create events.",
                EventType = "Running",
                DistanceKm = 21.1m,
                EventDate = DateTime.UtcNow.AddDays(30),
                StartTime = new TimeSpan(6, 0, 0),
                LocationCity = "Johannesburg",
                LocationProvince = "Gauteng",
                MaxCapacity = 1000,
                RegistrationDeadline = DateTime.UtcNow.AddDays(20)
            };

            // Act
            var result = await controller.CreateEvent(dto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task UpdateEvent_OrganiserRole_UpdatesEventSuccessfully()
        {
            // Arrange
            using var context = CreateSeededDbContext();
            var controller = new EventsController(context);
            AttachSession(controller, userId: 1, role: "Organiser");

            var updateDto = new UpdateEventRequestDto
            {
                EventName = "Soweto Marathon Updated",
                Description = "Updated description for Soweto Marathon.",
                EventType = "Running",
                DistanceKm = 42.20m,
                EventDate = new DateTime(2026, 11, 1),
                StartTime = new TimeSpan(5, 30, 0),
                LocationCity = "Johannesburg",
                LocationProvince = "Gauteng",
                MaxCapacity = 30000,
                RegistrationDeadline = new DateTime(2026, 10, 15),
                IsActive = true
            };

            // Act
            var result = await controller.UpdateEvent(1, updateDto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var updatedEvent = Assert.IsType<Event>(result.Value);
            Assert.Equal("Soweto Marathon Updated", updatedEvent.EventName);
            Assert.Equal(30000, updatedEvent.MaxCapacity);
        }

        [Fact]
        public async Task DeleteEvent_ParticipantRole_Returns403Forbidden()
        {
            // Arrange
            using var context = CreateSeededDbContext();
            var controller = new EventsController(context);
            AttachSession(controller, userId: 3, role: "Participant");

            // Act
            var result = await controller.DeleteEvent(1) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
        }
    }
}