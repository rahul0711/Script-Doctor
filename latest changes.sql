CREATE TABLE CMSSettings
(
    CMSSettingID INT AUTO_INCREMENT PRIMARY KEY,

    OrganizationID INT NOT NULL,

    WebsiteTitle VARCHAR(200) NOT NULL,

    WebsiteLogo VARCHAR(500),

    Favicon VARCHAR(500),

    PrimaryColor VARCHAR(20),

    SecondaryColor VARCHAR(20),

    AboutUs TEXT,

    Mission TEXT,

    Vision TEXT,

    ContactEmail VARCHAR(150),

    ContactPhone VARCHAR(20),

    EmergencyPhone VARCHAR(20),

    Address TEXT,

    GoogleMapEmbed TEXT,

    FacebookURL VARCHAR(300),

    InstagramURL VARCHAR(300),

    LinkedInURL VARCHAR(300),

    TwitterURL VARCHAR(300),

    YouTubeURL VARCHAR(300),

    FooterText TEXT,

    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,

    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (OrganizationID)
        REFERENCES Organizations(OrganizationID),

    UNIQUE (OrganizationID)
);

-- home
CREATE TABLE HeroSections
(
    HeroSectionID INT AUTO_INCREMENT PRIMARY KEY,

    OrganizationID INT NOT NULL,

    Title VARCHAR(255) NOT NULL,

    Subtitle TEXT,

    BannerImage VARCHAR(500),

    ButtonText VARCHAR(100),

    ButtonURL VARCHAR(300),

    IsActive BOOLEAN DEFAULT TRUE,

    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (OrganizationID)
        REFERENCES Organizations(OrganizationID)
);

-- Services

CREATE TABLE Services
(
    ServiceID INT AUTO_INCREMENT PRIMARY KEY,

    OrganizationID INT NOT NULL,

    ServiceName VARCHAR(200) NOT NULL,

    Description TEXT,

    ServiceImage VARCHAR(500),

    IsActive BOOLEAN DEFAULT TRUE,

    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (OrganizationID)
        REFERENCES Organizations(OrganizationID)
); 

-- Gallery

CREATE TABLE Gallery
(
    GalleryID INT AUTO_INCREMENT PRIMARY KEY,

    OrganizationID INT NOT NULL,

    ImageTitle VARCHAR(200),

    ImagePath VARCHAR(500) NOT NULL,

    DisplayOrder INT DEFAULT 1,

    IsActive BOOLEAN DEFAULT TRUE,

    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (OrganizationID)
        REFERENCES Organizations(OrganizationID)
);

-- FAQs

CREATE TABLE FAQs
(
    FAQID INT AUTO_INCREMENT PRIMARY KEY,

    OrganizationID INT NOT NULL,

    Question VARCHAR(500) NOT NULL,

    Answer TEXT NOT NULL,

    DisplayOrder INT DEFAULT 1,

    IsActive BOOLEAN DEFAULT TRUE,

    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (OrganizationID)
        REFERENCES Organizations(OrganizationID)
);

-- ContactMessages

CREATE TABLE ContactMessages
(
    ContactMessageID INT AUTO_INCREMENT PRIMARY KEY,

    OrganizationID INT NOT NULL,

    Name VARCHAR(200) NOT NULL,

    Email VARCHAR(150),

    Phone VARCHAR(20),

    Subject VARCHAR(250),

    Message TEXT NOT NULL,

    IsRead BOOLEAN DEFAULT FALSE,

    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (OrganizationID)
        REFERENCES Organizations(OrganizationID)
);

CREATE TABLE HomepageSections
(
    SectionID INT AUTO_INCREMENT PRIMARY KEY,

    OrganizationID INT NOT NULL,

    SectionName VARCHAR(100),

    DisplayOrder INT,

    IsVisible BOOLEAN DEFAULT TRUE,

    FOREIGN KEY (OrganizationID)
        REFERENCES Organizations(OrganizationID)
);

CREATE TABLE DoctorPaymentGateway
(
    DoctorPaymentGatewayID INT AUTO_INCREMENT PRIMARY KEY,

    OrganizationID INT NOT NULL,

    DoctorID INT NOT NULL,

    PaymentProvider ENUM('Razorpay') NOT NULL DEFAULT 'Razorpay',

    KeyID VARCHAR(255) NOT NULL,

    KeySecret VARCHAR(255) NOT NULL,

    IsActive BOOLEAN NOT NULL DEFAULT TRUE,

    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (OrganizationID)
        REFERENCES Organizations(OrganizationID),

    FOREIGN KEY (DoctorID)
        REFERENCES Doctors(DoctorID)
        ON DELETE CASCADE,

    UNIQUE (DoctorID, PaymentProvider)
);