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
-- 4. CATEGORIES TABLE
CREATE TABLE Categories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    CategoryName VARCHAR(100) NOT NULL,
    DistanceKm DECIMAL(5,2) NOT NULL,
    EntryFeeZAR DECIMAL(8,2) NOT NULL DEFAULT 0.00,
    MinAge INT DEFAULT 0,
    MaxAge INT DEFAULT 120,
    GenderRestriction VARCHAR(10) DEFAULT 'Open' CHECK (GenderRestriction IN ('Male', 'Female', 'Open')),
    FOREIGN KEY (EventID) REFERENCES Events(EventID) ON DELETE CASCADE
);

-- 5. ENROLMENTS TABLE
CREATE TABLE Enrolments (
    EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID INT NOT NULL,
    CategoryID INT NOT NULL,
    RaceNumber INT UNIQUE,
    PaymentStatus VARCHAR(20) DEFAULT 'Pending' CHECK (PaymentStatus IN ('Pending', 'Paid', 'Refunded')),
    EnrolmentDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ParticipantID) REFERENCES Users(UserID),
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID) ON DELETE CASCADE
);

-- 6. RESULTS TABLE
CREATE TABLE Results (
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID INT NOT NULL UNIQUE,
    FinishTime TIME,
    OverallPosition INT,
    CategoryPosition INT,
    RaceStatus VARCHAR(20) DEFAULT 'Finished' CHECK (RaceStatus IN ('Finished', 'DNF', 'DNS', 'Disqualified')),
    CapturedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (EnrolmentID) REFERENCES Enrolments(EnrolmentID) ON DELETE CASCADE
);

-- ====================================================================
-- SEED DATA 
-- ====================================================================

-- Insert Users (2 Organisers, 3 Participants)
INSERT INTO Users (FirstName, LastName, Email, PasswordHash, UserRole, Gender, DateOfBirth, PhoneNumber, EmergencyContactName, EmergencyContactPhone, MedicalConditions) VALUES
('Sipho', 'Nkosi', 'sipho@raceday.co.za', 'hash1', 'Organiser', 'Male', '1980-05-15', '0821112222', 'Zanele Nkosi', '0823334444', 'None'),
('Amina', 'Van Wyk', 'amina@raceday.co.za', 'hash2', 'Organiser', 'Female', '1985-08-22', '0831112222', 'Johan Van Wyk', '0833334444', 'None'),
('Thabo', 'Mokoena', 'thabo@gmail.com', 'hash3', 'Participant', 'Male', '1992-12-01', '0711112222', 'Lerato Mokoena', '0713334444', 'Asthma'),
('Jessica', 'Smith', 'jess@gmail.com', 'hash4', 'Participant', 'Female', '1995-03-10', '0721112222', 'Mark Smith', '0723334444', 'None'),
('David', 'Naidoo', 'david@gmail.com', 'hash5', 'Participant', 'Male', '1975-11-30', '0841112222', 'Priya Naidoo', '0843334444', 'Diabetic');
