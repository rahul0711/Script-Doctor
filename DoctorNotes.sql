CREATE TABLE DoctorNotes
(
    NoteID INT AUTO_INCREMENT PRIMARY KEY,

    AppointmentID INT NOT NULL,

    DoctorID INT NOT NULL,

    PatientID INT NOT NULL,

    ClinicalNotes TEXT,

    Diagnosis TEXT,

    Advice TEXT,

    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,

    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (AppointmentID)
        REFERENCES Appointments(AppointmentID)
        ON DELETE CASCADE,

    FOREIGN KEY (DoctorID)
        REFERENCES Doctors(DoctorID),

    FOREIGN KEY (PatientID)
        REFERENCES Patients(PatientID)
);

-- Prescriptions

CREATE TABLE Prescriptions
(
    PrescriptionID INT AUTO_INCREMENT PRIMARY KEY,

    AppointmentID INT NOT NULL,

    DoctorID INT NOT NULL,

    PatientID INT NOT NULL,

    PrescriptionNumber VARCHAR(50) UNIQUE,

    GeneralInstructions TEXT,

    NextVisitDate DATE NULL,

    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (AppointmentID)
        REFERENCES Appointments(AppointmentID)
        ON DELETE CASCADE,

    FOREIGN KEY (DoctorID)
        REFERENCES Doctors(DoctorID),

    FOREIGN KEY (PatientID)
        REFERENCES Patients(PatientID),

    UNIQUE(AppointmentID)
);

-- priscription 
CREATE TABLE PrescriptionMedicines
(
    PrescriptionMedicineID INT AUTO_INCREMENT PRIMARY KEY,

    PrescriptionID INT NOT NULL,

    MedicineName VARCHAR(255) NOT NULL,

    Strength VARCHAR(50),

    Dosage VARCHAR(100),

    Morning BOOLEAN DEFAULT FALSE,

    Afternoon BOOLEAN DEFAULT FALSE,

    Night BOOLEAN DEFAULT FALSE,

    BeforeFood BOOLEAN DEFAULT FALSE,

    AfterFood BOOLEAN DEFAULT FALSE,

    DurationDays INT NOT NULL,

    Quantity INT,

    Remarks VARCHAR(500),

    FOREIGN KEY (PrescriptionID)
        REFERENCES Prescriptions(PrescriptionID)
        ON DELETE CASCADE
);

-- followups

CREATE TABLE FollowUps
(
    FollowUpID INT AUTO_INCREMENT PRIMARY KEY,

    AppointmentID INT NOT NULL,

    DoctorID INT NOT NULL,

    PatientID INT NOT NULL,

    FollowUpDate DATE NOT NULL,

    Reason VARCHAR(500),

    Status ENUM
    (
        'Pending',
        'Completed',
        'Cancelled'
    ) DEFAULT 'Pending',

    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (AppointmentID)
        REFERENCES Appointments(AppointmentID),

    FOREIGN KEY (DoctorID)
        REFERENCES Doctors(DoctorID),

    FOREIGN KEY (PatientID)
        REFERENCES Patients(PatientID)
);