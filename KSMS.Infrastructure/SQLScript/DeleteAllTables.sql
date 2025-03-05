
-- Drop tables in reverse dependency order
PRINT 'Dropping all tables in proper dependency order...'

-- Child tables (no dependencies)
IF OBJECT_ID('dbo.KoiMedia', 'U') IS NOT NULL DROP TABLE dbo.KoiMedia;
IF OBJECT_ID('dbo.ShowRule', 'U') IS NOT NULL DROP TABLE dbo.ShowRule;
IF OBJECT_ID('dbo.CheckOutLog', 'U') IS NOT NULL DROP TABLE dbo.CheckOutLog;
IF OBJECT_ID('dbo.RegistrationPayment', 'U') IS NOT NULL DROP TABLE dbo.RegistrationPayment;
IF OBJECT_ID('dbo.ScoreDetailError', 'U') IS NOT NULL DROP TABLE dbo.ScoreDetailError;
IF OBJECT_ID('dbo.RoundResult', 'U') IS NOT NULL DROP TABLE dbo.RoundResult;
IF OBJECT_ID('dbo.BlogsNews', 'U') IS NOT NULL DROP TABLE dbo.BlogsNews;
IF OBJECT_ID('dbo.Vote', 'U') IS NOT NULL DROP TABLE dbo.Vote;
IF OBJECT_ID('dbo.Livestream', 'U') IS NOT NULL DROP TABLE dbo.Livestream;
IF OBJECT_ID('dbo.ShowStatus', 'U') IS NOT NULL DROP TABLE dbo.ShowStatus;
IF OBJECT_ID('dbo.Sponsor', 'U') IS NOT NULL DROP TABLE dbo.Sponsor;
IF OBJECT_ID('dbo.Feedback', 'U') IS NOT NULL DROP TABLE dbo.Feedback;
IF OBJECT_ID('dbo.Notification', 'U') IS NOT NULL DROP TABLE dbo.Notification;
IF OBJECT_ID('dbo.Ticket', 'U') IS NOT NULL DROP TABLE dbo.Ticket;
IF OBJECT_ID('dbo.ScoreDetail', 'U') IS NOT NULL DROP TABLE dbo.ScoreDetail;

-- Mid-level tables
IF OBJECT_ID('dbo.ErrorType', 'U') IS NOT NULL DROP TABLE dbo.ErrorType;
IF OBJECT_ID('dbo.TicketOrderDetail', 'U') IS NOT NULL DROP TABLE dbo.TicketOrderDetail;
IF OBJECT_ID('dbo.RegistrationRound', 'U') IS NOT NULL DROP TABLE dbo.RegistrationRound;
IF OBJECT_ID('dbo.CriteriaCompetitionCategory', 'U') IS NOT NULL DROP TABLE dbo.CriteriaCompetitionCategory;
IF OBJECT_ID('dbo.Award', 'U') IS NOT NULL DROP TABLE dbo.Award;
IF OBJECT_ID('dbo.RefereeAssignment', 'U') IS NOT NULL DROP TABLE dbo.RefereeAssignment;
IF OBJECT_ID('dbo.CategoryVariety', 'U') IS NOT NULL DROP TABLE dbo.CategoryVariety;

-- Upper-mid level tables
IF OBJECT_ID('dbo.Round', 'U') IS NOT NULL DROP TABLE dbo.Round;
IF OBJECT_ID('dbo.TicketOrder', 'U') IS NOT NULL DROP TABLE dbo.TicketOrder;
IF OBJECT_ID('dbo.TicketType', 'U') IS NOT NULL DROP TABLE dbo.TicketType;
IF OBJECT_ID('dbo.Registration', 'U') IS NOT NULL DROP TABLE dbo.Registration;
IF OBJECT_ID('dbo.Tank', 'U') IS NOT NULL DROP TABLE dbo.Tank;
IF OBJECT_ID('dbo.KoiProfile', 'U') IS NOT NULL DROP TABLE dbo.KoiProfile;

-- Upper level tables
IF OBJECT_ID('dbo.CompetitionCategory', 'U') IS NOT NULL DROP TABLE dbo.CompetitionCategory;
IF OBJECT_ID('dbo.ShowStaff', 'U') IS NOT NULL DROP TABLE dbo.ShowStaff;
IF OBJECT_ID('dbo.Criteria', 'U') IS NOT NULL DROP TABLE dbo.Criteria;

-- Base tables (referenced by many others)
IF OBJECT_ID('dbo.BlogCategory', 'U') IS NOT NULL DROP TABLE dbo.BlogCategory;
IF OBJECT_ID('dbo.KoiShow', 'U') IS NOT NULL DROP TABLE dbo.KoiShow;
IF OBJECT_ID('dbo.Variety', 'U') IS NOT NULL DROP TABLE dbo.Variety;
IF OBJECT_ID('dbo.Account', 'U') IS NOT NULL DROP TABLE dbo.Account;

PRINT 'All tables dropped successfully.'
