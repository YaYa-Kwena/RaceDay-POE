-- ====================================================================
-- RACEDAY DATABASE SCRIPT
-- ====================================================================

-- 1. USERS TABLE (Handles Organisers and Participants)
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Email VARCHAR(100) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    UserRole VARCHAR(20) NOT NULL CHECK (UserRole IN ('Organiser', 'Participant')),
    Gender VARCHAR(10) CHECK (Gender IN ('Male', 'Female', 'Other')),
    DateOfBirth DATE NOT NULL,
    PhoneNumber VARCHAR(15),
    EmergencyContactName VARCHAR(100),
    EmergencyContactPhone VARCHAR(15),
    MedicalConditions TEXT DEFAULT 'None',
    RegistrationDate DATETIME DEFAULT GETDATE()
);
-- 2. EVENTS TABLE
CREATE TABLE Events (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserID INT NOT NULL,
    EventName VARCHAR(150) NOT NULL,
    Description TEXT NOT NULL,
    EventType VARCHAR(50) CHECK (EventType IN ('Running', 'Walking', 'Cycling')),
    EventDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    LocationCity VARCHAR(100) NOT NULL,
    LocationProvince VARCHAR(50) NOT NULL,
    MaxCapacity INT NOT NULL DEFAULT 0,
    RegistrationDeadline DATE NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (OrganiserID) REFERENCES Users(UserID)
);

-- 3. EVENT ROUTES TABLE
CREATE TABLE EventRoutes (
    RouteID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    RouteName VARCHAR(100) NOT NULL,
    TotalDistanceKm DECIMAL(5,2) NOT NULL,
    ElevationGainMeters INT DEFAULT 0,
    WaterPointsCount INT DEFAULT 0,
    RouteMapUrl VARCHAR(255),
    FOREIGN KEY (EventID) REFERENCES Events(EventID) ON DELETE CASCADE
);