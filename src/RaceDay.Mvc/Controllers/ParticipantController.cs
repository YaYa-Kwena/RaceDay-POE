using Microsoft.AspNetCore.Mvc;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public class ParticipantController : Controller
{
    private readonly RaceDayApiClient _api;
    public ParticipantController(RaceDayApiClient api) => _api = api;

    private bool IsParticipant() => HttpContext.Session.GetInt32("UserID").HasValue;

    // --- 1. UPCOMING EVENTS & FILTERING ---
    public async Task<IActionResult> Index(string? type, string? search)
    {
        if (!IsParticipant()) return RedirectToAction("Login", "Account");

        var response = await _api.SendAsync(HttpMethod.Get, "api/events");
        var events = await _api.ReadAsync<List<Event>>(response) ?? new();

        // Filter out past events
        events = events.Where(e => e.EventDate >= DateTime.Today && e.IsActive).ToList();

        if (!string.IsNullOrWhiteSpace(type))
        {
            events = events.Where(e => string.Equals(e.EventType, type, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            events = events.Where(e =>
                e.EventName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                e.LocationCity.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return View(events);
    }

    // --- 2. EVENT DETAILS & CATEGORY SELECTION ---
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        if (!IsParticipant()) return RedirectToAction("Login", "Account");

        var response = await _api.SendAsync(HttpMethod.Get, $"api/events/{id}");
        var raceEvent = await _api.ReadAsync<Event>(response);

        if (raceEvent == null) return NotFound();

        return View(raceEvent);
    }

    // --- 3. SUBMIT ENROLMENT ---
    [HttpPost]
    public async Task<IActionResult> EnterEvent(int eventId, int categoryId)
    {
        if (!IsParticipant()) return RedirectToAction("Login", "Account");

        var request = new CreateEnrolmentRequestDto
        {
            EventId = eventId,
            CategoryId = categoryId
        };

        var response = await _api.SendAsync(HttpMethod.Post, "api/enrolments", request);
        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = await response.Content.ReadAsStringAsync();
            return RedirectToAction(nameof(Details), new { id = eventId });
        }

        TempData["Success"] = "You have successfully entered the event!";
        return RedirectToAction(nameof(MyEvents));
    }

    // --- 4. MY EVENTS ---
    public async Task<IActionResult> MyEvents()
    {
        if (!IsParticipant()) return RedirectToAction("Login", "Account");

        var response = await _api.SendAsync(HttpMethod.Get, "api/enrolments/me");
        var enrolments = await _api.ReadAsync<List<Enrolment>>(response) ?? new();

        return View(enrolments);
    }

    // --- 5. MY RESULTS ---
    public async Task<IActionResult> MyResults()
    {
        if (!IsParticipant()) return RedirectToAction("Login", "Account");

        var response = await _api.SendAsync(HttpMethod.Get, "api/results/me");
        var results = await _api.ReadAsync<List<MyResultVm>>(response) ?? new();

        return View(results);
    }
}

// View model matching the custom JSON object projection from my API ResultsController
public class MyResultVm
{
    public int ResultID { get; set; }
    public int EnrolmentID { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int RaceNumber { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? OverallPosition { get; set; }
    public int? CategoryPosition { get; set; }
    public string RaceStatus { get; set; } = string.Empty;
    public DateTime CapturedAt { get; set; }
}