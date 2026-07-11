CREATE TABLE Doctors
(
    DoctorID INT AUTO_INCREMENT PRIMARY KEY,

    UserID INT NOT NULL,

    OrganizationID INT NOT NULL,

    BranchID INT NULL,

    DepartmentID INT NULL,

    Qualification VARCHAR(250) NOT NULL,

    ExperienceYears INT DEFAULT 0,

    MedicalRegistrationNumber VARCHAR(100) NOT NULL,

    Biography TEXT,

    ConsultationFee DECIMAL(10,2) NOT NULL,

    VideoConsultationFee DECIMAL(10,2) NOT NULL,

    VoiceConsultationFee DECIMAL(10,2) NOT NULL,

    PriorityConsultationFee DECIMAL(10,2) NOT NULL,

    IsPriorityAvailable BOOLEAN DEFAULT FALSE,

    PriorityStartTime DATETIME NULL,

    PriorityEndTime DATETIME NULL,

    IsActive BOOLEAN DEFAULT TRUE,

    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,

    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (UserID)
        REFERENCES Users(UserID),

    FOREIGN KEY (OrganizationID)
        REFERENCES Organizations(OrganizationID),

    FOREIGN KEY (BranchID)
        REFERENCES Branches(BranchID),

    FOREIGN KEY (DepartmentID)
        REFERENCES Departments(DepartmentID),

    UNIQUE(UserID),

    UNIQUE(MedicalRegistrationNumber)
);


-- specilizaiton

CREATE TABLE DoctorSpecializations
(
    DoctorSpecializationID INT AUTO_INCREMENT PRIMARY KEY,

    DoctorID INT NOT NULL,

    SpecializationID INT NOT NULL,

    FOREIGN KEY (DoctorID)
        REFERENCES Doctors(DoctorID)
        ON DELETE CASCADE,

    FOREIGN KEY (SpecializationID)
        REFERENCES Specializations(SpecializationID),

    UNIQUE (DoctorID, SpecializationID)
);

-- avalibility
CREATE TABLE DoctorAvailability
(
    AvailabilityID INT AUTO_INCREMENT PRIMARY KEY,

    DoctorID INT NOT NULL,

    DayOfWeek ENUM
    (
        'Monday',
        'Tuesday',
        'Wednesday',
        'Thursday',
        'Friday',
        'Saturday',
        'Sunday'
    ) NOT NULL,

    StartTime TIME NOT NULL,

    EndTime TIME NOT NULL,

    SlotDuration INT NOT NULL DEFAULT 15,

    BreakStart TIME NULL,

    BreakEnd TIME NULL,

    IsAvailable BOOLEAN DEFAULT TRUE,

    FOREIGN KEY (DoctorID)
        REFERENCES Doctors(DoctorID)
        ON DELETE CASCADE
);

-- doctor leave

CREATE TABLE DoctorLeave
(
    LeaveID INT AUTO_INCREMENT PRIMARY KEY,

    DoctorID INT NOT NULL,

    LeaveStartDate DATE NOT NULL,

    LeaveEndDate DATE NOT NULL,

    Reason VARCHAR(250),

    FOREIGN KEY (DoctorID)
        REFERENCES Doctors(DoctorID)
        ON DELETE CASCADE
);

-- receptionist 
CREATE TABLE Receptionists
(
    ReceptionistID INT AUTO_INCREMENT PRIMARY KEY,

    UserID INT NOT NULL,

    OrganizationID INT NOT NULL,

    BranchID INT NULL,

    IsActive BOOLEAN DEFAULT TRUE,

    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (UserID)
        REFERENCES Users(UserID),

    FOREIGN KEY (OrganizationID)
        REFERENCES Organizations(OrganizationID),

    FOREIGN KEY (BranchID)
        REFERENCES Branches(BranchID),

    UNIQUE(UserID)
);