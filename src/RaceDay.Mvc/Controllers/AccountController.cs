using Microsoft.AspNetCore.Mvc;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public class AccountController : Controller
{
    private readonly RaceDayApiClient _api;

    public AccountController(RaceDayApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        var response = await _api.SendAsync(HttpMethod.Post, "api/auth/login", request);

        if (response.IsSuccessStatusCode)
        {
            var authResult = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (authResult != null)
            {
                // Store user data in Session for the MVC app to use
                HttpContext.Session.SetInt32("UserID", authResult.UserId);
                HttpContext.Session.SetString("UserName", authResult.FirstName);
                HttpContext.Session.SetString("UserRole", authResult.Role);

                // CRITICAL FIX: Commit the session so it persists immediately before redirecting
                await HttpContext.Session.CommitAsync();

                // Redirect based on role
                if (string.Equals(authResult.Role, "Organiser", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Index", "Organiser");
                }

                return RedirectToAction("Index", "Participant");
            }
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your credentials.");
        return View(request);
    }

    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();
        await HttpContext.Session.CommitAsync();
        return RedirectToAction("Index", "Home");
    }
}

// Helper class to match the JSON response from your API's Login method
public class LoginResponse
{
    public string Message { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}