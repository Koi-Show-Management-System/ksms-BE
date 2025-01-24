USE MASTER

CREATE DATABASE [KoiShowManagementSystem]
GO

USE [KoiShowManagementSystem]
GO

-- Roles table
CREATE TABLE [dbo].[Roles](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Name] nvarchar(50) NOT NULL UNIQUE,
    [Description] nvarchar(255) NULL
)
GO

-- Accounts table
CREATE TABLE [dbo].[Accounts](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Email] varchar(100) NULL UNIQUE,
    [HashedPassword] varchar(255) NOT NULL,
    [Username] varchar(50) NOT NULL UNIQUE,
    [FullName] nvarchar(100) NULL,
    [Phone] varchar(20) NULL,
    [Avatar] nvarchar(255) NULL,
    [RoleId] UNIQUEIDENTIFIER NULL,
    [Status] varchar(20) DEFAULT 'active',
    [ConfirmationToken] NVARCHAR(255)  NULL,
    [IsConfirmed]  BIT NOT NULL DEFAULT '0',
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([RoleId]) REFERENCES [Roles]([Id])
)
GO

-- Varieties table
CREATE TABLE [dbo].[Varieties](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Name] nvarchar(50) NOT NULL,
    [Description] nvarchar(max) NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL
)
GO

-- Shows table
CREATE TABLE [dbo].[Shows](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Name] nvarchar(100) NOT NULL,
    [StartDate] datetime NULL,
    [EndDate] datetime NULL,
    [StartExhibitionDate] datetime NULL,
    [EndExhibitionDate] datetime NULL,
    [Location] nvarchar(200) NULL,
    [Description] nvarchar(max) NULL,
    [RegistrationDeadline] date NULL,
    [MinParticipants] int NULL,
    [MaxParticipants] int NULL,
    [HasGrandChampion] bit DEFAULT 0,
    [HasBestInShow] bit DEFAULT 0,
    [ImgURL] nvarchar(255) NULL,
    [RegistrationFee] decimal(18,2) NOT NULL,
    [Status] varchar(20) NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL
)
GO

-- Categories table
CREATE TABLE [dbo].[Categories](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [ShowId] UNIQUEIDENTIFIER NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [SizeMin] decimal(5,2) NULL,
    [SizeMax] decimal(5,2) NULL,
    [VarietyId] UNIQUEIDENTIFIER NULL,
    [Description] nvarchar(max) NULL,
    [MaxEntries] int NULL,
    [StartTime] datetime NULL,
    [EndTime] datetime NULL,
    [Status] varchar(20) NULL,
	[CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL
    FOREIGN KEY ([ShowId]) REFERENCES [Shows]([Id]),
    FOREIGN KEY ([VarietyId]) REFERENCES [Varieties]([Id])
)
GO


-- KoiProfiles table
CREATE TABLE [dbo].[KoiProfiles](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [OwnerId] UNIQUEIDENTIFIER NOT NULL,
    [VarietyId] UNIQUEIDENTIFIER NOT NULL,
    [Name] nvarchar(100) NULL,
    [Size] decimal(5,2) NULL,
    [Age] int NULL,
    [Gender] varchar(10) NULL,
    [Bloodline] nvarchar(100) NULL,
    [Status] varchar(20) NULL,
    [ImgURL] nvarchar(255) NULL,
    [VideoURL] nvarchar(255) NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([OwnerId]) REFERENCES [Accounts]([Id]),
    FOREIGN KEY ([VarietyId]) REFERENCES [Varieties]([Id])
)
GO

-- Awards table
CREATE TABLE [dbo].[Awards](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [CategoryId] UNIQUEIDENTIFIER NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [AwardType] varchar(20) NULL,
    [PrizeValue] decimal(10,2) NULL,
    [Description] nvarchar(max) NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([CategoryId]) REFERENCES [Categories]([Id])
)
GO

-- Rounds table
CREATE TABLE [dbo].[Rounds](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [CategoryId] UNIQUEIDENTIFIER NULL,
    [Name] nvarchar(50) NULL,
    [RoundOrder] int NULL,
    [RoundType] varchar(20) NOT NULL,
    [StartTime] datetime NULL,
    [EndTime] datetime NULL,
    [MinScoreToAdvance] decimal(5,2) NULL,
    [Status] varchar(20) NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([CategoryId]) REFERENCES [Categories]([Id])
)
GO

-- CriteriaGroups table
CREATE TABLE [dbo].[CriteriaGroups](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [CategoryId] UNIQUEIDENTIFIER NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [RoundType] varchar(20) NULL,
    [Description] nvarchar(max) NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([CategoryId]) REFERENCES [Categories]([Id])
)
GO

-- Criteria table
CREATE TABLE [dbo].[Criteria](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [CriteriaGroupId] UNIQUEIDENTIFIER NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(max) NULL,
    [MaxScore] decimal(5,2) NULL,
    [Weight] decimal(3,2) NULL,
    [Order] int NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([CriteriaGroupId]) REFERENCES [CriteriaGroups]([Id])
)
GO

-- Registrations table
CREATE TABLE [dbo].[Registrations](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [RegistrationNumber] nvarchar(100) NULL,
    [VarietyId] UNIQUEIDENTIFIER NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Size] decimal(5,2) NOT NULL,
    [Age] int NOT NULL,
    [Gender] varchar(10) NOT NULL,
    [Bloodline] nvarchar(100) NULL,
    [ImgURL] nvarchar(255) NOT NULL,
    [VideoURL] nvarchar(255) NOT NULL,
    [CategoryId] UNIQUEIDENTIFIER NOT NULL,
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [RegistrationFee] decimal(18,2) NOT NULL,
    [Status] varchar(20) NULL,
    [Notes] nvarchar(max) NULL,
    [ApprovedAt] datetime NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([AccountId]) REFERENCES [Accounts]([Id]),
    FOREIGN KEY ([CategoryId]) REFERENCES [Categories]([Id])
)
GO

