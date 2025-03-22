-- Tiếp tục cách thay thế sử dụng DELETE không tắt ràng buộc
BEGIN TRANSACTION

-- Xóa dữ liệu từ các bảng con (không có bảng khác tham chiếu đến)
DELETE FROM dbo.KoiMedia
DELETE FROM dbo.ShowRule
DELETE FROM dbo.CheckOutLog
DELETE FROM dbo.RegistrationPayment
DELETE FROM dbo.ScoreDetailError
DELETE FROM dbo.RoundResult
DELETE FROM dbo.BlogsNews
DELETE FROM dbo.Vote
DELETE FROM dbo.Livestream
DELETE FROM dbo.ShowStatus
DELETE FROM dbo.Sponsor
DELETE FROM dbo.Feedback
DELETE FROM dbo.Notification
DELETE FROM dbo.Ticket
DELETE FROM dbo.ScoreDetail

-- Xóa dữ liệu từ các bảng trung gian
DELETE FROM dbo.ErrorType
DELETE FROM dbo.TicketOrderDetail
DELETE FROM dbo.RegistrationRound
DELETE FROM dbo.CriteriaCompetitionCategory
DELETE FROM dbo.Award
DELETE FROM dbo.RefereeAssignment
DELETE FROM dbo.CategoryVariety

-- Xóa dữ liệu từ các bảng trung gian cao hơn
DELETE FROM dbo.Round
DELETE FROM dbo.TicketOrder
DELETE FROM dbo.TicketType
DELETE FROM dbo.Registration
DELETE FROM dbo.Tank
DELETE FROM dbo.KoiProfile

-- Xóa dữ liệu từ các bảng cấp cao
DELETE FROM dbo.CompetitionCategory
DELETE FROM dbo.ShowStaff
DELETE FROM dbo.Criteria

-- Xóa dữ liệu từ các bảng cơ sở
DELETE FROM dbo.BlogCategory
DELETE FROM dbo.KoiShow
DELETE FROM dbo.Variety
DELETE FROM dbo.Account

COMMIT TRANSACTION
PRINT 'Đã xóa toàn bộ dữ liệu từ tất cả các bảng thành công.'