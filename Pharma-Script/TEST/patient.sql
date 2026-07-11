CREATE TABLE Patients
(
    PatientID INT AUTO_INCREMENT PRIMARY KEY,

    UserID INT NOT NULL,

    OrganizationID INT NOT NULL,

    BranchID INT NULL,

    DateOfBirth DATE NOT NULL,

    Gender ENUM
    (
        'Male',
        'Female',
        'Other'
    ) NOT NULL,

    BloodGroup ENUM
    (
        'A+',
        'A-',
        'B+',
        'B-',
        'AB+',
        'AB-',
        'O+',
        'O-'
    ) NULL,

    Height DECIMAL(5,2) NULL,

    Weight DECIMAL(5,2) NULL,

    EmergencyContactName VARCHAR(200) NULL,

    EmergencyContactNumber VARCHAR(20) NULL,

    Address VARCHAR(500) NULL,

    City VARCHAR(100) NULL,

    State VARCHAR(100) NULL,

    Country VARCHAR(100) NULL,

    Pincode VARCHAR(15) NULL,

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

    UNIQUE(UserID)
);

-- medical history

CREATE TABLE PatientMedicalHistory
(
    MedicalHistoryID INT AUTO_INCREMENT PRIMARY KEY,

    PatientID INT NOT NULL,

    Diabetes BOOLEAN DEFAULT FALSE,

    BloodPressure BOOLEAN DEFAULT FALSE,

    HeartDisease BOOLEAN DEFAULT FALSE,

    Asthma BOOLEAN DEFAULT FALSE,

    Thyroid BOOLEAN DEFAULT FALSE,

    Allergies TEXT NULL,

    CurrentMedications TEXT NULL,

    PastSurgeries TEXT NULL,

    FamilyMedicalHistory TEXT NULL,

    OtherMedicalConditions TEXT NULL,

    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,

    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (PatientID)
        REFERENCES Patients(PatientID)
        ON DELETE CASCADE
);

-- patients vitals

CREATE TABLE PatientVitals
(
    VitalID INT AUTO_INCREMENT PRIMARY KEY,

    PatientID INT NOT NULL,

    Height DECIMAL(5,2) NULL,

    Weight DECIMAL(5,2) NULL,

    BloodPressure VARCHAR(20) NULL,

    HeartRate INT NULL,

    Temperature DECIMAL(4,1) NULL,

    OxygenLevel INT NULL,

    BloodSugar VARCHAR(20) NULL,

    BMI DECIMAL(5,2) NULL,

    RecordedAt DATETIME DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (PatientID)
        REFERENCES Patients(PatientID)
        ON DELETE CASCADE
);

-- MedicalDocuments

CREATE TABLE MedicalDocuments
(
    DocumentID INT AUTO_INCREMENT PRIMARY KEY,

    PatientID INT NOT NULL,

    OrganizationID INT NOT NULL,

    UploadedByUserID INT NOT NULL,

    DocumentTitle VARCHAR(200) NOT NULL,

    DocumentType ENUM
    (
        'Prescription',
        'Blood Report',
        'MRI',
        'CT Scan',
        'X-Ray',
        'ECG',
        'Insurance',
        'Other'
    ) NOT NULL,

    FileName VARCHAR(255) NOT NULL,

    FilePath VARCHAR(500) NOT NULL,

    FileSize BIGINT NULL,

    UploadDate DATETIME DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (PatientID)
        REFERENCES Patients(PatientID),

    FOREIGN KEY (OrganizationID)
        REFERENCES Organizations(OrganizationID),

    FOREIGN KEY (UploadedByUserID)
        REFERENCES Users(UserID)
);