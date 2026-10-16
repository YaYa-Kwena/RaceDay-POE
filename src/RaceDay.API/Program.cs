using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RaceDay.API.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Register Controllers
builder.Services.AddControllers();

// 2. Register EF Core SQL Server DbContext
builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Configure Distributed Memory Cache and Server-Side Session Management
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.Name = ".RaceDay.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 4. Configure Swagger/OpenAPI with XML comments and Annotations
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RaceDay Event Management API",
        Version = "v1",
        Description = "RESTful API for the South African RaceDay road running, walking, and cycling platform. Uses server-side session authentication and enforces Organiser and Participant role-based access."
    });

    options.EnableAnnotations();

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Automatically create and seed the SQL Server database on startup if it does not exist
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
    if (dbContext.Database.IsRelational())
    {
        dbContext.Database.EnsureCreated();
    }
}

// Enable Swagger UI in all environments for testing and marking verification
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RaceDay API v1");
});

app.UseHttpsRedirection();

// Enable Session Middleware before Authorization and MapControllers
app.UseSession();

app.UseAuthorization();

app.MapControllers();

app.Run();