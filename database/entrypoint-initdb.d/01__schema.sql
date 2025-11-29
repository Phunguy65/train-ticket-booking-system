USE master;

GO
GO IF NOT EXISTS (
	SELECT
		*
	FROM
		sys.databases
	WHERE
		name = 'TrainTicketBooking'
) BEGIN
CREATE DATABASE TrainTicketBooking;

PRINT 'Created database TrainTicketBooking';

END ELSE BEGIN PRINT 'Database TrainTicketBooking already exists';

END
GO
-- Bảng người dùng
CREATE TABLE [User] (
	UserId INT IDENTITY(1, 1) NOT NULL,
	Username NVARCHAR(50) NOT NULL,
	PasswordHash NVARCHAR(255) NOT NULL,
	FullName NVARCHAR(100) NOT NULL,
	Email NVARCHAR(100) NOT NULL,
	PhoneNumber NVARCHAR(20) NULL,
	[Role] NVARCHAR(20) NOT NULL,
	CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_User_CreatedAt DEFAULT (GETDATE()),
	IsActive BIT NOT NULL CONSTRAINT DF_User_IsActive DEFAULT (1),
	CONSTRAINT PK_User PRIMARY KEY CLUSTERED (UserId),
	CONSTRAINT UQ_User_Username UNIQUE (Username),
	CONSTRAINT UQ_User_Email UNIQUE (Email),
	CONSTRAINT CK_User_Role CHECK ([Role] IN ('Admin', 'Customer'))
);

-- Bảng chuyến tàu
CREATE TABLE Train (
	TrainId INT IDENTITY(1, 1) NOT NULL,
	TrainNumber NVARCHAR(20) NOT NULL,
	TrainName NVARCHAR(100) NOT NULL,
	DepartureStation NVARCHAR(100) NOT NULL,
	ArrivalStation NVARCHAR(100) NOT NULL,
	DepartureTime DATETIME2 NOT NULL,
	ArrivalTime DATETIME2 NOT NULL,
	TotalSeats INT NOT NULL CONSTRAINT DF_Train_TotalSeats DEFAULT (10),
	TicketPrice DECIMAL(10, 2) NOT NULL,
	[Status] NVARCHAR(20) NOT NULL CONSTRAINT DF_Train_Status DEFAULT ('Active'),
	CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Train_CreatedAt DEFAULT (GETDATE()),
	CONSTRAINT PK_Train PRIMARY KEY CLUSTERED (TrainId),
	CONSTRAINT UQ_Train_TrainNumber UNIQUE (TrainNumber),
	CONSTRAINT CK_Train_Status CHECK ([Status] IN ('Active', 'Cancelled', 'Completed'))
);

-- Bảng ghế ngồi
CREATE TABLE Seat (
	SeatId INT IDENTITY(1, 1) NOT NULL,
	TrainId INT NOT NULL,
	SeatNumber NVARCHAR(10) NOT NULL,
	IsAvailable BIT NOT NULL CONSTRAINT DF_Seat_IsAvailable DEFAULT (1),
	[Version] INT NOT NULL CONSTRAINT DF_Seat_Version DEFAULT (0),
	CONSTRAINT PK_Seat PRIMARY KEY CLUSTERED (SeatId),
	CONSTRAINT UQ_Seat_TrainId_SeatNumber UNIQUE (TrainId, SeatNumber),
	CONSTRAINT FK_Seat_TrainId_Train FOREIGN KEY (TrainId) REFERENCES Train (TrainId)
);

-- Bảng đặt vé
CREATE TABLE Booking (
	BookingId INT IDENTITY(1, 1) NOT NULL,
	UserId INT NOT NULL,
	TrainId INT NOT NULL,
	SeatId INT NOT NULL,
	BookingStatus NVARCHAR(20) NOT NULL,
	BookingDate DATETIME2 NOT NULL CONSTRAINT DF_Booking_BookingDate DEFAULT (GETDATE()),
	TotalAmount DECIMAL(10, 2) NOT NULL,
	PaymentStatus NVARCHAR(20) NOT NULL CONSTRAINT DF_Booking_PaymentStatus DEFAULT ('Pending'),
	CancelledAt DATETIME2 NULL,
	CONSTRAINT PK_Booking PRIMARY KEY CLUSTERED (BookingId),
	CONSTRAINT FK_Booking_UserId_User FOREIGN KEY (UserId) REFERENCES [User] (UserId),
	CONSTRAINT FK_Booking_TrainId_Train FOREIGN KEY (TrainId) REFERENCES Train (TrainId),
	CONSTRAINT FK_Booking_SeatId_Seat FOREIGN KEY (SeatId) REFERENCES Seat (SeatId),
	CONSTRAINT CK_Booking_BookingStatus CHECK (
		BookingStatus IN ('Pending', 'Confirmed', 'Cancelled')
	),
	CONSTRAINT CK_Booking_PaymentStatus CHECK (PaymentStatus IN ('Pending', 'Paid', 'Refunded'))
);

-- Bảng lịch sử giao dịch
CREATE TABLE AuditLog (
	LogId INT IDENTITY(1, 1) NOT NULL,
	UserId INT NULL,
	[Action] NVARCHAR(100) NOT NULL,
	EntityType NVARCHAR(50) NULL,
	EntityId INT NULL,
	Details NVARCHAR(MAX) NULL,
	CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_AuditLog_CreatedAt DEFAULT (GETDATE()),
	CONSTRAINT PK_AuditLog PRIMARY KEY CLUSTERED (LogId),
	CONSTRAINT FK_AuditLog_UserId_User FOREIGN KEY (UserId) REFERENCES [User] (UserId)
);

-- Index cho performance
CREATE NONCLUSTERED
INDEX IX_Seat_TrainId_IsAvailable ON Seat (TrainId, IsAvailable);

CREATE NONCLUSTERED
INDEX IX_Booking_UserId ON Booking (UserId);

CREATE NONCLUSTERED
INDEX IX_Booking_TrainId ON Booking (TrainId);

CREATE NONCLUSTERED
INDEX IX_Train_DepartureTime ON Train (DepartureTime);
