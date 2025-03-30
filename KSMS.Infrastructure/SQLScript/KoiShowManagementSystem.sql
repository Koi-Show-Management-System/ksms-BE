USE MASTER

CREATE DATABASE [KoiShowManagementSystem]
GO

USE [KoiShowManagementSystem]
GO


-- Accounts table
CREATE TABLE [dbo].[Account](
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
CREATE TABLE [dbo].[Variety](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Name] nvarchar(50) NOT NULL,
    [Description] nvarchar(max) NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL
)
GO

-- Shows table
CREATE TABLE [dbo].[KoiShow](
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
  FOREIGN KEY ([KoiShowId]) REFERENCES [KoiShow]([Id]),
  FOREIGN KEY ([AccountId]) REFERENCES [Account]([Id]),
  FOREIGN KEY ([AssignedBy]) REFERENCES [Account]([Id])
)
GO
-- Categories table
CREATE TABLE [dbo].[CompetitionCategory](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [KoiShowId] UNIQUEIDENTIFIER NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [SizeMin] decimal(5,2) NULL,
    [SizeMax] decimal(5,2) NULL,
    [Description] nvarchar(max) NULL,
    [MaxEntries] int NULL,
    [HasTank] BIT NOT NULL DEFAULT '0',
    [RegistrationFee] decimal(18,2) NOT NULL,
    [StartTime] datetime NULL,
    [EndTime] datetime NULL,
    [Status] varchar(20) NULL,
	[CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL
    FOREIGN KEY ([KoiShowId]) REFERENCES [KoiShow]([Id]),
)
GO
--CategoryVarieties table
CREATE TABLE [dbo].[CategoryVariety](
  [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
  [VarietyId] UNIQUEIDENTIFIER NOT NULL,
  [CompetitionCategoryId] UNIQUEIDENTIFIER NOT NULL, 
      FOREIGN KEY ([VarietyId]) REFERENCES [Variety]([Id]),
      FOREIGN KEY ([CompetitionCategoryId]) REFERENCES [CompetitionCategory]([Id]),
)
GO
-- RefereeAssignments table
CREATE TABLE [dbo].[RefereeAssignment](
   [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
   [CompetitionCategoryId] UNIQUEIDENTIFIER NOT NULL,
   [RefereeAccountId] UNIQUEIDENTIFIER NOT NULL,
   [RoundType] varchar(20) NOT NULL,
   [AssignedAt] datetime NOT NULL DEFAULT GETDATE(),
   [AssignedBy] UNIQUEIDENTIFIER NOT NULL,
   FOREIGN KEY ([RefereeAccountId]) REFERENCES [Account]([Id]),
   FOREIGN KEY ([AssignedBy]) REFERENCES [Account]([Id]),
   FOREIGN KEY ([CompetitionCategoryId]) REFERENCES [CompetitionCategory]([Id])
)
GO
-- KoiProfiles table
CREATE TABLE [dbo].[KoiProfile](
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
    FOREIGN KEY ([OwnerId]) REFERENCES [Account]([Id]),
    FOREIGN KEY ([VarietyId]) REFERENCES [Variety]([Id])
)
GO

-- Awards table
CREATE TABLE [dbo].[Award](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [CompetitionCategoriesId] UNIQUEIDENTIFIER NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [AwardType] nvarchar(20) NULL,
    [PrizeValue] decimal(18,2) NULL,
    [Description] nvarchar(max) NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([CompetitionCategoriesId]) REFERENCES [CompetitionCategory]([Id])
)
GO

-- Rounds table
CREATE TABLE [dbo].[Round](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [CompetitionCategoriesId] UNIQUEIDENTIFIER NULL,
    [Name] nvarchar(50) NULL,
    [RoundOrder] int NULL,
    [RoundType] varchar(20) NOT NULL,
    [StartTime] datetime NULL,
    [EndTime] datetime NULL,
    [NumberOfRegistrationToAdvance] int NULL,
    [Status] varchar(20) NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([CompetitionCategoriesId]) REFERENCES [CompetitionCategory]([Id])
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
CREATE TABLE [dbo].[CriteriaCompetitionCategory](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [CompetitionCategoryId] UNIQUEIDENTIFIER NOT NULL,
    [CriteriaId] UNIQUEIDENTIFIER NOT NULL,
    [RoundType] varchar(20) NULL,
    [Weight] decimal(3,2) NULL,
    [Order] int NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([CompetitionCategoryId]) REFERENCES [CompetitionCategory]([Id]),
    FOREIGN KEY ([CriteriaId]) REFERENCES [Criteria]([Id])
                                                  
)
GO



-- Registrations table
CREATE TABLE [dbo].[Registration](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [KoiShowId] UNIQUEIDENTIFIER NOT NULL,
    [KoiProfileId] UNIQUEIDENTIFIER NOT NULL,
    [RegistrationNumber] nvarchar(100) NULL,
    [RegisterName] nvarchar(100) NOT NULL,
    [KoiSize] decimal(5,2) NOT NULL,
    [KoiAge] int NOT NULL,
    [CompetitionCategoryId] UNIQUEIDENTIFIER NOT NULL,
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [RegistrationFee] decimal(18,2) NOT NULL,
    [Rank] int NULL,
    [Status] varchar(20) NULL,
    [QRCodeData] varchar(255) NULL,
    [Notes] nvarchar(max) NULL,
    [ApprovedAt] datetime NULL,
    [CheckInExpiredDate] datetime NULL,
    [IsCheckedIn] BIT NOT NULL DEFAULT '0',
    [CheckInTime] datetime NULL,
    [CheckInLocation] nvarchar(100) NULL,
    [CheckedInBy] UNIQUEIDENTIFIER NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([AccountId]) REFERENCES [Account]([Id]),
    FOREIGN KEY ([KoiShowId]) REFERENCES [KoiShow]([Id]),
    FOREIGN KEY ([KoiProfileId]) REFERENCES [KoiProfile]([Id]),
    FOREIGN KEY ([CompetitionCategoryId]) REFERENCES [CompetitionCategory]([Id]),
    FOREIGN KEY ([CheckedInBy]) REFERENCES [Account]([Id])
)
GO

-- Tanks table
CREATE TABLE [dbo].[Tank](
  [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
  [CompetitionCategoryId] UNIQUEIDENTIFIER NOT NULL,
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
  FOREIGN KEY ([CompetitionCategoryId]) REFERENCES [CompetitionCategory]([Id]),
  FOREIGN KEY ([CreatedBy]) REFERENCES [Account]([Id]),
)
GO
-- RegistrationRounds table
CREATE TABLE [dbo].[RegistrationRound](
  [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
  [RegistrationId] UNIQUEIDENTIFIER NOT NULL,
  [RoundId] UNIQUEIDENTIFIER NOT NULL,
  [CheckInTime] datetime NULL,
  [CheckOutTime] datetime NULL,
  [TankId]  UNIQUEIDENTIFIER NULL,
  [Status] varchar(20) NULL,
  [Notes] nvarchar(max) NULL,
  [Rank] int NULL,
  [CreatedAt] datetime NOT NULL,
  [UpdatedAt] datetime NULL,
  FOREIGN KEY ([RegistrationId]) REFERENCES [Registration]([Id]),
  FOREIGN KEY ([RoundId]) REFERENCES [Round]([Id]),
  FOREIGN KEY ([TankId]) REFERENCES [Tank]([Id]),
)
GO


-- ScoreDetails table
CREATE TABLE [dbo].[ScoreDetail](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [RefereeAccountId] UNIQUEIDENTIFIER NOT NULL,
    [RegistrationRoundId] UNIQUEIDENTIFIER NOT NULL,
    [InitialScore] decimal(5,2) NOT NULL,
    [TotalPointMinus] decimal(5,2) NOT NULL,
    [IsPublic] bit DEFAULT 0,
    [Comments] nvarchar(max) NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([RegistrationRoundId]) REFERENCES [RegistrationRound]([Id]),
    FOREIGN KEY ([RefereeAccountId]) REFERENCES [Account]([Id]),
)
GO
-- ErrorTypes table
CREATE TABLE [dbo].[ErrorType] (
    [Id] UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    [CriteriaId]  UNIQUEIDENTIFIER NOT NULL,
    [Name] NVARCHAR(255) NOT NULL,
    [CreatedAt] [datetime] NOT NULL,
    [UpdatedAt] [datetime] NULL,
    FOREIGN KEY ([CriteriaId]) REFERENCES [Criteria]([Id])
)
GO
-- ScoreDetailErrors table
CREATE TABLE [dbo].[ScoreDetailError] (
   [Id] UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
   [ScoreDetailId]  UNIQUEIDENTIFIER NOT NULL,
   [ErrorTypeId]  UNIQUEIDENTIFIER NOT NULL,
   [Severity]  NVARCHAR(20) NOT NULL,
   [PointMinus] decimal(5,2) NOT NULL,
   FOREIGN KEY ([ScoreDetailId]) REFERENCES [ScoreDetail]([Id]),
   FOREIGN KEY ([ErrorTypeId]) REFERENCES [ErrorType]([Id])
)
GO
-- RoundResults table
CREATE TABLE [dbo].[RoundResult](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [RegistrationRoundsId] UNIQUEIDENTIFIER NOT NULL,
    [TotalScore] decimal(5,2) NOT NULL,
    [IsPublic] bit DEFAULT 0,
    [Comments] nvarchar(max) NULL,
    [Status] nvarchar(20) NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([RegistrationRoundsId]) REFERENCES [RegistrationRound]([Id]),
)
GO



-- TicketType table
CREATE TABLE [dbo].[TicketType](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [KoiShowId] UNIQUEIDENTIFIER NOT NULL,
    [Name] nvarchar(50) NOT NULL,
    [Price] decimal(10,2) NOT NULL,
    [AvailableQuantity] int NOT NULL,
    FOREIGN KEY ([KoiShowId]) REFERENCES [KoiShow]([Id])
)
GO


-- TicketOrders table
CREATE TABLE [dbo].[TicketOrder](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [FullName] nvarchar(100) NOT NULL,
    [Email] varchar(100) NOT NULL,
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [OrderDate] datetime NOT NULL DEFAULT GETDATE(),
    [TransactionCode] NVARCHAR(50) NOT NULL UNIQUE ,
    [TotalAmount] decimal(18,2) NOT NULL,
    [PaymentMethod] nvarchar(50) NULL,
    [Status] nvarchar(50) NULL,
    [Notes] nvarchar(max) NULL,
    FOREIGN KEY ([AccountId]) REFERENCES [Account]([Id]),
)
GO

-- TicketOrderDetails table
CREATE TABLE [dbo].[TicketOrderDetail](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TicketOrderId] UNIQUEIDENTIFIER NOT NULL,
    [TicketTypeId] UNIQUEIDENTIFIER NOT NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    FOREIGN KEY ([TicketOrderId]) REFERENCES [TicketOrder]([Id]),
    FOREIGN KEY ([TicketTypeId]) REFERENCES [TicketType]([Id])
)
GO

-- Tickets table
CREATE TABLE [dbo].[Ticket](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TicketOrderDetailId] UNIQUEIDENTIFIER NOT NULL,
    [QRCodeData] varchar(255) NULL,
    [ExpiredDate] datetime NOT NULL,
    [IsCheckedIn] BIT NOT NULL DEFAULT '0',
    [CheckInTime] datetime NULL,
    [CheckInLocation] nvarchar(100) NULL,
    [CheckedInBy] UNIQUEIDENTIFIER NULL,
    [Status] varchar(20) NULL,
    FOREIGN KEY ([TicketOrderDetailId]) REFERENCES [TicketOrderDetail]([Id]),
    FOREIGN KEY ([CheckedInBy]) REFERENCES [Account]([Id])
)
GO

-- Notifications table
CREATE TABLE [dbo].[Notification](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [AccountId] UNIQUEIDENTIFIER NULL,
    [Content] nvarchar(max) NOT NULL,
    [SentDate] datetime NOT NULL DEFAULT GETDATE(),
    Title nvarchar(255) NOT NULL,
    Type nvarchar(50) NOT NULL,
    [IsRead] BIT NOT NULL DEFAULT '0',
    FOREIGN KEY ([AccountId]) REFERENCES [Account]([Id])
)
GO

-- Feedbacks table
CREATE TABLE [dbo].[Feedback](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [KoiShowId] UNIQUEIDENTIFIER NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([AccountId]) REFERENCES [Account]([Id]),
    FOREIGN KEY ([KoiShowId]) REFERENCES [KoiShow]([Id])
)
GO

-- Sponsors table
CREATE TABLE [dbo].[Sponsor](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Name] nvarchar(255) NOT NULL,
    [LogoUrl] nvarchar(500) NULL,
    [InvestMoney] decimal(18,2) NOT NULL,
    [KoiShowId] UNIQUEIDENTIFIER NOT NULL,
    FOREIGN KEY ([KoiShowId]) REFERENCES [KoiShow]([Id])
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
    FOREIGN KEY ([KoiShowId]) REFERENCES [KoiShow]([Id])
)
GO

-- Livestreams table
CREATE TABLE [dbo].[Livestream](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [KoiShowId] UNIQUEIDENTIFIER NOT NULL,
    [StartTime] datetime NOT NULL,
    [EndTime] datetime NULL,
    [StreamUrl] nvarchar(500) NOT NULL,
    FOREIGN KEY ([KoiShowId]) REFERENCES [KoiShow]([Id])
)
GO

-- Votes table
CREATE TABLE [dbo].[Vote](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [RegistrationId] UNIQUEIDENTIFIER NOT NULL,
    [Prediction] nvarchar(max) NULL,
    [CreatedAt] datetime NOT NULL,
    [UpdatedAt] datetime NULL,
    FOREIGN KEY ([AccountId]) REFERENCES [Account]([Id]),
    FOREIGN KEY ([RegistrationId]) REFERENCES [Registration]([Id])
)
GO

-- BlogCategory table
CREATE TABLE [dbo].[BlogCategory](
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
	FOREIGN KEY ([AccountId]) REFERENCES [Account]([Id]),
	FOREIGN KEY ([BlogCategoryId]) REFERENCES [BlogCategory]([Id]),
)
GO
-- RegistrationPayments table
CREATE TABLE [dbo].[RegistrationPayment] (
      [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
      [RegistrationId] UNIQUEIDENTIFIER NOT NULL UNIQUE,
      [QRCodeData] varchar(255) NULL,
      [PaidAmount] DECIMAL(18,2) NOT NULL,
      [PaymentDate] DATETIME NOT NULL DEFAULT GETDATE(),
      [TransactionCode] NVARCHAR(50) NOT NULL UNIQUE ,
      [PaymentMethod] NVARCHAR(50) NOT NULL,
      [Status] VARCHAR(20) NOT NULL,
      FOREIGN KEY ([RegistrationId]) REFERENCES [Registration]([Id])
     
)
GO

-- CheckoutLogs table
CREATE TABLE [dbo].[CheckOutLog](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [RegistrationId] UNIQUEIDENTIFIER NULL UNIQUE,
    [ImgCheckOut] nvarchar(255) NOT NULL,
    [CheckOutTime] datetime DEFAULT GETDATE(),
    [CheckedOutBy] UNIQUEIDENTIFIER NULL,
    [Notes] nvarchar(255) NULL,
    FOREIGN KEY ([RegistrationId]) REFERENCES [Registration]([Id]),
    FOREIGN KEY ([CheckedOutBy]) REFERENCES [Account]([Id]),
)
GO
-- ShowRules table
CREATE TABLE [dbo].[ShowRule](
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [KoiShowId] UNIQUEIDENTIFIER NOT NULL,
    [Title] [nvarchar](200) NOT NULL,
    [Content] [nvarchar](max) NOT NULL,
    [CreatedAt] [datetime] NOT NULL,
    [UpdatedAt] [datetime] NULL,
	FOREIGN KEY ([KoiShowId]) REFERENCES [KoiShow]([Id])
)
GO
-- Media table
CREATE TABLE [dbo].[KoiMedia] (
   [Id] UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
   [MediaUrl] NVARCHAR(MAX) NOT NULL, -- URL của hình ảnh hoặc video
   [MediaType] NVARCHAR(10) NOT NULL, -- Loại phương tiện: 'Image' hoặc 'Video'
   [KoiProfileId] UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES KoiProfile(Id),
   [RegistrationId] UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES Registration(Id),
   CONSTRAINT CHK_MediaOf_ForeignKey CHECK (
       (KoiProfileId IS NOT NULL AND RegistrationId IS NULL)
           OR (KoiProfileId IS NULL AND RegistrationId IS NOT NULL)
       )
)
GO

