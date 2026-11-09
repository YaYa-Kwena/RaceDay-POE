using Microsoft.AspNetCore.Mvc;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public class OrganiserController : Controller
{
    private readonly RaceDayApiClient _api;
    public OrganiserController(RaceDayApiClient api) => _api = api;

    private bool IsOrganiser() =>
        string.Equals(HttpContext.Session.GetString("UserRole"), "Organiser", StringComparison.OrdinalIgnoreCase);

    // --- 1. DASHBOARD & EVENTS LIST ---
    public async Task<IActionResult> Index()
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

        int? organiserId = HttpContext.Session.GetInt32("UserID");

        var response = await _api.SendAsync(HttpMethod.Get, "api/events");
        var allEvents = await _api.ReadAsync<List<Event>>(response) ?? new();

        // Filter to show only events owned by this Organiser
        var myEvents = allEvents.Where(e => e.OrganiserID == organiserId).ToList();

        return View(myEvents);
    }

    // --- 2. CREATE EVENT (GET) ---
    [HttpGet]
    public IActionResult CreateEvent()
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
        return View();
    }

    // --- 3. CREATE EVENT (POST) ---
    [HttpPost]
    public async Task<IActionResult> CreateEvent(CreateEventRequestDto dto, IFormFile? banner)
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
        if (!ModelState.IsValid) return View(dto);

        // 1. Create the event first via API
        var response = await _api.SendAsync(HttpMethod.Post, "api/events", dto);
        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Failed to create event: {error}");
            return View(dto);
        }

        // 2. If a banner file was uploaded, read the created event ID and upload the banner through the API
        if (banner != null && banner.Length > 0)
        {
            // Read back the created event to get its ID (or adapt based on your API's creation response)
            var createdEvent = await _api.ReadAsync<Event>(response);
            if (createdEvent != null)
            {
                await _api.UploadFileAsync($"api/events/{createdEvent.EventID}/banner", banner);
            }
        }

        return RedirectToAction(nameof(Index));
    }

    // --- 4. VIEW ENROLMENTS & CAPTURE RESULTS ---
    [HttpGet]
    public async Task<IActionResult> Enrolments(int eventId)
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

        var response = await _api.SendAsync(HttpMethod.Get, $"api/events/{eventId}/enrolments");
        var enrolments = await _api.ReadAsync<List<Enrolment>>(response) ?? new();

        ViewBag.EventId = eventId;
        return View(enrolments);
    }

    [HttpPost]
    public async Task<IActionResult> CaptureResult(int enrolmentId, int eventId, CaptureResultRequestDto dto)
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

        var response = await _api.SendAsync(HttpMethod.Post, $"api/enrolments/{enrolmentId}/results", dto);
        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = await response.Content.ReadAsStringAsync();
        }
        else
        {
            TempData["Success"] = "Result captured successfully.";
        }

        return RedirectToAction(nameof(Enrolments), new { eventId });
    }
}