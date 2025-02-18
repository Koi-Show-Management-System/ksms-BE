USE MASTER

CREATE DATABASE [KoiShowManagementSystem]
GO

USE [KoiShowManagementSystem]
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
    [Role] nvarchar(50) NOT NULL,
    [Status] varchar(20) DEFAULT 'active',
    [ConfirmationToken] NVARCHAR(255)  NULL,
    [IsConfirmed]  BIT NOT NULL DEFAULT '0',
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    [ResetPasswordOTP] nvarchar(10) NULL,
    [ResetPasswordOTPExpiry] datetime NULL
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
CREATE TABLE [dbo].[KoiShows](
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
-- ShowStaff table
CREATE TABLE [dbo].[ShowStaff](
  [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
  [KoiShowId] UNIQUEIDENTIFIER NOT NULL,
  [AccountId] UNIQUEIDENTIFIER NOT NULL,
  [AssignedBy] UNIQUEIDENTIFIER NOT NULL,
  [AssignedAt] datetime NOT NULL DEFAULT GETDATE(),
  FOREIGN KEY ([KoiShowId]) REFERENCES [KoiShows]([Id]),
  FOREIGN KEY ([AccountId]) REFERENCES [Accounts]([Id]),
  FOREIGN KEY ([AssignedBy]) REFERENCES [Accounts]([Id])
)
GO
-- Categories table
CREATE TABLE [dbo].[CompetitionCategories](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [KoiShowId] UNIQUEIDENTIFIER NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [SizeMin] decimal(5,2) NULL,
    [SizeMax] decimal(5,2) NULL,
    [Description] nvarchar(max) NULL,
    [MaxEntries] int NULL,
    [StartTime] datetime NULL,
    [EndTime] datetime NULL,
    [Status] varchar(20) NULL,
	[CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL
    FOREIGN KEY ([KoiShowId]) REFERENCES [KoiShows]([Id]),
)
GO
--CategoryVarieties table
CREATE TABLE [dbo].[CategoryVarieties](
  [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
  [VarietyId] UNIQUEIDENTIFIER NOT NULL,
  [CompetitionCategoryId] UNIQUEIDENTIFIER NOT NULL, 
      FOREIGN KEY ([VarietyId]) REFERENCES [Varieties]([Id]),
      FOREIGN KEY ([CompetitionCategoryId]) REFERENCES [CompetitionCategories]([Id]),
)
GO
-- RefereeAssignments table
CREATE TABLE [dbo].[RefereeAssignments](
   [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
   [CompetitionCategoryId] UNIQUEIDENTIFIER NOT NULL,
   [RefereeAccountId] UNIQUEIDENTIFIER NOT NULL,
   [RoundType] varchar(20) NOT NULL,
   [AssignedAt] datetime NOT NULL DEFAULT GETDATE(),
   [AssignedBy] UNIQUEIDENTIFIER NOT NULL,
   FOREIGN KEY ([RefereeAccountId]) REFERENCES [Accounts]([Id]),
   FOREIGN KEY ([AssignedBy]) REFERENCES [Accounts]([Id]),
   FOREIGN KEY ([CompetitionCategoryId]) REFERENCES [CompetitionCategories]([Id])
)
GO
-- KoiProfiles table
CREATE TABLE [dbo].[KoiProfiles](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [OwnerId] UNIQUEIDENTIFIER NOT NULL,
    [VarietyId] UNIQUEIDENTIFIER NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Size] decimal(5,2) NOT NULL,
    [Age] int NOT NULL,
    [Gender] nvarchar(10) NOT NULL,
    [Bloodline] nvarchar(100) NOT NULL,
    [Status] varchar(20) NOT NULL,
    [IsPublic] bit DEFAULT 0,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([OwnerId]) REFERENCES [Accounts]([Id]),
    FOREIGN KEY ([VarietyId]) REFERENCES [Varieties]([Id])
)
GO

-- Awards table
CREATE TABLE [dbo].[Awards](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [CompetitionCategoriesId] UNIQUEIDENTIFIER NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [AwardType] nvarchar(20) NULL,
    [PrizeValue] decimal(10,2) NULL,
    [Description] nvarchar(max) NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([CompetitionCategoriesId]) REFERENCES [CompetitionCategories]([Id])
)
GO

-- Rounds table
CREATE TABLE [dbo].[Rounds](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [CompetitionCategoriesId] UNIQUEIDENTIFIER NULL,
    [Name] nvarchar(50) NULL,
    [RoundOrder] int NULL,
    [RoundType] varchar(20) NOT NULL,
    [StartTime] datetime NULL,
    [EndTime] datetime NULL,
    [MinScoreToAdvance] decimal(5,2) NULL,
    [Status] varchar(20) NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([CompetitionCategoriesId]) REFERENCES [CompetitionCategories]([Id])
)
GO

-- Criteria table
CREATE TABLE [dbo].[Criteria](
     [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
     [Name] nvarchar(100) NOT NULL,
     [Description] nvarchar(max) NULL,
     [Order] int NULL,
     [CreatedAt] datetime NOT NULL,
     [UpdatedAt] datetime NULL,
)
GO
-- CriteriaCompetitionCategories table
CREATE TABLE [dbo].[CriteriaCompetitionCategories](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [CompetitionCategoryId] UNIQUEIDENTIFIER NOT NULL,
    [CriteriaId] UNIQUEIDENTIFIER NOT NULL,
    [RoundType] varchar(20) NULL,
    [Weight] decimal(3,2) NULL,
    [Order] int NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([CompetitionCategoryId]) REFERENCES [CompetitionCategories]([Id]),
    FOREIGN KEY ([CriteriaId]) REFERENCES [Criteria]([Id])
                                                  
)
GO



-- Registrations table
CREATE TABLE [dbo].[Registrations](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [KoiShowId] UNIQUEIDENTIFIER NOT NULL,
    [KoiProfileId] UNIQUEIDENTIFIER NOT NULL,
    [RegistrationNumber] nvarchar(100) NULL,
    [RegisterName] nvarchar(100) NOT NULL,
    [KoiSize] decimal(5,2) NOT NULL,
    [KoiAge] int NOT NULL,
    [CompetitionCategoryId] UNIQUEIDENTIFIER NULL,
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [RegistrationFee] decimal(18,2) NOT NULL,
    [Status] varchar(20) NULL,
    [QRCodeData] varchar(255) NULL,
    [Notes] nvarchar(max) NULL,
    [ApprovedAt] datetime NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([AccountId]) REFERENCES [Accounts]([Id]),
    FOREIGN KEY ([KoiShowId]) REFERENCES [KoiShows]([Id]),
    FOREIGN KEY ([KoiProfileId]) REFERENCES [KoiProfiles]([Id]),
    FOREIGN KEY ([CompetitionCategoryId]) REFERENCES [CompetitionCategories]([Id])
)
GO

-- Tanks table
CREATE TABLE [dbo].[Tanks](
  [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
  [KoiShowId] UNIQUEIDENTIFIER NOT NULL,
  [Name] nvarchar(100) NOT NULL,
  [Capacity] int NOT NULL,
  [WaterType] nvarchar(50) NULL,
  [Temperature] decimal(5, 2) NULL,
  [PHLevel] decimal(5, 2) NULL,
  [Size] decimal(10, 2) NULL,
  [Location] nvarchar(100) NULL,
  [Status] nvarchar(20) NULL, --Active, Maintenance, InActive
  [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
  [CreatedAt] datetime NOT NULL,
  [UpdatedAt] datetime NULL,
  FOREIGN KEY ([KoiShowId]) REFERENCES [KoiShows]([Id]),
  FOREIGN KEY ([CreatedBy]) REFERENCES [ShowStaff]([Id]),
)
GO
-- RegistrationRounds table
CREATE TABLE [dbo].[RegistrationRounds](
  [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
  [RegistrationId] UNIQUEIDENTIFIER NOT NULL,
  [RoundId] UNIQUEIDENTIFIER NOT NULL,
  [CheckInTime] datetime NULL,
  [CheckOutTime] datetime NULL,
  [TankId]  UNIQUEIDENTIFIER NOT NULL,
  [Status] varchar(20) NULL,
  [Notes] nvarchar(max) NULL,
  [CreatedAt] datetime NOT NULL,
  [UpdatedAt] datetime NULL,
  FOREIGN KEY ([RegistrationId]) REFERENCES [Registrations]([Id]),
  FOREIGN KEY ([RoundId]) REFERENCES [Rounds]([Id]),
  FOREIGN KEY ([TankId]) REFERENCES [Tanks]([Id]),
)
GO


-- ScoreDetails table
CREATE TABLE [dbo].[ScoreDetails](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [RefereeAccountId] UNIQUEIDENTIFIER NOT NULL,
    [RegistrationRoundId] UNIQUEIDENTIFIER NOT NULL,
    [InitialScore] decimal(5,2) NOT NULL,
    [TotalPointMinus] decimal(5,2) NOT NULL,
    [IsPublic] bit DEFAULT 0,
    [Comments] nvarchar(max) NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([RegistrationRoundId]) REFERENCES [RegistrationRounds]([Id]),
    FOREIGN KEY ([RefereeAccountId]) REFERENCES [RefereeAssignments]([Id]),
)
GO
-- ErrorTypes table
CREATE TABLE [dbo].[ErrorTypes] (
    [Id] UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    [CriteriaId]  UNIQUEIDENTIFIER NOT NULL,
    [Name] NVARCHAR(255) NOT NULL,
    [CreatedAt] [datetime] NOT NULL,
    [UpdatedAt] [datetime] NULL,
    FOREIGN KEY ([CriteriaId]) REFERENCES [Criteria]([Id])
)
GO
-- ScoreDetailErrors table
CREATE TABLE [dbo].[ScoreDetailErrors] (
   [Id] UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
   [ScoreDetailId]  UNIQUEIDENTIFIER NOT NULL,
   [ErrorTypeId]  UNIQUEIDENTIFIER NOT NULL,
   [Severity]  NVARCHAR(20) NOT NULL,
   [CreatedAt] [datetime] NOT NULL,
   [PointMinus] decimal(5,2) NOT NULL,
   FOREIGN KEY ([ScoreDetailId]) REFERENCES [ScoreDetails]([Id]),
   FOREIGN KEY ([ErrorTypeId]) REFERENCES [ErrorTypes]([Id])
)
GO
-- RoundResults table
CREATE TABLE [dbo].[RoundResults](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [RegistrationRoundsId] UNIQUEIDENTIFIER NOT NULL,
    [TotalScore] decimal(5,2) NOT NULL,
    [IsPublic] bit DEFAULT 0,
    [Comments] nvarchar(max) NULL,
    [Status] nvarchar(20) NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([RegistrationRoundsId]) REFERENCES [RegistrationRounds]([Id]),
)
GO




-- PaymentTypes table
CREATE TABLE [dbo].[PaymentTypes](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Name] nvarchar(255) NOT NULL,
    [Description] nvarchar(1000) NULL
)
GO

-- TicketType table
CREATE TABLE [dbo].[TicketTypes](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [KoiShowId] UNIQUEIDENTIFIER NOT NULL,
    [TicketType] nvarchar(50) NOT NULL,
    [Price] decimal(10,2) NOT NULL,
    [AvailableQuantity] int NOT NULL,
    FOREIGN KEY ([KoiShowId]) REFERENCES [KoiShows]([Id])
)
GO


-- TicketOrders table
CREATE TABLE [dbo].[TicketOrders](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [FullName] nvarchar(100) NOT NULL,
    [Email] varchar(100) NOT NULL,
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [PaymentTypeId] UNIQUEIDENTIFIER NULL,
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
    [TicketTypeId] UNIQUEIDENTIFIER NOT NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    FOREIGN KEY ([TicketOrderId]) REFERENCES [TicketOrders]([Id]),
    FOREIGN KEY ([TicketTypeId]) REFERENCES [TicketTypes]([Id])
)
GO

-- Tickets table
CREATE TABLE [dbo].[Tickets](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TicketOrderDetailId] UNIQUEIDENTIFIER NOT NULL,
    [QRCodeData] varchar(255) NULL,
    [ExpiredDate] datetime NOT NULL,
    [IsUsed] BIT NOT NULL DEFAULT '0',
    FOREIGN KEY ([TicketOrderDetailId]) REFERENCES [TicketOrderDetails]([Id])
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
    [KoiShowId] UNIQUEIDENTIFIER NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([AccountId]) REFERENCES [Accounts]([Id]),
    FOREIGN KEY ([KoiShowId]) REFERENCES [KoiShows]([Id])
)
GO

-- Sponsors table
CREATE TABLE [dbo].[Sponsors](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Name] nvarchar(255) NOT NULL,
    [LogoUrl] nvarchar(500) NULL,
    [InvestMoney] decimal(18,2) NOT NULL,
    [KoiShowId] UNIQUEIDENTIFIER NOT NULL,
    FOREIGN KEY ([KoiShowId]) REFERENCES [KoiShows]([Id])
)
GO

-- ShowStatus table
CREATE TABLE [dbo].[ShowStatus](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [KoiShowId] UNIQUEIDENTIFIER NOT NULL,
    [StatusName] nvarchar(50) NOT NULL,
    [Description] nvarchar(255) NULL,
    [StartDate] datetime NOT NULL,
    [EndDate] datetime NOT NULL,
    [IsActive] bit NOT NULL DEFAULT 0,
    FOREIGN KEY ([KoiShowId]) REFERENCES [KoiShows]([Id])
)
GO

-- Livestreams table
CREATE TABLE [dbo].[Livestreams](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [KoiShowId] UNIQUEIDENTIFIER NOT NULL,
    [StartTime] datetime NOT NULL,
    [EndTime] datetime NULL,
    [StreamUrl] nvarchar(500) NOT NULL,
    FOREIGN KEY ([KoiShowId]) REFERENCES [KoiShows]([Id])
)
GO

-- Votes table
CREATE TABLE [dbo].[Votes](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [RegistrationId] UNIQUEIDENTIFIER NOT NULL,
    [Prediction] nvarchar(max) NULL,
    [CreatedAt] datetime NOT NULL,
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
CREATE TABLE [dbo].[RegistrationPayments] (
      [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
      [RegistrationId] UNIQUEIDENTIFIER NOT NULL UNIQUE,
      [PaymentTypeId] UNIQUEIDENTIFIER NULL,
      [QRCodeData] varchar(255) NULL,
      [PaidAmount] DECIMAL(18,2) NOT NULL,
      [PaymentDate] DATETIME NOT NULL DEFAULT GETDATE(),
      [PaymentMethod] NVARCHAR(50) NOT NULL,
      [Status] VARCHAR(20) NOT NULL,
      FOREIGN KEY ([RegistrationId]) REFERENCES [Registrations]([Id]),
      FOREIGN KEY ([PaymentTypeId]) REFERENCES [PaymentTypes]([Id])
)
GO

-- CheckInLogs table
CREATE TABLE [dbo].[CheckInLogs](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TicketId] UNIQUEIDENTIFIER NULL UNIQUE ,
    [RegistrationPaymentId] UNIQUEIDENTIFIER NULL UNIQUE,
    [CheckInTime] datetime DEFAULT GETDATE(),
    [CheckInLocation] nvarchar(100) NULL,
    [CheckedInBy] UNIQUEIDENTIFIER NULL,
    [Status] varchar(20) NULL,
    [Notes] nvarchar(255) NULL,
    FOREIGN KEY ([TicketId]) REFERENCES [Tickets]([Id]),
    FOREIGN KEY ([RegistrationPaymentId]) REFERENCES [RegistrationPayments]([Id]),
    FOREIGN KEY ([CheckedInBy]) REFERENCES [ShowStaff]([Id]),
    CONSTRAINT CHK_CheckInLog_ForeignKey CHECK (
        (TicketId IS NOT NULL AND RegistrationPaymentId IS NULL)
            OR (TicketId IS NULL AND RegistrationPaymentId IS NOT NULL)
        )
)
GO
-- CheckoutLogs table
CREATE TABLE [dbo].[CheckOutLogs](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [RegistrationId] UNIQUEIDENTIFIER NULL UNIQUE,
    [ImgCheckOut] nvarchar(255) NOT NULL,
    [CheckOutTime] datetime DEFAULT GETDATE(),
    [CheckedOutBy] UNIQUEIDENTIFIER NULL,
    [Notes] nvarchar(255) NULL,
    FOREIGN KEY ([RegistrationId]) REFERENCES [Registrations]([Id]),
    FOREIGN KEY ([CheckedOutBy]) REFERENCES [ShowStaff]([Id]),
)
GO
-- ShowRules table
CREATE TABLE [dbo].[ShowRules](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [KoiShowId] UNIQUEIDENTIFIER NOT NULL,
    [Title] [nvarchar](200) NOT NULL,
    [Content] [nvarchar](max) NOT NULL,
    [CreatedAt] [datetime] NOT NULL,
    [UpdatedAt] [datetime] NULL,
	FOREIGN KEY ([KoiShowId]) REFERENCES [KoiShows]([Id])
)
GO
-- Media table
CREATE TABLE [dbo].[KoiMedia] (
   [Id] UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
   [MediaUrl] NVARCHAR(MAX) NOT NULL, -- URL của hình ảnh hoặc video
   [MediaType] NVARCHAR(10) NOT NULL, -- Loại phương tiện: 'Image' hoặc 'Video'
   [KoiProfileId] UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES KoiProfiles(Id),
   [RegistrationId] UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES Registrations(Id),
   CONSTRAINT CHK_MediaOf_ForeignKey CHECK (
       (KoiProfileId IS NOT NULL AND RegistrationId IS NULL)
           OR (KoiProfileId IS NULL AND RegistrationId IS NOT NULL)
       )
)
GO

