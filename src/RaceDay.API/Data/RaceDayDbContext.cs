using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<EventRoute> EventRoutes => Set<EventRoute>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Enrolment> Enrolments => Set<Enrolment>();
        public DbSet<Result> Results => Set<Result>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Enrolment>()
                .HasIndex(e => e.RaceNumber)
                .IsUnique();

            modelBuilder.Entity<Result>()
                .HasIndex(r => r.EnrolmentID)
                .IsUnique();

            // Relationships & Delete Behaviors (prevents SQL Server multiple cascade path errors)
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany(u => u.OrganisedEvents)
                .HasForeignKey(e => e.OrganiserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventRoute>()
                .HasOne(r => r.Event)
                .WithMany(e => e.EventRoutes)
                .HasForeignKey(r => r.EventID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Category>()
                .HasOne(c => c.Event)
                .WithMany(e => e.Categories)
                .HasForeignKey(c => c.EventID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Enrolment>()
                .HasOne(en => en.Participant)
                .WithMany(u => u.Enrolments)
                .HasForeignKey(en => en.ParticipantID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrolment>()
                .HasOne(en => en.Event)
                .WithMany(ev => ev.Enrolments)
                .HasForeignKey(en => en.EventID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrolment>()
                .HasOne(en => en.Category)
                .WithMany(c => c.Enrolments)
                .HasForeignKey(en => en.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrolment>()
                .HasOne(en => en.Result)
                .WithOne(r => r.Enrolment)
                .HasForeignKey<Result>(r => r.EnrolmentID)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed Data matching Part 1 SQL Script
            // SHA-256 hash below corresponds to "Password123!"
            const string defaultPasswordHash = "c7ad44cbad762a5da0a452f9e854fdc1e0e7a52a38015f23f3eab1d80b931dd4";

            modelBuilder.Entity<User>().HasData(
                new User { UserID = 1, FirstName = "Sipho", LastName = "Nkosi", Email = "sipho@raceday.co.za", PasswordHash = defaultPasswordHash, UserRole = "Organiser", Gender = "Male", DateOfBirth = new DateTime(1980, 5, 15), PhoneNumber = "0821112222", EmergencyContactName = "Zanele Nkosi", EmergencyContactPhone = "0823334444", MedicalConditions = "None", RegistrationDate = new DateTime(2026, 1, 10) },
                new User { UserID = 2, FirstName = "Amina", LastName = "Van Wyk", Email = "amina@raceday.co.za", PasswordHash = defaultPasswordHash, UserRole = "Organiser", Gender = "Female", DateOfBirth = new DateTime(1985, 8, 22), PhoneNumber = "0831112222", EmergencyContactName = "Johan Van Wyk", EmergencyContactPhone = "0833334444", MedicalConditions = "None", RegistrationDate = new DateTime(2026, 1, 12) },
                new User { UserID = 3, FirstName = "Thabo", LastName = "Mokoena", Email = "thabo@gmail.com", PasswordHash = defaultPasswordHash, UserRole = "Participant", Gender = "Male", DateOfBirth = new DateTime(1992, 12, 1), PhoneNumber = "0711112222", EmergencyContactName = "Lerato Mokoena", EmergencyContactPhone = "0713334444", MedicalConditions = "Asthma", RegistrationDate = new DateTime(2026, 1, 15) },
                new User { UserID = 4, FirstName = "Jessica", LastName = "Smith", Email = "jess@gmail.com", PasswordHash = defaultPasswordHash, UserRole = "Participant", Gender = "Female", DateOfBirth = new DateTime(1995, 3, 10), PhoneNumber = "0721112222", EmergencyContactName = "Mark Smith", EmergencyContactPhone = "0723334444", MedicalConditions = "None", RegistrationDate = new DateTime(2026, 1, 18) },
                new User { UserID = 5, FirstName = "David", LastName = "Naidoo", Email = "david@gmail.com", PasswordHash = defaultPasswordHash, UserRole = "Participant", Gender = "Male", DateOfBirth = new DateTime(1975, 11, 30), PhoneNumber = "0841112222", EmergencyContactName = "Priya Naidoo", EmergencyContactPhone = "0843334444", MedicalConditions = "Diabetic", RegistrationDate = new DateTime(2026, 1, 20) }
            );

            modelBuilder.Entity<Event>().HasData(
                new Event { EventID = 1, OrganiserID = 1, EventName = "Soweto Marathon", Description = "The peoples race through historic Soweto.", EventType = "Running", DistanceKm = 42.20m, EventDate = new DateTime(2026, 11, 1), StartTime = new TimeSpan(5, 30, 0), LocationCity = "Johannesburg", LocationProvince = "Gauteng", MaxCapacity = 25000, RegistrationDeadline = new DateTime(2026, 10, 1), IsActive = true, CreatedAt = new DateTime(2026, 2, 1) },
                new Event { EventID = 2, OrganiserID = 2, EventName = "Cape Town Cycle Tour", Description = "Scenic coastal cycling route.", EventType = "Cycling", DistanceKm = 109.00m, EventDate = new DateTime(2026, 3, 8), StartTime = new TimeSpan(6, 0, 0), LocationCity = "Cape Town", LocationProvince = "Western Cape", MaxCapacity = 35000, RegistrationDeadline = new DateTime(2026, 2, 15), IsActive = true, CreatedAt = new DateTime(2026, 1, 20) },
                new Event { EventID = 3, OrganiserID = 1, EventName = "Comrades Marathon", Description = "The ultimate human race (Up Run).", EventType = "Running", DistanceKm = 87.70m, EventDate = new DateTime(2026, 6, 14), StartTime = new TimeSpan(5, 30, 0), LocationCity = "Durban", LocationProvince = "KwaZulu-Natal", MaxCapacity = 20000, RegistrationDeadline = new DateTime(2026, 5, 1), IsActive = true, CreatedAt = new DateTime(2026, 2, 5) }
            );

            modelBuilder.Entity<EventRoute>().HasData(
                new EventRoute { RouteID = 1, EventID = 1, RouteName = "Soweto Full Marathon Route", TotalDistanceKm = 42.20m, ElevationGainMeters = 450, WaterPointsCount = 16 },
                new EventRoute { RouteID = 2, EventID = 2, RouteName = "Peninsula Loop", TotalDistanceKm = 109.00m, ElevationGainMeters = 1200, WaterPointsCount = 8 },
                new EventRoute { RouteID = 3, EventID = 3, RouteName = "Durban to Pietermaritzburg (Up)", TotalDistanceKm = 87.70m, ElevationGainMeters = 1750, WaterPointsCount = 43 }
            );

            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryID = 1, EventID = 1, CategoryName = "Open Men 42.2km", DistanceKm = 42.20m, EntryFeeZAR = 350.00m, MinAge = 18, MaxAge = 59, GenderRestriction = "Male" },
                new Category { CategoryID = 2, EventID = 1, CategoryName = "Open Women 42.2km", DistanceKm = 42.20m, EntryFeeZAR = 350.00m, MinAge = 18, MaxAge = 59, GenderRestriction = "Female" },
                new Category { CategoryID = 3, EventID = 2, CategoryName = "Elite Cycling 109km", DistanceKm = 109.00m, EntryFeeZAR = 600.00m, MinAge = 18, MaxAge = 120, GenderRestriction = "Open" },
                new Category { CategoryID = 4, EventID = 3, CategoryName = "Comrades Veterans 87km", DistanceKm = 87.70m, EntryFeeZAR = 1200.00m, MinAge = 40, MaxAge = 49, GenderRestriction = "Open" }
            );

            modelBuilder.Entity<Enrolment>().HasData(
                new Enrolment { EnrolmentID = 1, ParticipantID = 3, EventID = 1, CategoryID = 1, RaceNumber = 10045, PaymentStatus = "Paid", EnrolmentDate = new DateTime(2026, 2, 10) },
                new Enrolment { EnrolmentID = 2, ParticipantID = 4, EventID = 2, CategoryID = 3, RaceNumber = 2201, PaymentStatus = "Paid", EnrolmentDate = new DateTime(2026, 2, 11) },
                new Enrolment { EnrolmentID = 3, ParticipantID = 5, EventID = 3, CategoryID = 4, RaceNumber = 30560, PaymentStatus = "Paid", EnrolmentDate = new DateTime(2026, 2, 12) }
            );

            modelBuilder.Entity<Result>().HasData(
                new Result { ResultID = 1, EnrolmentID = 1, FinishTime = new TimeSpan(3, 15, 22), OverallPosition = 150, CategoryPosition = 45, RaceStatus = "Finished", CapturedAt = new DateTime(2026, 11, 1, 10, 0, 0) },
                new Result { ResultID = 2, EnrolmentID = 2, FinishTime = new TimeSpan(2, 55, 10), OverallPosition = 42, CategoryPosition = 12, RaceStatus = "Finished", CapturedAt = new DateTime(2026, 3, 8, 10, 0, 0) }
            );
        }
    }
}