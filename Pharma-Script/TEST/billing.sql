CREATE TABLE Invoices
(
    InvoiceID INT AUTO_INCREMENT PRIMARY KEY,

    OrganizationID INT NOT NULL,

    AppointmentID INT NOT NULL,

    PatientID INT NOT NULL,

    InvoiceNumber VARCHAR(50) NOT NULL UNIQUE,

    InvoiceDate DATETIME DEFAULT CURRENT_TIMESTAMP,

    SubTotal DECIMAL(10,2) NOT NULL,

    DiscountAmount DECIMAL(10,2) DEFAULT 0,

    TaxAmount DECIMAL(10,2) DEFAULT 0,

    GrandTotal DECIMAL(10,2) NOT NULL,

    InvoiceStatus ENUM
    (
        'Draft',
        'Generated',
        'Paid',
        'Cancelled'
    ) DEFAULT 'Generated',

    Notes TEXT,

    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (OrganizationID)
        REFERENCES Organizations(OrganizationID),

    FOREIGN KEY (AppointmentID)
        REFERENCES Appointments(AppointmentID),

    FOREIGN KEY (PatientID)
        REFERENCES Patients(PatientID)
);

-- InvoiceItems
CREATE TABLE InvoiceItems
(
    InvoiceItemID INT AUTO_INCREMENT PRIMARY KEY,

    InvoiceID INT NOT NULL,

    ItemName VARCHAR(200) NOT NULL,

    Description VARCHAR(500),

    Quantity INT DEFAULT 1,

    UnitPrice DECIMAL(10,2) NOT NULL,

    TotalPrice DECIMAL(10,2) NOT NULL,

    FOREIGN KEY (InvoiceID)
        REFERENCES Invoices(InvoiceID)
        ON DELETE CASCADE
);

-- Refunds

CREATE TABLE Refunds
(
    RefundID INT AUTO_INCREMENT PRIMARY KEY,

    PaymentID INT NOT NULL,

    RefundAmount DECIMAL(10,2) NOT NULL,

    RefundReason VARCHAR(500),

    RefundStatus ENUM
    (
        'Pending',
        'Processed',
        'Rejected'
    ) DEFAULT 'Pending',

    RefundDate DATETIME DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (PaymentID)
        REFERENCES Payments(PaymentID)
);