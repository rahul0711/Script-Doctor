-- =====================================================================
-- Phase 3 — Appointments, Payments, Clinical Notes (missing from live DB)
-- Applied as a prerequisite for Phase 4 Public Appointment Booking.
-- Source: TEST/Appointments.sql, TEST/DoctorNotes.sql
-- =====================================================================

CREATE TABLE Appointments
(
    AppointmentID INT AUTO_INCREMENT PRIMARY KEY,
    OrganizationID INT NOT NULL,
    BranchID INT NULL,
    DoctorID INT NOT NULL,
    PatientID INT NOT NULL,
    AppointmentType ENUM('Clinic','Video','Voice') NOT NULL,
    AppointmentDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    ConsultationFee DECIMAL(10,2) NOT NULL,
    PriorityConsultation BOOLEAN DEFAULT FALSE,
    Symptoms TEXT,
    AppointmentReason TEXT,
    Status ENUM('Pending','Approved','Rejected','Rescheduled','Completed','Cancelled','No Show') DEFAULT 'Pending',
    Notes TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (OrganizationID) REFERENCES Organizations(OrganizationID),
    FOREIGN KEY (BranchID) REFERENCES Branches(BranchID),
    FOREIGN KEY (DoctorID) REFERENCES Doctors(DoctorID),
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID)
);

CREATE TABLE AppointmentStatusHistory
(
    HistoryID INT AUTO_INCREMENT PRIMARY KEY,
    AppointmentID INT NOT NULL,
    OldStatus VARCHAR(50),
    NewStatus VARCHAR(50) NOT NULL,
    ChangedByUserID INT NOT NULL,
    Remarks VARCHAR(500),
    ChangedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (AppointmentID) REFERENCES Appointments(AppointmentID) ON DELETE CASCADE,
    FOREIGN KEY (ChangedByUserID) REFERENCES Users(UserID)
);

CREATE TABLE Payments
(
    PaymentID INT AUTO_INCREMENT PRIMARY KEY,
    AppointmentID INT NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    PaymentMethod ENUM('Cash','UPI','Credit Card','Debit Card','Net Banking') NOT NULL,
    TransactionReference VARCHAR(255),
    PaymentStatus ENUM('Pending','Paid','Failed','Refunded') DEFAULT 'Pending',
    PaidAt DATETIME NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (AppointmentID) REFERENCES Appointments(AppointmentID) ON DELETE CASCADE,
    UNIQUE(AppointmentID)
);

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
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (AppointmentID) REFERENCES Appointments(AppointmentID) ON DELETE CASCADE,
    FOREIGN KEY (DoctorID) REFERENCES Doctors(DoctorID),
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID)
);

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
    FOREIGN KEY (AppointmentID) REFERENCES Appointments(AppointmentID) ON DELETE CASCADE,
    FOREIGN KEY (DoctorID) REFERENCES Doctors(DoctorID),
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
    UNIQUE(AppointmentID)
);

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
    FOREIGN KEY (PrescriptionID) REFERENCES Prescriptions(PrescriptionID) ON DELETE CASCADE
);

CREATE TABLE FollowUps
(
    FollowUpID INT AUTO_INCREMENT PRIMARY KEY,
    AppointmentID INT NOT NULL,
    DoctorID INT NOT NULL,
    PatientID INT NOT NULL,
    FollowUpDate DATE NOT NULL,
    Reason VARCHAR(500),
    Status ENUM('Pending','Completed','Cancelled') DEFAULT 'Pending',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (AppointmentID) REFERENCES Appointments(AppointmentID),
    FOREIGN KEY (DoctorID) REFERENCES Doctors(DoctorID),
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID)
);
