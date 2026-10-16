using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Helpers;

namespace RaceDay.Tests
{
    public class InMemoryTestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();

        public string Id { get; } = Guid.NewGuid().ToString();
        public bool IsAvailable => true;
        public IEnumerable<string> Keys => _store.Keys;

        public void Clear() => _store.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => _store.Remove(key);

        public void Set(string key, byte[] value) => _store[key] = value;

        public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value) => _store.TryGetValue(key, out value);
    }

    public abstract class TestBase
    {
        protected static RaceDayDbContext CreateSeededDbContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new RaceDayDbContext(options);
            context.Database.EnsureCreated(); // Applies OnModelCreating HasData seed records
            return context;
        }

        protected static void AttachSession(
            ControllerBase controller,
            int? userId = null,
            string? role = null,
            string? email = null)
        {
            var httpContext = new DefaultHttpContext();
            var session = new InMemoryTestSession();

            if (userId.HasValue && !string.IsNullOrEmpty(role))
            {
                SessionAuthHelper.SetUserSession(session, userId.Value, role, email ?? "test@raceday.co.za");
            }

            httpContext.Session = session;
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }
    }
}