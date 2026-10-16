using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests
{
    public class EnrolmentsControllerTests : TestBase
    {
        [Fact]
        public async Task CreateEnrolment_ValidParticipant_RecordsParticipantEventAndCategoryLink()
        {
            // Arrange
            using var context = CreateSeededDbContext();
            var controller = new EnrolmentsController(context);
            // User 3 (Thabo) is enrolled in Event 1 in seed data, so we enrol him in Event 2, Category 3
            AttachSession(controller, userId: 3, role: "Participant", email: "thabo@gmail.com");

            var dto = new CreateEnrolmentRequestDto
            {
                EventId = 2,
                CategoryId = 3
            };

            // Act
            var result = await controller.CreateEnrolment(dto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status201Created, result.StatusCode);

            var enrolment = Assert.IsType<Enrolment>(result.Value);
            Assert.Equal(3, enrolment.ParticipantID);
            Assert.Equal(2, enrolment.EventID);
            Assert.Equal(3, enrolment.CategoryID);
        }

        [Fact]
        public async Task CreateEnrolment_OrganiserRole_Returns403Forbidden()
        {
            // Arrange
            using var context = CreateSeededDbContext();
            var controller = new EnrolmentsController(context);
            // Organiser attempting Participant-only enrolment functionality
            AttachSession(controller, userId: 1, role: "Organiser", email: "sipho@raceday.co.za");

            var dto = new CreateEnrolmentRequestDto
            {
                EventId = 1,
                CategoryId = 1
            };

            // Act
            var result = await controller.CreateEnrolment(dto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task CreateEnrolment_DuplicateEnrolment_Returns409Conflict()
        {
            // Arrange
            using var context = CreateSeededDbContext();
            var controller = new EnrolmentsController(context);
            // User 3 is already enrolled in Event 1 (Category 1) in seed data
            AttachSession(controller, userId: 3, role: "Participant", email: "thabo@gmail.com");

            var dto = new CreateEnrolmentRequestDto
            {
                EventId = 1,
                CategoryId = 1
            };

            // Act
            var result = await controller.CreateEnrolment(dto);

            // Assert
            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task CreateEnrolment_MismatchedCategoryAndEvent_Returns400BadRequest()
        {
            // Arrange
            using var context = CreateSeededDbContext();
            var controller = new EnrolmentsController(context);
            AttachSession(controller, userId: 4, role: "Participant", email: "jess@gmail.com");

            // Category 3 belongs to Event 2, not Event 1
            var dto = new CreateEnrolmentRequestDto
            {
                EventId = 1,
                CategoryId = 3
            };

            // Act
            var result = await controller.CreateEnrolment(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetEnrolmentsForEvent_ParticipantRole_Returns403Forbidden()
        {
            // Arrange
            using var context = CreateSeededDbContext();
            var controller = new EnrolmentsController(context);
            // Participant attempting Organiser-only view of all event enrolments
            AttachSession(controller, userId: 3, role: "Participant", email: "thabo@gmail.com");

            // Act
            var result = await controller.GetEnrolmentsForEvent(1) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
        }
    }
}