-- Scores table
CREATE TABLE [dbo].[Scores](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [RegistrationId] UNIQUEIDENTIFIER NOT NULL,
    [RoundId] UNIQUEIDENTIFIER NOT NULL,
    [RefereeAccountId] UNIQUEIDENTIFIER NOT NULL,
    [CriteriaId] UNIQUEIDENTIFIER NOT NULL,
    [Score] decimal(5,2) NULL,
    [Comments] nvarchar(max) NULL,
    [Status] varchar(20) NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([RegistrationId]) REFERENCES [Registrations]([Id]),
    FOREIGN KEY ([RoundId]) REFERENCES [Rounds]([Id]),
    FOREIGN KEY ([RefereeAccountId]) REFERENCES [Accounts]([Id]),
    FOREIGN KEY ([CriteriaId]) REFERENCES [Criteria]([Id])
)
GO
-- Results table
CREATE TABLE [dbo].[Results](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [RegistrationId] UNIQUEIDENTIFIER NOT NULL,
    [FinalScore] decimal(5,2) NOT NULL,
    [Rank] int NULL,
    [AwardId] UNIQUEIDENTIFIER NOT NULL,
    [Comments] nvarchar(max) NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([RegistrationId]) REFERENCES [Registrations]([Id]),
    FOREIGN KEY ([AwardId]) REFERENCES [Awards]([Id])
)
GO

-- ShowStaff table
CREATE TABLE [dbo].[ShowStaff](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [ShowId] UNIQUEIDENTIFIER NOT NULL,
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [AssignedBy] UNIQUEIDENTIFIER NOT NULL,
    [AssignedAt] datetime NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY ([ShowId]) REFERENCES [Shows]([Id]),
    FOREIGN KEY ([AccountId]) REFERENCES [Accounts]([Id]),
	FOREIGN KEY ([AssignedBy]) REFERENCES [Accounts]([Id])
)
GO

-- PaymentTypes table
CREATE TABLE [dbo].[PaymentTypes](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Name] nvarchar(255) NOT NULL,
    [Description] nvarchar(1000) NULL
)
GO

-- Tickets table
CREATE TABLE [dbo].[Tickets](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [ShowId] UNIQUEIDENTIFIER NOT NULL,
    [TicketType] nvarchar(50) NOT NULL,
    [Price] decimal(10,2) NOT NULL,
    [AvailableQuantity] int NOT NULL,
    FOREIGN KEY ([ShowId]) REFERENCES [Shows]([Id])
)
GO

