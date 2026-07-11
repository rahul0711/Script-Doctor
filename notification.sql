CREATE TABLE Notifications
(
    NotificationID INT AUTO_INCREMENT PRIMARY KEY,

    OrganizationID INT NOT NULL,

    UserID INT NOT NULL,

    Title VARCHAR(200) NOT NULL,

    Message TEXT NOT NULL,

    NotificationType ENUM
    (
        'Appointment',
        'Payment',
        'Prescription',
        'Reminder',
        'FollowUp',
        'System'
    ) NOT NULL,

    IsRead BOOLEAN DEFAULT FALSE,

    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (OrganizationID)
        REFERENCES Organizations(OrganizationID),

    FOREIGN KEY (UserID)
        REFERENCES Users(UserID)
);

-- rivews
CREATE TABLE Reviews
(
    ReviewID INT AUTO_INCREMENT PRIMARY KEY,

    OrganizationID INT NOT NULL,

    DoctorID INT NOT NULL,

    PatientID INT NOT NULL,

    AppointmentID INT NOT NULL,

    Rating INT NOT NULL,

    Review TEXT,

    IsApproved BOOLEAN DEFAULT TRUE,

    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (OrganizationID)
        REFERENCES Organizations(OrganizationID),

    FOREIGN KEY (DoctorID)
        REFERENCES Doctors(DoctorID),

    FOREIGN KEY (PatientID)
        REFERENCES Patients(PatientID),

    FOREIGN KEY (AppointmentID)
        REFERENCES Appointments(AppointmentID),

    UNIQUE(AppointmentID)
);

-- tempelate
CREATE TABLE NotificationTemplates
(
    TemplateID INT AUTO_INCREMENT PRIMARY KEY,

    OrganizationID INT NULL,

    TemplateName VARCHAR(150) NOT NULL,

    NotificationChannel ENUM
    (
        'InApp',
        'Email',
        'SMS',
        'WhatsApp'
    ) NOT NULL,

    Subject VARCHAR(255),

    MessageBody TEXT NOT NULL,

    IsActive BOOLEAN DEFAULT TRUE,

    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (OrganizationID)
        REFERENCES Organizations(OrganizationID)
);

-- logs
CREATE TABLE NotificationLogs
(
    LogID INT AUTO_INCREMENT PRIMARY KEY,

    NotificationID INT NOT NULL,

    NotificationChannel ENUM
    (
        'InApp',
        'Email',
        'SMS',
        'WhatsApp'
    ) NOT NULL,

    DeliveryStatus ENUM
    (
        'Pending',
        'Sent',
        'Failed'
    ) DEFAULT 'Pending',

    SentAt DATETIME,

    ErrorMessage TEXT,

    FOREIGN KEY (NotificationID)
        REFERENCES Notifications(NotificationID)
        ON DELETE CASCADE
);	