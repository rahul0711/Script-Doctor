CREATE TABLE Appointments
(
    AppointmentID INT AUTO_INCREMENT PRIMARY KEY,

    OrganizationID INT NOT NULL,

    BranchID INT NULL,

    DoctorID INT NOT NULL,

    PatientID INT NOT NULL,

    AppointmentType ENUM
    (
        'Clinic',
        'Video',
        'Voice'
    ) NOT NULL,

    AppointmentDate DATE NOT NULL,

    StartTime TIME NOT NULL,

    EndTime TIME NOT NULL,

    ConsultationFee DECIMAL(10,2) NOT NULL,

    PriorityConsultation BOOLEAN DEFAULT FALSE,

    Symptoms TEXT,

    AppointmentReason TEXT,

    Status ENUM
    (
        'Pending',
        'Approved',
        'Rejected',
        'Rescheduled',
        'Completed',
        'Cancelled',
        'No Show'
    ) DEFAULT 'Pending',

    Notes TEXT,

    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,

    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (OrganizationID)
        REFERENCES Organizations(OrganizationID),

    FOREIGN KEY (BranchID)
        REFERENCES Branches(BranchID),

    FOREIGN KEY (DoctorID)
        REFERENCES Doctors(DoctorID),

    FOREIGN KEY (PatientID)
        REFERENCES Patients(PatientID)
);

-- AppointmentStatusHistory

CREATE TABLE AppointmentStatusHistory
(
    HistoryID INT AUTO_INCREMENT PRIMARY KEY,

    AppointmentID INT NOT NULL,

    OldStatus VARCHAR(50),

    NewStatus VARCHAR(50) NOT NULL,

    ChangedByUserID INT NOT NULL,

    Remarks VARCHAR(500),

    ChangedAt DATETIME DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (AppointmentID)
        REFERENCES Appointments(AppointmentID)
        ON DELETE CASCADE,

    FOREIGN KEY (ChangedByUserID)
        REFERENCES Users(UserID)
);

-- Payments

CREATE TABLE Payments
(
    PaymentID INT AUTO_INCREMENT PRIMARY KEY,

    AppointmentID INT NOT NULL,

    Amount DECIMAL(10,2) NOT NULL,

    PaymentMethod ENUM
    (
        'Cash',
        'UPI',
        'Credit Card',
        'Debit Card',
        'Net Banking'
    ) NOT NULL,

    TransactionReference VARCHAR(255),

    PaymentStatus ENUM
    (
        'Pending',
        'Paid',
        'Failed',
        'Refunded'
    ) DEFAULT 'Pending',

    PaidAt DATETIME NULL,

    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (AppointmentID)
        REFERENCES Appointments(AppointmentID)
        ON DELETE CASCADE,

    UNIQUE(AppointmentID)
);