-- TicketOrders table
CREATE TABLE [dbo].[TicketOrders](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [PaymentTypeId] UNIQUEIDENTIFIER NOT NULL,
    [OrderDate] datetime NOT NULL DEFAULT GETDATE(),
    [TotalAmount] decimal(18,2) NOT NULL,
    [PaymentMethod] nvarchar(50) NULL,
    [Status] nvarchar(50) NULL,
    [Notes] nvarchar(max) NULL,
    FOREIGN KEY ([AccountId]) REFERENCES [Accounts]([Id]),
    FOREIGN KEY ([PaymentTypeId]) REFERENCES [PaymentTypes]([Id])
)
GO

-- TicketOrderDetails table
CREATE TABLE [dbo].[TicketOrderDetails](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TicketOrderId] UNIQUEIDENTIFIER NOT NULL,
    [TicketId] UNIQUEIDENTIFIER NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    FOREIGN KEY ([TicketOrderId]) REFERENCES [TicketOrders]([Id]),
    FOREIGN KEY ([TicketId]) REFERENCES [Tickets]([Id])
)
GO





-- CategoryTanks table
CREATE TABLE [dbo].[CategoryTanks](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [CategoryId] UNIQUEIDENTIFIER NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Capacity] decimal(10,2) NULL,
    [Dimensions] varchar(50) NULL,
    [Location] nvarchar(100) NULL,
    [Status] varchar(20) NULL,
    [Temperature] decimal(4,1) NULL,
    [PhLevel] decimal(3,1) NULL,
    [CreatedAt] datetime NOT NULL,
	[UpdatedAt] datetime NULL,
    FOREIGN KEY ([CategoryId]) REFERENCES [Categories]([Id])
)
GO

-- FishTankAssignments table
CREATE TABLE [dbo].[FishTankAssignments](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [RegistrationId] UNIQUEIDENTIFIER NOT NULL,
    [TankId] UNIQUEIDENTIFIER NULL,
    [CheckInTime] datetime NULL,
    [CheckOutTime] datetime NULL,
    [Status] varchar(20) NULL,
    [Notes] nvarchar(max) NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([RegistrationId]) REFERENCES [Registrations]([Id]),
    FOREIGN KEY ([TankId]) REFERENCES [CategoryTanks]([Id])
)
GO
-- GrandChampionContenders table
CREATE TABLE [dbo].[GrandChampionContenders](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [RoundId] UNIQUEIDENTIFIER NOT NULL,
    [RegistrationId] UNIQUEIDENTIFIER NOT NULL,
    [QualificationType] varchar(50) NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([RoundId]) REFERENCES [Rounds]([Id]),
    FOREIGN KEY ([RegistrationId]) REFERENCES [Registrations]([Id])
)
GO



-- Notifications table
CREATE TABLE [dbo].[Notifications](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [AccountId] UNIQUEIDENTIFIER NULL,
    [Content] nvarchar(max) NOT NULL,
    [SentDate] datetime NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY ([AccountId]) REFERENCES [Accounts]([Id])
)
GO

-- Feedbacks table
CREATE TABLE [dbo].[Feedbacks](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [ShowId] UNIQUEIDENTIFIER NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([AccountId]) REFERENCES [Accounts]([Id]),
    FOREIGN KEY ([ShowId]) REFERENCES [Shows]([Id])
)
GO

-- Sponsors table
CREATE TABLE [dbo].[Sponsors](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Name] nvarchar(255) NOT NULL,
    [LogoUrl] nvarchar(500) NULL,
    [ShowId] UNIQUEIDENTIFIER NOT NULL,
    FOREIGN KEY ([ShowId]) REFERENCES [Shows]([Id])
)
GO

-- ShowStatistics table
CREATE TABLE [dbo].[ShowStatistics](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [ShowId] UNIQUEIDENTIFIER NULL,
    [MetricName] nvarchar(255) NOT NULL,
    [MetricValue] decimal(10,2) NOT NULL,
    FOREIGN KEY ([ShowId]) REFERENCES [Shows]([Id])
)
GO

-- ShowStatus table
CREATE TABLE [dbo].[ShowStatus](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [ShowId] UNIQUEIDENTIFIER NOT NULL,
    [StatusName] nvarchar(50) NOT NULL,
    [Description] nvarchar(255) NULL,
    [StartDate] datetime NOT NULL,
    [EndDate] datetime NOT NULL,
    [IsActive] bit NOT NULL DEFAULT 0,
    FOREIGN KEY ([ShowId]) REFERENCES [Shows]([Id])
)
GO

