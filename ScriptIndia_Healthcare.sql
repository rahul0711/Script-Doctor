create database ScriptIndia_Healthcare;
CREATE TABLE Organizations
(
    OrganizationID INT AUTO_INCREMENT PRIMARY KEY,

    OrganizationName VARCHAR(200) NOT NULL,

    OrganizationType VARCHAR(50) NOT NULL,

    Email VARCHAR(150) NOT NULL,

    Phone VARCHAR(20) NOT NULL,

    AlternatePhone VARCHAR(20),

    AddressLine1 VARCHAR(250) NOT NULL,

    AddressLine2 VARCHAR(250),

    City VARCHAR(100) NOT NULL,

    State VARCHAR(100) NOT NULL,

    Country VARCHAR(100) NOT NULL,

    Pincode VARCHAR(15) NOT NULL,

    GSTNumber VARCHAR(50),

    LicenseNumber VARCHAR(100),

    IsActive BOOLEAN NOT NULL DEFAULT TRUE,

    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    UpdatedAt DATETIME DEFAULT NULL
);




-- braches-- 

CREATE TABLE Branches
(
    BranchID INT auto_increment PRIMARY KEY,

    OrganizationID INT NOT NULL,

    BranchName VARCHAR(200) NOT NULL,

    Email VARCHAR(150) NULL,

    Phone VARCHAR(20) NULL,

    AddressLine1 VARCHAR(250) NOT NULL,

    AddressLine2 VARCHAR(250) NULL,

    City VARCHAR(100) NOT NULL,

    State VARCHAR(100) NOT NULL,

    Country VARCHAR(100) NOT NULL,

    Pincode VARCHAR(15) NOT NULL,

    IsMainBranch BIT NOT NULL DEFAULT 0,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (OrganizationID)
    REFERENCES Organizations(OrganizationID)
);


-- Roles

CREATE TABLE Roles
(
    RoleID INT auto_increment PRIMARY KEY,

    RoleName VARCHAR(50) NOT NULL UNIQUE,

    Description VARCHAR(250) NULL
);


-- users 


CREATE TABLE Users
(
    UserID INT auto_increment PRIMARY KEY,

    OrganizationID INT NULL,

    RoleID INT NOT NULL,

    FirstName VARCHAR(100) NOT NULL,

    LastName VARCHAR(100) NULL,

    Email VARCHAR(150) NOT NULL UNIQUE,

    Phone VARCHAR(20) NOT NULL,

    PasswordHash VARCHAR(500) NOT NULL,

    ProfileImage VARCHAR(500) NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    LastLogin DATETIME NULL,

    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    UpdatedAt DATETIME NULL,

    FOREIGN KEY (OrganizationID)
        REFERENCES Organizations(OrganizationID),

    FOREIGN KEY (RoleID)
        REFERENCES Roles(RoleID)
);

-- --departments

CREATE TABLE Departments
(
    DepartmentID INT auto_increment PRIMARY KEY,

    OrganizationID INT NOT NULL,

    DepartmentName VARCHAR(150) NOT NULL,

    Description VARCHAR(500) NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (OrganizationID)
    REFERENCES Organizations(OrganizationID)
);


CREATE TABLE Specializations
(
    SpecializationID INT auto_increment PRIMARY KEY,

    SpecializationName VARCHAR(150) NOT NULL UNIQUE,

    Description VARCHAR(500) NULL,

    IsActive BIT NOT NULL DEFAULT 1
);