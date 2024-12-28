-- Crear tabla Hotels
CREATE TABLE Hotels (
    HotelId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(255) NOT NULL,
    Location NVARCHAR(255) NOT NULL,
    BasePrice DECIMAL(10, 2) NOT NULL,
    Status BIT NOT NULL DEFAULT 1, -- 1 para habilitado, 0 para deshabilitado
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
);

-- Crear tabla Rooms
CREATE TABLE Rooms (
    RoomId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    HotelId UNIQUEIDENTIFIER NOT NULL,
    RoomType NVARCHAR(100) NOT NULL,
    BaseCost DECIMAL(10, 2) NOT NULL,
    TaxPercentage DECIMAL(5, 2) NOT NULL,
    Location NVARCHAR(255) NOT NULL,
    Status BIT NOT NULL DEFAULT 1, -- 1 para habilitado, 0 para deshabilitado
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    CONSTRAINT FK_Rooms_Hotels FOREIGN KEY (HotelId) REFERENCES Hotels(HotelId) ON DELETE CASCADE
);

-- Crear tabla Reservations
CREATE TABLE Reservations (
    ReservationId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RoomId UNIQUEIDENTIFIER NOT NULL,
    CheckInDate DATE NOT NULL,
    CheckOutDate DATE NOT NULL,
    TotalGuests INT NOT NULL,
    TotalCost DECIMAL(10, 2) NOT NULL,
    EmailNotification BIT NOT NULL DEFAULT 0, -- 0 para no enviado, 1 para enviado
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Reservations_Rooms FOREIGN KEY (RoomId) REFERENCES Rooms(RoomId) ON DELETE CASCADE
);

-- Crear tabla Guests
CREATE TABLE Guests (
    GuestId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ReservationId UNIQUEIDENTIFIER NOT NULL,
    FullName NVARCHAR(255) NOT NULL,
    DateOfBirth DATE NOT NULL,
    Gender NVARCHAR(10) NOT NULL,
    DocumentType NVARCHAR(50) NOT NULL,
    DocumentNumber NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    CONSTRAINT FK_Guests_Reservations FOREIGN KEY (ReservationId) REFERENCES Reservations(ReservationId) ON DELETE CASCADE
);

-- Crear tabla EmergencyContacts
CREATE TABLE EmergencyContacts (
    ContactId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    GuestId UNIQUEIDENTIFIER NOT NULL,
    FullName NVARCHAR(255) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    CONSTRAINT FK_EmergencyContacts_Guests FOREIGN KEY (GuestId) REFERENCES Guests(GuestId) ON DELETE CASCADE
);