-- Livestreams table
CREATE TABLE [dbo].[Livestreams](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [ShowId] UNIQUEIDENTIFIER NOT NULL,
    [StartTime] datetime NOT NULL,
    [EndTime] datetime NULL,
    [StreamUrl] nvarchar(500) NOT NULL,
    FOREIGN KEY ([ShowId]) REFERENCES [Shows]([Id])
)
GO

-- Votes table
CREATE TABLE [dbo].[Votes](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [RegistrationId] UNIQUEIDENTIFIER NOT NULL,
    [Prediction] nvarchar(max) NULL,
    [CreatedAt] datetime NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([AccountId]) REFERENCES [Accounts]([Id]),
    FOREIGN KEY ([RegistrationId]) REFERENCES [Registrations]([Id])
)
GO

-- BlogCategory table
CREATE TABLE [dbo].[BlogCategories](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL
)
GO

-- BlogsNews table
CREATE TABLE [dbo].[BlogsNews](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Title] nvarchar(255) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
	[BlogCategoryId] UNIQUEIDENTIFIER NOT NULL,
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [ImgURL] nvarchar(255) NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
	FOREIGN KEY ([AccountId]) REFERENCES [Accounts]([Id]),
	FOREIGN KEY ([BlogCategoryId]) REFERENCES [BlogCategories]([Id]),
)
GO
-- RegistrationPayments table
CREATE TABLE [dbo].[RegistrationPayments](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [RegistrationId] UNIQUEIDENTIFIER NOT NULL,
    [PaymentTypeId] UNIQUEIDENTIFIER NOT NULL,
    [PaidAmount] decimal(18,2) NOT NULL,
    [PaymentDate] datetime NOT NULL DEFAULT GETDATE(),
    [PaymentMethod] nvarchar(50) NOT NULL,
    [Status] varchar(20) NOT NULL,
    FOREIGN KEY ([RegistrationId]) REFERENCES [Registrations]([Id]),
    FOREIGN KEY ([PaymentTypeId]) REFERENCES [PaymentTypes]([Id])
)
GO
-- QRCodes table
CREATE TABLE [dbo].[QRCodes](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TicketOrderDetailId] UNIQUEIDENTIFIER NULL,
    [RegistrationPaymentId] UNIQUEIDENTIFIER NULL,
    [QRCodeData] varchar(255) NOT NULL,
    [ExpiryDate] datetime NULL,
    [IsActive] bit DEFAULT 1,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([TicketOrderDetailId]) REFERENCES [TicketOrderDetails]([Id]),
    FOREIGN KEY ([RegistrationPaymentId]) REFERENCES [RegistrationPayments]([Id])
)
GO
-- RefereeAssignments table (based on the image)
CREATE TABLE [dbo].[RefereeAssignments](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [CategoryId] UNIQUEIDENTIFIER NOT NULL,
    [RefereeAccountId] UNIQUEIDENTIFIER NOT NULL,
    [AssignedAt] datetime NOT NULL DEFAULT GETDATE(),
    [AssignedBy] UNIQUEIDENTIFIER NOT NULL,
    FOREIGN KEY ([RefereeAccountId]) REFERENCES [Accounts]([Id]),
    FOREIGN KEY ([AssignedBy]) REFERENCES [Accounts]([Id]),
	FOREIGN KEY ([CategoryId]) REFERENCES [Categories]([Id])
)
GO
-- CheckInLogs table
CREATE TABLE [dbo].[CheckInLogs](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [QRCodeId] UNIQUEIDENTIFIER NOT NULL,
    [CheckInTime] datetime DEFAULT GETDATE(),
    [CheckInLocation] nvarchar(100) NULL,
    [CheckedInBy] UNIQUEIDENTIFIER NULL,
    [Status] varchar(20) NULL,
    [Notes] nvarchar(255) NULL,
    FOREIGN KEY ([QRCodeId]) REFERENCES [QRCodes]([Id]),
    FOREIGN KEY ([CheckedInBy]) REFERENCES [Accounts]([Id])
)
GO

CREATE TABLE [dbo].[ShowRules](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [ShowId] UNIQUEIDENTIFIER NOT NULL,
    [Title] [nvarchar](200) NOT NULL,
    [Content] [nvarchar](max) NOT NULL,
    [CreatedAt] [datetime] NOT NULL,
    [UpdatedAt] [datetime] NULL,
	FOREIGN KEY ([ShowId]) REFERENCES [Shows]([Id])
)
GO