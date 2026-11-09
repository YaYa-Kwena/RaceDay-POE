using RaceDay.Mvc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .ConfigureApplicationPartManager(manager =>
    {
        // Prevent MVC from loading the API's controllers and causing 405 route collisions
        var apiAssembly = manager.ApplicationParts.FirstOrDefault(part => part.Name == "RaceDay.API");
        if (apiAssembly != null)
        {
            manager.ApplicationParts.Remove(apiAssembly);
        }
    });

// Session stores the MVC user's ID and role between page requests
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Crucial: prevents cookie policy from dropping the session on refresh
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddHttpContextAccessor();

// Register named HttpClient pointing to your API
builder.Services.AddHttpClient("RaceDayApi", client =>
{
    string baseUrl = builder.Configuration["ApiSettings:BaseUrl"]
        ?? throw new InvalidOperationException("ApiSettings:BaseUrl is missing.");
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddScoped<RaceDayApiClient>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// CRITICAL: app.UseSession() MUST be placed BEFORE app.UseRouting() and app.UseAuthorization()
app.UseSession();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();