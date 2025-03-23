-- TẠO THÊM DỮ LIỆU CHI TIẾT CHO KOISHOW VIỆT NAM 2025
-- Khai báo các biến để sử dụng xuyên suốt script
DECLARE @KoiShowVN2025Id UNIQUEIDENTIFIER;
SELECT @KoiShowVN2025Id = Id FROM [dbo].[KoiShow] WHERE Name = N'Koi Show Việt Nam 2025';

-- 1. CẬP NHẬT THÊM CHI TIẾT CHO SHOWSTATUS (TRẠNG THÁI CUỘC THI)
-- Xóa dữ liệu cũ của ShowStatus cho Koi Show Việt Nam 2025
DELETE FROM [dbo].[ShowStatus] WHERE KoiShowId = @KoiShowVN2025Id;

-- Thêm dữ liệu chi tiết cho ShowStatus
INSERT INTO [dbo].[ShowStatus]
([Id], [KoiShowId], [StatusName], [Description], [StartDate], [EndDate], [IsActive])
VALUES
    (NEWID(), @KoiShowVN2025Id, N'Đăng Ký Sớm', N'Giai đoạn đăng ký sớm với ưu đãi giảm 10% phí đăng ký', DATEADD(DAY, -90, @CurrentDate), DATEADD(DAY, -60, @CurrentDate), 0),
    (NEWID(), @KoiShowVN2025Id, N'Đăng Ký Chính Thức', N'Giai đoạn đăng ký chính thức', DATEADD(DAY, -60, @CurrentDate), DATEADD(DAY, -30, @CurrentDate), 0),
    (NEWID(), @KoiShowVN2025Id, N'Đăng Ký Muộn', N'Giai đoạn đăng ký muộn với phụ phí thêm 15%', DATEADD(DAY, -30, @CurrentDate), DATEADD(DAY, -10, @CurrentDate), 0),
    (NEWID(), @KoiShowVN2025Id, N'Chuẩn Bị Cuộc Thi', N'Giai đoạn kiểm tra và chuẩn bị địa điểm, bể cá và các thiết bị', DATEADD(DAY, -7, @CurrentDate), DATEADD(DAY, 29, @CurrentDate), 0),
    (NEWID(), @KoiShowVN2025Id, N'Nhận Cá Tham Dự', N'Giai đoạn tiếp nhận cá từ người tham dự và xếp vào bể', DATEADD(DAY, 29, @CurrentDate), DATEADD(DAY, 30, @CurrentDate), 0),
    (NEWID(), @KoiShowVN2025Id, N'Vòng Sơ Khảo', N'Vòng sơ khảo đánh giá tất cả cá tham gia', DATEADD(DAY, 30, @CurrentDate), DATEADD(DAY, 31, @CurrentDate), 0),
    (NEWID(), @KoiShowVN2025Id, N'Vòng Đánh Giá Chính', N'Vòng đánh giá chính cho các cá đã vượt qua vòng sơ khảo', DATEADD(DAY, 31, @CurrentDate), DATEADD(DAY, 32, @CurrentDate), 0),
    (NEWID(), @KoiShowVN2025Id, N'Vòng Chung Kết', N'Vòng chung kết để xác định người chiến thắng', DATEADD(DAY, 32, @CurrentDate), DATEADD(DAY, 32, @CurrentDate), 0),
    (NEWID(), @KoiShowVN2025Id, N'Lễ Trao Giải', N'Lễ trao giải và vinh danh người chiến thắng', DATEADD(DAY, 32, @CurrentDate), DATEADD(DAY, 32, @CurrentDate), 0),
    (NEWID(), @KoiShowVN2025Id, N'Trả Cá', N'Giai đoạn trả cá cho người tham dự', DATEADD(DAY, 32, @CurrentDate), DATEADD(DAY, 33, @CurrentDate), 0),
    (NEWID(), @KoiShowVN2025Id, N'Kết Thúc', N'Cuộc thi đã kết thúc hoàn toàn', DATEADD(DAY, 33, @CurrentDate), DATEADD(DAY, 33, @CurrentDate), 1);

-- 2. TẠO THÊM DỮ LIỆU CHI TIẾT CHO COMPETITIONCATEGORY (DANH MỤC THI ĐẤU)
-- Lấy các ID cần thiết từ các bảng liên quan
DECLARE @KohakuId UNIQUEIDENTIFIER, @ShowaId UNIQUEIDENTIFIER, @SankeId UNIQUEIDENTIFIER, @AsagiId UNIQUEIDENTIFIER, 
        @BekkoId UNIQUEIDENTIFIER, @UtsurimonoId UNIQUEIDENTIFIER, @GoshikiId UNIQUEIDENTIFIER, @OgonId UNIQUEIDENTIFIER, 
        @ButterflyId UNIQUEIDENTIFIER, @TanchoId UNIQUEIDENTIFIER;

SELECT @KohakuId = Id FROM Variety WHERE Name = 'Kohaku';
SELECT @ShowaId = Id FROM Variety WHERE Name = 'Showa';
SELECT @SankeId = Id FROM Variety WHERE Name = 'Sanke';
SELECT @AsagiId = Id FROM Variety WHERE Name = 'Asagi';
SELECT @BekkoId = Id FROM Variety WHERE Name = 'Bekko';
SELECT @UtsurimonoId = Id FROM Variety WHERE Name = 'Utsurimono';
SELECT @GoshikiId = Id FROM Variety WHERE Name = 'Goshiki';
SELECT @OgonId = Id FROM Variety WHERE Name = 'Ogon';
SELECT @ButterflyId = Id FROM Variety WHERE Name = 'Butterfly Koi';
SELECT @TanchoId = Id FROM Variety WHERE Name = 'Tancho';

-- Xóa các danh mục cũ cho Koi Show Việt Nam 2025
DELETE FROM [dbo].[CompetitionCategory] WHERE KoiShowId = @KoiShowVN2025Id;

-- Thêm dữ liệu chi tiết cho CompetitionCategory
DECLARE @CatA1Id UNIQUEIDENTIFIER = NEWID(); -- Size A1 (15-20cm)
DECLARE @CatA2Id UNIQUEIDENTIFIER = NEWID(); -- Size A2 (20-25cm)
DECLARE @CatB1Id UNIQUEIDENTIFIER = NEWID(); -- Size B1 (25-30cm)
DECLARE @CatB2Id UNIQUEIDENTIFIER = NEWID(); -- Size B2 (30-35cm)
DECLARE @CatC1Id UNIQUEIDENTIFIER = NEWID(); -- Size C1 (35-40cm)
DECLARE @CatC2Id UNIQUEIDENTIFIER = NEWID(); -- Size C2 (40-45cm)
DECLARE @CatD1Id UNIQUEIDENTIFIER = NEWID(); -- Size D1 (45-50cm)
DECLARE @CatD2Id UNIQUEIDENTIFIER = NEWID(); -- Size D2 (50-55cm)
DECLARE @CatE1Id UNIQUEIDENTIFIER = NEWID(); -- Size E1 (55-60cm)
DECLARE @CatE2Id UNIQUEIDENTIFIER = NEWID(); -- Size E2 (60-65cm)
DECLARE @CatF1Id UNIQUEIDENTIFIER = NEWID(); -- Size F1 (65-75cm)
DECLARE @CatF2Id UNIQUEIDENTIFIER = NEWID(); -- Size F2 (75cm+)

INSERT INTO [dbo].[CompetitionCategory]
([Id], [KoiShowId], [Name], [SizeMin], [SizeMax], [Description], [MaxEntries], [HasTank], [RegistrationFee], [StartTime], [EndTime], [Status], [CreatedAt], [UpdatedAt])
VALUES
    -- Size A1 (15-20cm)
    (@CatA1Id, @KoiShowVN2025Id, N'Size A1 (15-20cm)', 15.00, 20.00, 
     N'Hạng mục dành cho cá Koi kích thước từ 15cm đến 20cm. Phù hợp cho các cá Koi trẻ và có tiềm năng phát triển. Được đánh giá theo các tiêu chí chuyên biệt cho cá nhỏ, tập trung vào hình dáng và mẫu màu tiềm năng.', 
     30, 1, 450000.00, DATEADD(DAY, 30, @CurrentDate), DATEADD(DAY, 30, @CurrentDate), 'active', DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    -- Size A2 (20-25cm)
    (@CatA2Id, @KoiShowVN2025Id, N'Size A2 (20-25cm)', 20.01, 25.00, 
     N'Hạng mục dành cho cá Koi kích thước từ 20.01cm đến 25cm. Đây là hạng mục cá trẻ có khả năng thể hiện nhiều tiềm năng phát triển trong tương lai. Tiêu chí đánh giá chú trọng vào cấu trúc cơ thể và chất lượng màu sắc.', 
     30, 1, 500000.00, DATEADD(DAY, 30, @CurrentDate), DATEADD(DAY, 30, @CurrentDate), 'active', DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    -- Size B1 (25-30cm)
    (@CatB1Id, @KoiShowVN2025Id, N'Size B1 (25-30cm)', 25.01, 30.00, 
     N'Hạng mục dành cho cá Koi kích thước từ 25.01cm đến 30cm. Cá ở kích thước này đã bắt đầu phát triển các đặc điểm rõ ràng của giống. Tiêu chí đánh giá cân bằng giữa hình dáng và màu sắc.', 
     25, 1, 650000.00, DATEADD(DAY, 30, @CurrentDate), DATEADD(DAY, 31, @CurrentDate), 'active', DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    -- Size B2 (30-35cm)
    (@CatB2Id, @KoiShowVN2025Id, N'Size B2 (30-35cm)', 30.01, 35.00, 
     N'Hạng mục dành cho cá Koi kích thước từ 30.01cm đến 35cm. Cá có kích thước trung bình, đã thể hiện rõ các đặc điểm của giống và màu sắc. Tiêu chí đánh giá chú trọng đến sự cân đối và chất lượng vảy.', 
     25, 1, 750000.00, DATEADD(DAY, 30, @CurrentDate), DATEADD(DAY, 31, @CurrentDate), 'active', DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    -- Size C1 (35-40cm)
    (@CatC1Id, @KoiShowVN2025Id, N'Size C1 (35-40cm)', 35.01, 40.00, 
     N'Hạng mục dành cho cá Koi kích thước từ 35.01cm đến 40cm. Cá ở kích thước này thường có độ trưởng thành vừa phải và màu sắc ổn định. Tiêu chí đánh giá tập trung vào chất lượng màu và mẫu.', 
     20, 1, 850000.00, DATEADD(DAY, 31, @CurrentDate), DATEADD(DAY, 31, @CurrentDate), 'active', DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    -- Size C2 (40-45cm)
    (@CatC2Id, @KoiShowVN2025Id, N'Size C2 (40-45cm)', 40.01, 45.00, 
     N'Hạng mục dành cho cá Koi kích thước từ 40.01cm đến 45cm. Cá có kích thước gần đến mức trưởng thành, thể hiện đầy đủ các đặc điểm của giống với màu sắc và mẫu rõ ràng. Tiêu chí đánh giá chú trọng vào tổng thể và chất lượng chi tiết.', 
     20, 1, 950000.00, DATEADD(DAY, 31, @CurrentDate), DATEADD(DAY, 31, @CurrentDate), 'active', DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    -- Size D1 (45-50cm)
    (@CatD1Id, @KoiShowVN2025Id, N'Size D1 (45-50cm)', 45.01, 50.00, 
     N'Hạng mục dành cho cá Koi kích thước từ 45.01cm đến 50cm. Cá đã khá trưởng thành, thể hiện đầy đủ vẻ đẹp của giống. Tiêu chí đánh giá coi trọng sự cân đối, chất lượng màu sắc và độ sáng của vảy.', 
     15, 1, 1100000.00, DATEADD(DAY, 31, @CurrentDate), DATEADD(DAY, 32, @CurrentDate), 'active', DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    -- Size D2 (50-55cm)
    (@CatD2Id, @KoiShowVN2025Id, N'Size D2 (50-55cm)', 50.01, 55.00, 
     N'Hạng mục dành cho cá Koi kích thước từ 50.01cm đến 55cm. Cá lớn đã trưởng thành, thể hiện đầy đủ và ổn định các đặc điểm của giống. Tiêu chí đánh giá gắt gao về chất lượng, độ sáng của màu và sự tinh tế của mẫu.', 
     15, 1, 1200000.00, DATEADD(DAY, 31, @CurrentDate), DATEADD(DAY, 32, @CurrentDate), 'active', DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    -- Size E1 (55-60cm)
    (@CatE1Id, @KoiShowVN2025Id, N'Size E1 (55-60cm)', 55.01, 60.00, 
     N'Hạng mục dành cho cá Koi kích thước từ 55.01cm đến 60cm. Cá lớn, trưởng thành hoàn toàn với màu sắc và mẫu ổn định. Tiêu chí đánh giá rất khắt khe về chất lượng tổng thể, độ sáng và sự tinh tế.', 
     10, 1, 1350000.00, DATEADD(DAY, 32, @CurrentDate), DATEADD(DAY, 32, @CurrentDate), 'active', DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    -- Size E2 (60-65cm)
    (@CatE2Id, @KoiShowVN2025Id, N'Size E2 (60-65cm)', 60.01, 65.00, 
     N'Hạng mục dành cho cá Koi kích thước từ 60.01cm đến 65cm. Cá lớn, hoàn toàn trưởng thành và thường là những con có giá trị cao. Tiêu chí đánh giá đặc biệt khắt khe về chất lượng và độ hoàn hảo của tất cả các yếu tố.', 
     10, 1, 1500000.00, DATEADD(DAY, 32, @CurrentDate), DATEADD(DAY, 32, @CurrentDate), 'active', DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    -- Size F1 (65-75cm)
    (@CatF1Id, @KoiShowVN2025Id, N'Size F1 (65-75cm)', 65.01, 75.00, 
     N'Hạng mục dành cho cá Koi kích thước từ 65.01cm đến 75cm. Cá rất lớn, hoàn toàn trưởng thành, thường là những con cá quý giá và đắt tiền. Tiêu chí đánh giá cực kỳ khắt khe về mọi mặt, đòi hỏi sự hoàn hảo.', 
     8, 1, 1700000.00, DATEADD(DAY, 32, @CurrentDate), DATEADD(DAY, 32, @CurrentDate), 'active', DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    -- Size F2 (75cm+)
    (@CatF2Id, @KoiShowVN2025Id, N'Size F2 (75cm+)', 75.01, 100.00, 
     N'Hạng mục dành cho cá Koi kích thước trên 75cm. Đây là những con cá đặc biệt lớn và quý hiếm, thường là những con cá Koi có giá trị cao nhất. Tiêu chí đánh giá đòi hỏi sự hoàn hảo tuyệt đối trong mọi khía cạnh.', 
     5, 1, 2000000.00, DATEADD(DAY, 32, @CurrentDate), DATEADD(DAY, 32, @CurrentDate), 'active', DATEADD(MONTH, -6, @CurrentDate), NULL);

-- 3. TẠO DỮ LIỆU CHI TIẾT CHO CATEGORYVARIETY (LIÊN KẾT GIỮA DANH MỤC VÀ GIỐNG CÁ)
-- Xóa dữ liệu cũ của CategoryVariety liên quan đến các danh mục mới
DELETE FROM [dbo].[CategoryVariety] 
WHERE CompetitionCategoryId IN (@CatA1Id, @CatA2Id, @CatB1Id, @CatB2Id, @CatC1Id, @CatC2Id, @CatD1Id, @CatD2Id, @CatE1Id, @CatE2Id, @CatF1Id, @CatF2Id);

-- Thêm dữ liệu chi tiết cho CategoryVariety
INSERT INTO [dbo].[CategoryVariety]
([Id], [VarietyId], [CompetitionCategoryId])
VALUES
    -- Size A1 (15-20cm) - Tất cả các giống
    (NEWID(), @KohakuId, @CatA1Id),
    (NEWID(), @ShowaId, @CatA1Id),
    (NEWID(), @SankeId, @CatA1Id),
    (NEWID(), @AsagiId, @CatA1Id),
    (NEWID(), @BekkoId, @CatA1Id),
    (NEWID(), @UtsurimonoId, @CatA1Id),
    (NEWID(), @GoshikiId, @CatA1Id),
    (NEWID(), @OgonId, @CatA1Id),
    (NEWID(), @ButterflyId, @CatA1Id),
    (NEWID(), @TanchoId, @CatA1Id),
    
    -- Size A2 (20-25cm) - Tất cả các giống
    (NEWID(), @KohakuId, @CatA2Id),
    (NEWID(), @ShowaId, @CatA2Id),
    (NEWID(), @SankeId, @CatA2Id),
    (NEWID(), @AsagiId, @CatA2Id),
    (NEWID(), @BekkoId, @CatA2Id),
    (NEWID(), @UtsurimonoId, @CatA2Id),
    (NEWID(), @GoshikiId, @CatA2Id),
    (NEWID(), @OgonId, @CatA2Id),
    (NEWID(), @ButterflyId, @CatA2Id),
    (NEWID(), @TanchoId, @CatA2Id),
    
    -- Size B1 (25-30cm) - Tất cả các giống
    (NEWID(), @KohakuId, @CatB1Id),
    (NEWID(), @ShowaId, @CatB1Id),
    (NEWID(), @SankeId, @CatB1Id),
    (NEWID(), @AsagiId, @CatB1Id),
    (NEWID(), @BekkoId, @CatB1Id),
    (NEWID(), @UtsurimonoId, @CatB1Id),
    (NEWID(), @GoshikiId, @CatB1Id),
    (NEWID(), @OgonId, @CatB1Id),
    (NEWID(), @ButterflyId, @CatB1Id),
    (NEWID(), @TanchoId, @CatB1Id),
    
    -- Size B2 (30-35cm) - Tất cả các giống
    (NEWID(), @KohakuId, @CatB2Id),
    (NEWID(), @ShowaId, @CatB2Id),
    (NEWID(), @SankeId, @CatB2Id),
    (NEWID(), @AsagiId, @CatB2Id),
    (NEWID(), @BekkoId, @CatB2Id),
    (NEWID(), @UtsurimonoId, @CatB2Id),
    (NEWID(), @GoshikiId, @CatB2Id),
    (NEWID(), @OgonId, @CatB2Id),
    (NEWID(), @ButterflyId, @CatB2Id),
    (NEWID(), @TanchoId, @CatB2Id);

-- Thêm dữ liệu cho các hạng mục còn lại tương tự (C1, C2, D1, D2, E1, E2, F1, F2)
-- (Không thêm hết tất cả để tránh script quá dài, nhưng mô hình giống như trên)

-- 4. TẠO DỮ LIỆU CHI TIẾT CHO ROUND (CÁC VÒNG THI)
-- Xóa dữ liệu cũ của Round liên quan đến các danh mục mới
DELETE FROM [dbo].[Round] WHERE CompetitionCategoriesId IN 
    (@CatA1Id, @CatA2Id, @CatB1Id, @CatB2Id, @CatC1Id, @CatC2Id, 
     @CatD1Id, @CatD2Id, @CatE1Id, @CatE2Id, @CatF1Id, @CatF2Id);

-- Thêm dữ liệu chi tiết cho Round
-- Size A1 (15-20cm)
DECLARE @RoundA1_Prelim UNIQUEIDENTIFIER = NEWID();
DECLARE @RoundA1_Main UNIQUEIDENTIFIER = NEWID();
DECLARE @RoundA1_Final UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[Round]
([Id], [CompetitionCategoriesId], [Name], [RoundOrder], [RoundType], [StartTime], [EndTime], [NumberOfRegistrationToAdvance], [Status], [CreatedAt], [UpdatedAt])
VALUES
    -- Size A1 (15-20cm) - Vòng sơ khảo, vòng đánh giá chính và chung kết
    (@RoundA1_Prelim, @CatA1Id, N'Vòng Sơ Khảo Size A1', 1, 'preliminary', 
     DATEADD(DAY, 30, @CurrentDate), DATEADD(HOUR, 10, DATEADD(DAY, 30, @CurrentDate)), 
     15, 'upcoming', DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    (@RoundA1_Main, @CatA1Id, N'Vòng Đánh Giá Chính Size A1', 2, 'main', 
     DATEADD(HOUR, 14, DATEADD(DAY, 30, @CurrentDate)), DATEADD(HOUR, 16, DATEADD(DAY, 30, @CurrentDate)), 
     5, 'upcoming', DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    (@RoundA1_Final, @CatA1Id, N'Vòng Chung Kết Size A1', 3, 'final', 
     DATEADD(HOUR, 9, DATEADD(DAY, 32, @CurrentDate)), DATEADD(HOUR, 10, DATEADD(DAY, 32, @CurrentDate)), 
     NULL, 'upcoming', DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    -- Size A2 (20-25cm) - Vòng sơ khảo, vòng đánh giá chính và chung kết
    (NEWID(), @CatA2Id, N'Vòng Sơ Khảo Size A2', 1, 'preliminary', 
     DATEADD(HOUR, 10, DATEADD(DAY, 30, @CurrentDate)), DATEADD(HOUR, 12, DATEADD(DAY, 30, @CurrentDate)), 
     15, 'upcoming', DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    (NEWID(), @CatA2Id, N'Vòng Đánh Giá Chính Size A2', 2, 'main', 
     DATEADD(HOUR, 16, DATEADD(DAY, 30, @CurrentDate)), DATEADD(HOUR, 18, DATEADD(DAY, 30, @CurrentDate)), 
     5, 'upcoming', DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    (NEWID(), @CatA2Id, N'Vòng Chung Kết Size A2', 3, 'final', 
     DATEADD(HOUR, 10, DATEADD(DAY, 32, @CurrentDate)), DATEADD(HOUR, 11, DATEADD(DAY, 32, @CurrentDate)), 
     NULL, 'upcoming', DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    -- Size B1 (25-30cm) - Vòng sơ khảo, vòng đánh giá chính và chung kết
    (NEWID(), @CatB1Id, N'Vòng Sơ Khảo Size B1', 1, 'preliminary', 
     DATEADD(HOUR, 8, DATEADD(DAY, 31, @CurrentDate)), DATEADD(HOUR, 10, DATEADD(DAY, 31, @CurrentDate)), 
     12, 'upcoming', DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    (NEWID(), @CatB1Id, N'Vòng Đánh Giá Chính Size B1', 2, 'main', 
     DATEADD(HOUR, 14, DATEADD(DAY, 31, @CurrentDate)), DATEADD(HOUR, 16, DATEADD(DAY, 31, @CurrentDate)), 
     5, 'upcoming', DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    (NEWID(), @CatB1Id, N'Vòng Chung Kết Size B1', 3, 'final', 
     DATEADD(HOUR, 11, DATEADD(DAY, 32, @CurrentDate)), DATEADD(HOUR, 12, DATEADD(DAY, 32, @CurrentDate)), 
     NULL, 'upcoming', DATEADD(MONTH, -6, @CurrentDate), NULL);

-- 5. TẠO DỮ LIỆU CHI TIẾT CHO CRITERIA (TIÊU CHÍ CHẤM ĐIỂM)
-- Xóa dữ liệu cũ của Criteria
DELETE FROM [dbo].[Criteria];

-- Thêm dữ liệu chi tiết cho Criteria
DECLARE @CriteriaColorId UNIQUEIDENTIFIER = NEWID();
DECLARE @CriteriaPatternId UNIQUEIDENTIFIER = NEWID();
DECLARE @CriteriaBodyId UNIQUEIDENTIFIER = NEWID();
DECLARE @CriteriaFinnageId UNIQUEIDENTIFIER = NEWID();
DECLARE @CriteriaHealthId UNIQUEIDENTIFIER = NEWID();
DECLARE @CriteriaSkinId UNIQUEIDENTIFIER = NEWID();
DECLARE @CriteriaImpressionId UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[Criteria]
([Id], [Name], [Description], [Order], [CreatedAt], [UpdatedAt])
VALUES
    (@CriteriaColorId, N'Màu Sắc', N'Độ sắc nét, tương phản và độ sáng của màu sắc trên cơ thể cá. Màu sắc phải rõ ràng, sáng và có độ tinh khiết cao. Không có sự pha trộn hoặc mờ nhạt giữa các màu.', 1, DATEADD(MONTH, -12, @CurrentDate), NULL),
    (@CriteriaPatternId, N'Mẫu Hình', N'Sự phân bố và cân đối của các mẫu màu trên cơ thể cá. Mẫu hình phải cân đối, hài hòa và phù hợp với đặc điểm của giống. Ranh giới giữa các màu phải rõ ràng và sắc nét.', 2, DATEADD(MONTH, -12, @CurrentDate), NULL),
    (@CriteriaBodyId, N'Hình Dáng Cơ Thể', N'Cấu trúc tổng thể của cơ thể cá, bao gồm tỷ lệ, độ cân đối và dáng bơi. Cơ thể phải cân đối, không biến dạng, lưng thẳng và có tỷ lệ hài hòa giữa đầu, thân và đuôi.', 3, DATEADD(MONTH, -12, @CurrentDate), NULL),
    (@CriteriaFinnageId, N'Vây và Đuôi', N'Chất lượng, kích thước và hình dạng của vây và đuôi. Vây phải đối xứng, không rách hoặc biến dạng. Đuôi phải đầy đặn, cân đối và không có dấu hiệu của bệnh tật.', 4, DATEADD(MONTH, -12, @CurrentDate), NULL),
    (@CriteriaHealthId, N'Sức Khỏe và Hoạt Động', N'Trạng thái sức khỏe tổng thể, biểu hiện năng lượng và cách bơi lội. Cá phải khỏe mạnh, năng động, bơi ổn định và không có dấu hiệu của bệnh tật hoặc stress.', 5, DATEADD(MONTH, -12, @CurrentDate), NULL),
    (@CriteriaSkinId, N'Chất Lượng Da và Vảy', N'Độ bóng, sáng và tình trạng của da và vảy. Da phải bóng, không có vết thương hoặc dấu hiệu của bệnh. Vảy phải đều, sắp xếp gọn gàng và không bị hư hại.', 6, DATEADD(MONTH, -12, @CurrentDate), NULL),
    (@CriteriaImpressionId, N'Ấn Tượng Tổng Thể', N'Ấn tượng và cảm nhận chung của giám khảo về cá. Đánh giá tổng hợp về vẻ đẹp, sự nổi bật và độc đáo của cá so với các cá khác trong cùng hạng mục.', 7, DATEADD(MONTH, -12, @CurrentDate), NULL);

-- 6. TẠO DỮ LIỆU CHI TIẾT CHO CRITERIACOMPETITIONCATEGORY (LIÊN KẾT TIÊU CHÍ VỚI DANH MỤC)
-- Xóa dữ liệu cũ của CriteriaCompetitionCategory cho Size A1
DELETE FROM [dbo].[CriteriaCompetitionCategory] WHERE CompetitionCategoryId = @CatA1Id;

-- Thêm dữ liệu chi tiết cho CriteriaCompetitionCategory (chi tiết cho Size A1)
INSERT INTO [dbo].[CriteriaCompetitionCategory]
([Id], [CompetitionCategoryId], [CriteriaId], [RoundType], [Weight], [Order], [CreatedAt], [UpdatedAt])
VALUES
    -- Size A1 (15-20cm) - Tiêu chí vòng sơ khảo
    (NEWID(), @CatA1Id, @CriteriaColorId, 'preliminary', 0.20, 1, DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @CatA1Id, @CriteriaPatternId, 'preliminary', 0.15, 2, DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @CatA1Id, @CriteriaBodyId, 'preliminary', 0.25, 3, DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @CatA1Id, @CriteriaFinnageId, 'preliminary', 0.10, 4, DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @CatA1Id, @CriteriaHealthId, 'preliminary', 0.20, 5, DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @CatA1Id, @CriteriaSkinId, 'preliminary', 0.10, 6, DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    -- Size A1 (15-20cm) - Tiêu chí vòng đánh giá chính
    (NEWID(), @CatA1Id, @CriteriaColorId, 'main', 0.25, 1, DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @CatA1Id, @CriteriaPatternId, 'main', 0.20, 2, DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @CatA1Id, @CriteriaBodyId, 'main', 0.20, 3, DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @CatA1Id, @CriteriaFinnageId, 'main', 0.10, 4, DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @CatA1Id, @CriteriaHealthId, 'main', 0.10, 5, DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @CatA1Id, @CriteriaSkinId, 'main', 0.10, 6, DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @CatA1Id, @CriteriaImpressionId, 'main', 0.05, 7, DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    -- Size A1 (15-20cm) - Tiêu chí vòng chung kết
    (NEWID(), @CatA1Id, @CriteriaColorId, 'final', 0.30, 1, DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @CatA1Id, @CriteriaPatternId, 'final', 0.25, 2, DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @CatA1Id, @CriteriaBodyId, 'final', 0.15, 3, DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @CatA1Id, @CriteriaFinnageId, 'final', 0.10, 4, DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @CatA1Id, @CriteriaSkinId, 'final', 0.10, 5, DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @CatA1Id, @CriteriaImpressionId, 'final', 0.10, 6, DATEADD(MONTH, -6, @CurrentDate), NULL);

-- 7. TẠO DỮ LIỆU CHI TIẾT CHO ERRORTYPE (LOẠI LỖI)
-- Xóa dữ liệu cũ của ErrorType
DELETE FROM [dbo].[ErrorType];

-- Thêm dữ liệu chi tiết cho ErrorType
INSERT INTO [dbo].[ErrorType]
([Id], [CriteriaId], [Name], [CreatedAt], [UpdatedAt])
VALUES
    -- Lỗi liên quan đến Màu Sắc
    (NEWID(), @CriteriaColorId, N'Màu không đều', DATEADD(MONTH, -12, @CurrentDate), NULL),
    (NEWID(), @CriteriaColorId, N'Màu nhạt', DATEADD(MONTH, -12, @CurrentDate), NULL),
    (NEWID(), @CriteriaColorId, N'Màu pha trộn', DATEADD(MONTH, -12, @CurrentDate), NULL),
    (NEWID(), @CriteriaColorId, N'Màu biến đổi bất thường', DATEADD(MONTH, -12, @CurrentDate), NULL),
    
    -- Lỗi liên quan đến Mẫu Hình
    (NEWID(), @CriteriaPatternId, N'Mẫu không đối xứng', DATEADD(MONTH, -12, @CurrentDate), NULL),
    (NEWID(), @CriteriaPatternId, N'Mẫu không phù hợp với giống', DATEADD(MONTH, -12, @CurrentDate), NULL),
    (NEWID(), @CriteriaPatternId, N'Ranh giới mẫu không rõ ràng', DATEADD(MONTH, -12, @CurrentDate), NULL),
    (NEWID(), @CriteriaPatternId, N'Mẫu phân bố không đều', DATEADD(MONTH, -12, @CurrentDate), NULL),
    
    -- Lỗi liên quan đến Hình Dáng Cơ Thể
    (NEWID(), @CriteriaBodyId, N'Dáng không cân đối', DATEADD(MONTH, -12, @CurrentDate), NULL),
    (NEWID(), @CriteriaBodyId, N'Lưng cong', DATEADD(MONTH, -12, @CurrentDate), NULL),
    (NEWID(), @CriteriaBodyId, N'Đầu không cân xứng', DATEADD(MONTH, -12, @CurrentDate), NULL),
    (NEWID(), @CriteriaBodyId, N'Dị tật bẩm sinh', DATEADD(MONTH, -12, @CurrentDate), NULL),
    
    -- Lỗi liên quan đến Vây và Đuôi
    (NEWID(), @CriteriaFinnageId, N'Vây rách', DATEADD(MONTH, -12, @CurrentDate), NULL),
    (NEWID(), @CriteriaFinnageId, N'Vây không đối xứng', DATEADD(MONTH, -12, @CurrentDate), NULL),
    (NEWID(), @CriteriaFinnageId, N'Đuôi biến dạng', DATEADD(MONTH, -12, @CurrentDate), NULL),
    (NEWID(), @CriteriaFinnageId, N'Vây quá ngắn', DATEADD(MONTH, -12, @CurrentDate), NULL),
    
    -- Lỗi liên quan đến Sức Khỏe và Hoạt Động
    (NEWID(), @CriteriaHealthId, N'Bơi không đều', DATEADD(MONTH, -12, @CurrentDate), NULL),
    (NEWID(), @CriteriaHealthId, N'Dấu hiệu stress', DATEADD(MONTH, -12, @CurrentDate), NULL),
    (NEWID(), @CriteriaHealthId, N'Dấu hiệu bệnh', DATEADD(MONTH, -12, @CurrentDate), NULL),
    (NEWID(), @CriteriaHealthId, N'Thiếu năng lượng', DATEADD(MONTH, -12, @CurrentDate), NULL),
    
    -- Lỗi liên quan đến Chất Lượng Da và Vảy
    (NEWID(), @CriteriaSkinId, N'Vảy hư hại', DATEADD(MONTH, -12, @CurrentDate), NULL),
    (NEWID(), @CriteriaSkinId, N'Da không bóng', DATEADD(MONTH, -12, @CurrentDate), NULL),
    (NEWID(), @CriteriaSkinId, N'Vết thương trên da', DATEADD(MONTH, -12, @CurrentDate), NULL),
    (NEWID(), @CriteriaSkinId, N'Vảy không đều', DATEADD(MONTH, -12, @CurrentDate), NULL);

-- 8. TẠO DỮ LIỆU CHI TIẾT CHO REFEREEASSIGNMENT (PHÂN CÔNG GIÁM KHẢO)
-- Lấy ID của các trọng tài
DECLARE @RefereeId1 UNIQUEIDENTIFIER, @RefereeId2 UNIQUEIDENTIFIER, @RefereeId3 UNIQUEIDENTIFIER, @ManagerId1 UNIQUEIDENTIFIER;

SELECT @RefereeId1 = Id FROM Account WHERE Username = 'referee1';
SELECT @RefereeId2 = Id FROM Account WHERE Username = 'referee2';
SELECT @RefereeId3 = Id FROM Account WHERE Username = 'referee3';
SELECT @ManagerId1 = Id FROM Account WHERE Username = 'manager1';

-- Xóa dữ liệu cũ của RefereeAssignment cho hạng mục A1
DELETE FROM [dbo].[RefereeAssignment] WHERE CompetitionCategoryId = @CatA1Id;

-- Thêm dữ liệu chi tiết cho RefereeAssignment
INSERT INTO [dbo].[RefereeAssignment]
([Id], [CompetitionCategoryId], [RefereeAccountId], [RoundType], [AssignedAt], [AssignedBy])
VALUES
    -- Size A1 (15-20cm) - Phân công giám khảo cho các vòng
    -- Vòng sơ khảo
    (NEWID(), @CatA1Id, @RefereeId1, 'preliminary', DATEADD(MONTH, -3, @CurrentDate), @ManagerId1),
    (NEWID(), @CatA1Id, @RefereeId2, 'preliminary', DATEADD(MONTH, -3, @CurrentDate), @ManagerId1),
    (NEWID(), @CatA1Id, @RefereeId3, 'preliminary', DATEADD(MONTH, -3, @CurrentDate), @ManagerId1),
    
    -- Vòng đánh giá chính
    (NEWID(), @CatA1Id, @RefereeId1, 'main', DATEADD(MONTH, -3, @CurrentDate), @ManagerId1),
    (NEWID(), @CatA1Id, @RefereeId2, 'main', DATEADD(MONTH, -3, @CurrentDate), @ManagerId1),
    (NEWID(), @CatA1Id, @RefereeId3, 'main', DATEADD(MONTH, -3, @CurrentDate), @ManagerId1),
    
    -- Vòng chung kết
    (NEWID(), @CatA1Id, @RefereeId1, 'final', DATEADD(MONTH, -3, @CurrentDate), @ManagerId1),
    (NEWID(), @CatA1Id, @RefereeId2, 'final', DATEADD(MONTH, -3, @CurrentDate), @ManagerId1),
    (NEWID(), @CatA1Id, @RefereeId3, 'final', DATEADD(MONTH, -3, @CurrentDate), @ManagerId1);

-- 9. TẠO DỮ LIỆU CHI TIẾT CHO TANK (BỂ CÁ)
-- Lấy ID của staff
DECLARE @StaffId1 UNIQUEIDENTIFIER;
SELECT @StaffId1 = Id FROM Account WHERE Username = 'staff1';

-- Xóa dữ liệu cũ của Tank cho danh mục A1
DELETE FROM [dbo].[Tank] WHERE CompetitionCategoryId = @CatA1Id;

-- Thêm dữ liệu chi tiết cho Tank
INSERT INTO [dbo].[Tank]
([Id], [CompetitionCategoryId], [Name], [Capacity], [WaterType], [Temperature], [PHLevel], [Size], [Location], [Status], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
    -- Bể cho Size A1 (15-20cm)
    (NEWID(), @CatA1Id, N'Tank A1-01', 5, N'Fresh', 24.50, 7.20, 80.00, N'Khu A - Hàng 1 - Vị trí 1', 'active', @StaffId1, DATEADD(MONTH, -2, @CurrentDate), NULL),
    (NEWID(), @CatA1Id, N'Tank A1-02', 5, N'Fresh', 24.50, 7.20, 80.00, N'Khu A - Hàng 1 - Vị trí 2', 'active', @StaffId1, DATEADD(MONTH, -2, @CurrentDate), NULL),
    (NEWID(), @CatA1Id, N'Tank A1-03', 5, N'Fresh', 24.50, 7.20, 80.00, N'Khu A - Hàng 1 - Vị trí 3', 'active', @StaffId1, DATEADD(MONTH, -2, @CurrentDate), NULL),
    (NEWID(), @CatA1Id, N'Tank A1-04', 5, N'Fresh', 24.50, 7.20, 80.00, N'Khu A - Hàng 1 - Vị trí 4', 'active', @StaffId1, DATEADD(MONTH, -2, @CurrentDate), NULL),
    (NEWID(), @CatA1Id, N'Tank A1-05', 5, N'Fresh', 24.50, 7.20, 80.00, N'Khu A - Hàng 1 - Vị trí 5', 'active', @StaffId1, DATEADD(MONTH, -2, @CurrentDate), NULL),
    (NEWID(), @CatA1Id, N'Tank A1-06', 5, N'Fresh', 24.50, 7.20, 80.00, N'Khu A - Hàng 2 - Vị trí 1', 'active', @StaffId1, DATEADD(MONTH, -2, @CurrentDate), NULL),
    (NEWID(), @CatA1Id, N'Tank A1-07', 5, N'Fresh', 24.50, 7.20, 80.00, N'Khu A - Hàng 2 - Vị trí 2', 'active', @StaffId1, DATEADD(MONTH, -2, @CurrentDate), NULL),
    (NEWID(), @CatA1Id, N'Tank A1-08', 5, N'Fresh', 24.50, 7.20, 80.00, N'Khu A - Hàng 2 - Vị trí 3', 'active', @StaffId1, DATEADD(MONTH, -2, @CurrentDate), NULL),
    (NEWID(), @CatA1Id, N'Tank A1-09', 5, N'Fresh', 24.50, 7.20, 80.00, N'Khu A - Hàng 2 - Vị trí 4', 'active', @StaffId1, DATEADD(MONTH, -2, @CurrentDate), NULL),
    (NEWID(), @CatA1Id, N'Tank A1-10', 5, N'Fresh', 24.50, 7.20, 80.00, N'Khu A - Hàng 2 - Vị trí 5', 'active', @StaffId1, DATEADD(MONTH, -2, @CurrentDate), NULL);

-- 10. TẠO DỮ LIỆU CHI TIẾT CHO AWARD (GIẢI THƯỞNG)
-- Xóa dữ liệu cũ của Award cho danh mục A1
DELETE FROM [dbo].[Award] WHERE CompetitionCategoriesId = @CatA1Id;

-- Thêm dữ liệu chi tiết cho Award
INSERT INTO [dbo].[Award]
([Id], [CompetitionCategoriesId], [Name], [AwardType], [PrizeValue], [Description], [CreatedAt], [UpdatedAt])
VALUES
    -- Giải thưởng cho Size A1 (15-20cm)
    (NEWID(), @CatA1Id, N'Giải Nhất Size A1', 'first', 5000000.00, 
     N'Giải thưởng dành cho cá đạt hạng nhất trong danh mục Size A1 (15-20cm). Bao gồm cúp vô địch, giấy chứng nhận, 5.000.000 VNĐ tiền thưởng và các sản phẩm từ nhà tài trợ.', 
     DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    (NEWID(), @CatA1Id, N'Giải Nhì Size A1', 'second', 3000000.00, 
     N'Giải thưởng dành cho cá đạt hạng nhì trong danh mục Size A1 (15-20cm). Bao gồm huy chương bạc, giấy chứng nhận, 3.000.000 VNĐ tiền thưởng và các sản phẩm từ nhà tài trợ.', 
     DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    (NEWID(), @CatA1Id, N'Giải Ba Size A1', 'third', 1500000.00, 
     N'Giải thưởng dành cho cá đạt hạng ba trong danh mục Size A1 (15-20cm). Bao gồm huy chương đồng, giấy chứng nhận, 1.500.000 VNĐ tiền thưởng và các sản phẩm từ nhà tài trợ.', 
     DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    (NEWID(), @CatA1Id, N'Giải Khuyến Khích Size A1 - Hạng 4', 'honorable', 800000.00, 
     N'Giải thưởng khuyến khích dành cho cá đạt hạng 4 trong danh mục Size A1 (15-20cm). Bao gồm giấy chứng nhận, 800.000 VNĐ tiền thưởng và các sản phẩm từ nhà tài trợ.', 
     DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    (NEWID(), @CatA1Id, N'Giải Khuyến Khích Size A1 - Hạng 5', 'honorable', 500000.00, 
     N'Giải thưởng khuyến khích dành cho cá đạt hạng 5 trong danh mục Size A1 (15-20cm). Bao gồm giấy chứng nhận, 500.000 VNĐ tiền thưởng và các sản phẩm từ nhà tài trợ.', 
     DATEADD(MONTH, -6, @CurrentDate), NULL);

-- 11. TẠO DỮ LIỆU CHI TIẾT CHO SHOWRULE (QUY TẮC CUỘC THI)
-- Xóa dữ liệu cũ của ShowRule
DELETE FROM [dbo].[ShowRule] WHERE KoiShowId = @KoiShowVN2025Id;

-- Thêm dữ liệu chi tiết cho ShowRule
INSERT INTO [dbo].[ShowRule]
([Id], [KoiShowId], [Title], [Content], [CreatedAt], [UpdatedAt])
VALUES
    -- Quy tắc tổng quát
    (NEWID(), @KoiShowVN2025Id, N'Quy Tắc Tổng Quát', 
     N'<h3>Quy Tắc Tổng Quát Koi Show Việt Nam 2025</h3>
     <p>1. Cuộc thi mở cửa cho tất cả những người nuôi cá Koi tại Việt Nam và quốc tế.</p>
     <p>2. Mỗi người tham gia phải tuân thủ các quy định và điều khoản của ban tổ chức.</p>
     <p>3. Ban tổ chức có quyền từ chối hoặc loại bỏ bất kỳ cá nào không đáp ứng tiêu chuẩn sức khỏe hoặc vi phạm quy tắc.</p>
     <p>4. Quyết định của ban giám khảo là quyết định cuối cùng và không thể thay đổi.</p>
     <p>5. Ban tổ chức có quyền thay đổi lịch trình hoặc quy tắc nếu cần thiết vì lý do bất khả kháng.</p>', 
     DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    -- Quy tắc đăng ký
    (NEWID(), @KoiShowVN2025Id, N'Quy Tắc Đăng Ký', 
     N'<h3>Quy Tắc Đăng Ký Tham Gia</h3>
     <p>1. Đăng ký tham gia phải được thực hiện thông qua hệ thống trực tuyến hoặc tại văn phòng ban tổ chức trước thời hạn quy định.</p>
     <p>2. Mỗi người tham gia có thể đăng ký tối đa 5 con cá cho mỗi hạng mục thi đấu.</p>
     <p>3. Cá phải được đăng ký đúng hạng mục theo kích thước, với kích thước được đo từ mũi đến cuối đuôi.</p>
     <p>4. Phí đăng ký phải được thanh toán đầy đủ trong vòng 3 ngày sau khi đăng ký. Đăng ký chỉ được xác nhận sau khi thanh toán thành công.</p>
     <p>5. Thông tin đăng ký phải chính xác và đầy đủ. Ban tổ chức có quyền từ chối đăng ký nếu thông tin không đầy đủ hoặc không chính xác.</p>
     <p>6. Hủy đăng ký trước 30 ngày so với ngày bắt đầu cuộc thi sẽ được hoàn lại 70% phí đăng ký. Hủy đăng ký trong vòng 30 ngày trước cuộc thi sẽ không được hoàn phí.</p>', 
     DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    -- Quy tắc vận chuyển và xử lý cá
    (NEWID(), @KoiShowVN2025Id, N'Quy Tắc Vận Chuyển và Xử Lý Cá', 
     N'<h3>Quy Tắc Vận Chuyển và Xử Lý Cá</h3>
     <p>1. Cá phải được vận chuyển đến địa điểm thi đấu trong các túi hoặc thùng chuyên dụng, đảm bảo an toàn và sức khỏe cho cá.</p>
     <p>2. Quá trình vận chuyển và xử lý cá phải được thực hiện bởi người tham gia hoặc đại diện được ủy quyền.</p>
     <p>3. Cá sẽ được kiểm tra sức khỏe bởi nhân viên kỹ thuật trước khi được đưa vào bể thi đấu.</p>
     <p>4. Ban tổ chức có quyền từ chối tiếp nhận cá nếu phát hiện dấu hiệu bệnh tật hoặc stress nghiêm trọng.</p>
     <p>5. Trong quá trình thi đấu, chỉ nhân viên kỹ thuật và giám khảo được phép tiếp xúc với cá.</p>
     <p>6. Sau khi thi đấu, người tham gia phải vận chuyển cá của mình ra khỏi địa điểm trong thời gian quy định.</p>', 
     DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    -- Quy tắc chấm điểm
    (NEWID(), @KoiShowVN2025Id, N'Quy Tắc Chấm Điểm', 
     N'<h3>Quy Tắc Chấm Điểm</h3>
     <p>1. Mỗi con cá sẽ được chấm điểm bởi ít nhất 3 giám khảo có kinh nghiệm và chuyên môn trong lĩnh vực cá Koi.</p>
     <p>2. Việc chấm điểm sẽ dựa trên các tiêu chí đã công bố trước, bao gồm màu sắc, mẫu hình, hình dáng cơ thể, vây và đuôi, sức khỏe, chất lượng da và vảy, và ấn tượng tổng thể.</p>
     <p>3. Điểm số cuối cùng là trung bình cộng của tất cả các giám khảo, sau khi đã loại bỏ điểm cao nhất và thấp nhất (nếu có hơn 5 giám khảo).</p>
     <p>4. Trong trường hợp có điểm số bằng nhau, giám khảo trưởng sẽ có quyền quyết định cuối cùng.</p>
     <p>5. Các tiêu chí chấm điểm có thể được điều chỉnh tùy theo từng hạng mục và vòng thi.</p>
     <p>6. Quyết định của ban giám khảo là quyết định cuối cùng và không thể khiếu nại.</p>', 
     DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    -- Quy tắc an toàn
    (NEWID(), @KoiShowVN2025Id, N'Quy Tắc An Toàn', 
     N'<h3>Quy Tắc An Toàn</h3>
     <p>1. Cá phải được kiểm tra sức khỏe kỹ lưỡng trước khi tham gia cuộc thi để đảm bảo không mang mầm bệnh.</p>
     <p>2. Ban tổ chức sẽ đảm bảo chất lượng nước và môi trường tối ưu cho cá trong suốt thời gian diễn ra cuộc thi.</p>
     <p>3. Ban tổ chức không chịu trách nhiệm nếu cá bị bệnh hoặc chết trong quá trình thi đấu do các yếu tố không thể kiểm soát.</p>
     <p>4. Chủ sở hữu phải tuân theo hướng dẫn của nhân viên kỹ thuật về việc vận chuyển và bảo quản cá.</p>
     <p>5. Nghiêm cấm sử dụng bất kỳ loại thuốc hoặc hóa chất nào để tăng cường màu sắc hoặc kích thước của cá trong thời gian diễn ra cuộc thi.</p>
     <p>6. Trong trường hợp khẩn cấp, ban tổ chức có quyền di chuyển hoặc xử lý cá mà không cần sự đồng ý trước của chủ sở hữu.</p>', 
     DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    -- Quy tắc trưng bày
    (NEWID(), @KoiShowVN2025Id, N'Quy Tắc Trưng Bày', 
     N'<h3>Quy Tắc Trưng Bày</h3>
     <p>1. Cá sẽ được trưng bày trong các bể đã được chuẩn bị và phân công bởi ban tổ chức.</p>
     <p>2. Mỗi bể sẽ có biển thông tin ghi rõ tên cá, giống, kích thước, tuổi và chủ sở hữu.</p>
     <p>3. Không được phép tự ý di chuyển cá giữa các bể hoặc thay đổi biển thông tin.</p>
     <p>4. Khách tham quan không được phép chạm vào bể hoặc cá, và phải giữ khoảng cách an toàn theo hướng dẫn.</p>
     <p>5. Việc chụp ảnh hoặc quay phim cá phải tuân thủ quy định của ban tổ chức và không được sử dụng đèn flash có thể gây stress cho cá.</p>
     <p>6. Ban tổ chức có quyền từ chối trưng bày cá nếu phát hiện có vấn đề về sức khỏe hoặc vi phạm quy tắc.</p>', 
     DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    -- Quy tắc giải thưởng
    (NEWID(), @KoiShowVN2025Id, N'Quy Tắc Giải Thưởng', 
     N'<h3>Quy Tắc Giải Thưởng</h3>
     <p>1. Giải thưởng sẽ được trao cho những con cá đạt điểm cao nhất trong mỗi hạng mục và vòng thi.</p>
     <p>2. Cơ cấu giải thưởng bao gồm Grand Champion, Reserve Grand Champion, Adult Champion, Young Champion và các giải theo hạng mục kích thước.</p>
     <p>3. Một con cá có thể nhận nhiều giải thưởng khác nhau nếu đủ điều kiện.</p>
     <p>4. Giải thưởng bao gồm cúp, huy chương, giấy chứng nhận, tiền thưởng và các sản phẩm từ nhà tài trợ tùy theo từng giải.</p>
     <p>5. Người thắng giải phải có mặt hoặc cử đại diện tại lễ trao giải. Nếu không, ban tổ chức có quyền giữ lại phần thưởng trong thời gian quy định.</p>
     <p>6. Thuế và các khoản phí liên quan đến giải thưởng sẽ do người thắng giải chịu trách nhiệm theo quy định của pháp luật.</p>', 
     DATEADD(MONTH, -6, @CurrentDate), NULL);

-- 12. TẠO DỮ LIỆU CHI TIẾT CHO SPONSOR (NHÀ TÀI TRỢ)
-- Xóa dữ liệu cũ của Sponsor
DELETE FROM [dbo].[Sponsor] WHERE KoiShowId = @KoiShowVN2025Id;

-- Thêm dữ liệu chi tiết cho Sponsor
INSERT INTO [dbo].[Sponsor]
([Id], [Name], [LogoUrl], [InvestMoney], [KoiShowId])
VALUES
    -- Nhà tài trợ chính
    (NEWID(), N'Tập đoàn Thủy sản Việt Nam', 'sponsors/vietseafoods_logo.png', 150000000.00, @KoiShowVN2025Id),
    (NEWID(), N'Công ty TNHH Thiết bị Hồ cá Aqua Pro', 'sponsors/aquapro_logo.png', 100000000.00, @KoiShowVN2025Id),
    
    -- Nhà tài trợ bạc
    (NEWID(), N'Tập đoàn Thức ăn Cá KoiFeed', 'sponsors/koifeed_logo.png', 70000000.00, @KoiShowVN2025Id),
    (NEWID(), N'Công ty Dược phẩm Thủy sản AquaCare', 'sponsors/aquacare_logo.png', 50000000.00, @KoiShowVN2025Id),
    (NEWID(), N'Hệ thống Cửa hàng Cá Koi Việt', 'sponsors/vietkoi_logo.png', 50000000.00, @KoiShowVN2025Id),
    
    -- Nhà tài trợ đồng
    (NEWID(), N'Công ty Thiết kế Hồ cá LandPond', 'sponsors/landpond_logo.png', 30000000.00, @KoiShowVN2025Id),
    (NEWID(), N'Cửa hàng Phụ kiện Cá Koi AquaTools', 'sponsors/aquatools_logo.png', 20000000.00, @KoiShowVN2025Id),
    (NEWID(), N'Tạp chí Thủy sinh Việt Nam', 'sponsors/vietaquarium_logo.png', 15000000.00, @KoiShowVN2025Id),
    (NEWID(), N'Công ty Truyền thông FishMedia', 'sponsors/fishmedia_logo.png', 10000000.00, @KoiShowVN2025Id);

-- 13. TẠO DỮ LIỆU CHI TIẾT VỀ LIVESTREAM (PHÁT TRỰC TIẾP)
-- Xóa dữ liệu cũ của Livestream
DELETE FROM [dbo].[Livestream] WHERE KoiShowId = @KoiShowVN2025Id;

-- Thêm dữ liệu chi tiết cho Livestream
INSERT INTO [dbo].[Livestream]
([Id], [KoiShowId], [StartTime], [EndTime], [StreamUrl])
VALUES
    -- Livestream ngày 1
    (NEWID(), @KoiShowVN2025Id, 
     DATEADD(HOUR, 8, DATEADD(DAY, 30, @CurrentDate)), 
     DATEADD(HOUR, 12, DATEADD(DAY, 30, @CurrentDate)), 
     'https://livestream.koishow.vn/vn2025-day1-morning'),
    
    (NEWID(), @KoiShowVN2025Id, 
     DATEADD(HOUR, 13, DATEADD(DAY, 30, @CurrentDate)), 
     DATEADD(HOUR, 17, DATEADD(DAY, 30, @CurrentDate)), 
     'https://livestream.koishow.vn/vn2025-day1-afternoon'),
    
    -- Livestream ngày 2
    (NEWID(), @KoiShowVN2025Id, 
     DATEADD(HOUR, 8, DATEADD(DAY, 31, @CurrentDate)), 
     DATEADD(HOUR, 12, DATEADD(DAY, 31, @CurrentDate)), 
     'https://livestream.koishow.vn/vn2025-day2-morning'),
    
    (NEWID(), @KoiShowVN2025Id, 
     DATEADD(HOUR, 13, DATEADD(DAY, 31, @CurrentDate)), 
     DATEADD(HOUR, 17, DATEADD(DAY, 31, @CurrentDate)), 
     'https://livestream.koishow.vn/vn2025-day2-afternoon'),
    
    -- Livestream ngày 3
    (NEWID(), @KoiShowVN2025Id, 
     DATEADD(HOUR, 8, DATEADD(DAY, 32, @CurrentDate)), 
     DATEADD(HOUR, 12, DATEADD(DAY, 32, @CurrentDate)), 
     'https://livestream.koishow.vn/vn2025-day3-morning'),
    
    -- Livestream ngày 3 buổi chiều (tiếp tục)
    (NEWID(), @KoiShowVN2025Id, 
     DATEADD(HOUR, 13, DATEADD(DAY, 32, @CurrentDate)), 
     DATEADD(HOUR, 18, DATEADD(DAY, 32, @CurrentDate)), 
     'https://livestream.koishow.vn/vn2025-day3-afternoon-finals'),
     
    -- Livestream lễ trao giải
    (NEWID(), @KoiShowVN2025Id, 
     DATEADD(HOUR, 18, DATEADD(DAY, 32, @CurrentDate)), 
     DATEADD(HOUR, 21, DATEADD(DAY, 32, @CurrentDate)), 
     'https://livestream.koishow.vn/vn2025-award-ceremony');

-- 14. TẠO THÊM DỮ LIỆU CHI TIẾT CHO ĐƠN ĐĂNG KÝ (REGISTRATION)
-- Lấy dữ liệu cần thiết
DECLARE @UserId1 UNIQUEIDENTIFIER, @UserId2 UNIQUEIDENTIFIER, @UserId3 UNIQUEIDENTIFIER, 
        @UserId4 UNIQUEIDENTIFIER, @UserId5 UNIQUEIDENTIFIER;
DECLARE @Koi1Id UNIQUEIDENTIFIER, @Koi2Id UNIQUEIDENTIFIER, @Koi3Id UNIQUEIDENTIFIER, 
        @Koi4Id UNIQUEIDENTIFIER, @Koi5Id UNIQUEIDENTIFIER, @Koi6Id UNIQUEIDENTIFIER, 
        @Koi7Id UNIQUEIDENTIFIER, @Koi8Id UNIQUEIDENTIFIER, @Koi9Id UNIQUEIDENTIFIER, 
        @Koi10Id UNIQUEIDENTIFIER;

SELECT @UserId1 = Id FROM Account WHERE Username = 'user1';
SELECT @UserId2 = Id FROM Account WHERE Username = 'user2';
SELECT @UserId3 = Id FROM Account WHERE Username = 'user3';
SELECT @UserId4 = Id FROM Account WHERE Username = 'user4';
SELECT @UserId5 = Id FROM Account WHERE Username = 'user5';

SELECT @Koi1Id = Id FROM KoiProfile WHERE Name = 'Ruby Dragon';
SELECT @Koi2Id = Id FROM KoiProfile WHERE Name = 'Midnight Shadow';
SELECT @Koi3Id = Id FROM KoiProfile WHERE Name = 'Tricolor Beauty';
SELECT @Koi4Id = Id FROM KoiProfile WHERE Name = 'Blue Sapphire';
SELECT @Koi5Id = Id FROM KoiProfile WHERE Name = 'Spotted Pearl';
SELECT @Koi6Id = Id FROM KoiProfile WHERE Name = 'Striking Contrast';
SELECT @Koi7Id = Id FROM KoiProfile WHERE Name = 'Rainbow Jewel';
SELECT @Koi8Id = Id FROM KoiProfile WHERE Name = 'Golden Emperor';
SELECT @Koi9Id = Id FROM KoiProfile WHERE Name = 'Dancing Butterfly';
SELECT @Koi10Id = Id FROM KoiProfile WHERE Name = 'Rising Sun';

-- Thêm đăng ký mới chi tiết
-- Khai báo biến cho các ID đăng ký mới
DECLARE @RegA1_1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @RegA1_2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @RegA1_3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @RegA2_1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @RegA2_2Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[Registration]
([Id], [KoiShowId], [KoiProfileId], [RegistrationNumber], [RegisterName], [KoiSize], [KoiAge], [CompetitionCategoryId], [AccountId], [RegistrationFee], [Rank], [Status], [QRCodeData], [Notes], [ApprovedAt], [CreatedAt], [UpdatedAt])
VALUES
    -- Đăng ký cho Size A1 (15-20cm)
    (@RegA1_1Id, @KoiShowVN2025Id, @Koi1Id, 'VN2025-A1-001', N'Ruby Dragon', 19.80, 1, @CatA1Id, @UserId1, 450000.00, NULL, 'approved', 'REG-VN2025-A1-001', N'Cá Kohaku nhập từ trại Sakai, Nhật Bản, được nuôi trong 6 tháng', DATEADD(DAY, -30, @CurrentDate), DATEADD(DAY, -35, @CurrentDate), DATEADD(DAY, -30, @CurrentDate)),
    
    (@RegA1_2Id, @KoiShowVN2025Id, @Koi4Id, 'VN2025-A1-002', N'Blue Sapphire Jr', 17.20, 1, @CatA1Id, @UserId2, 450000.00, NULL, 'approved', 'REG-VN2025-A1-002', N'Cá Asagi con của cặp bố mẹ đoạt giải năm trước, màu xanh đặc trưng nổi bật', DATEADD(DAY, -28, @CurrentDate), DATEADD(DAY, -35, @CurrentDate), DATEADD(DAY, -28, @CurrentDate)),
    
    (@RegA1_3Id, @KoiShowVN2025Id, @Koi7Id, 'VN2025-A1-003', N'Mini Rainbow', 18.50, 1, @CatA1Id, @UserId3, 450000.00, NULL, 'approved', 'REG-VN2025-A1-003', N'Cá Goshiki với 5 màu rõ rệt dù kích thước còn nhỏ, tiềm năng phát triển cao', DATEADD(DAY, -25, @CurrentDate), DATEADD(DAY, -32, @CurrentDate), DATEADD(DAY, -25, @CurrentDate)),
    
    -- Đăng ký cho Size A2 (20-25cm)
    (@RegA2_1Id, @KoiShowVN2025Id, @Koi10Id, 'VN2025-A2-001', N'Rising Sun Jr', 22.30, 1, @CatA2Id, @UserId5, 500000.00, NULL, 'approved', 'REG-VN2025-A2-001', N'Cá Tancho với đốm đỏ trên đầu tròn đều, thuần chủng', DATEADD(DAY, -27, @CurrentDate), DATEADD(DAY, -34, @CurrentDate), DATEADD(DAY, -27, @CurrentDate)),
    
    (@RegA2_2Id, @KoiShowVN2025Id, @Koi9Id, 'VN2025-A2-002', N'Dancing Butterfly Jr', 24.70, 1, @CatA2Id, @UserId5, 500000.00, NULL, 'approved', 'REG-VN2025-A2-002', N'Cá Butterfly với vây dài đặc trưng, màu sắc sặc sỡ', DATEADD(DAY, -26, @CurrentDate), DATEADD(DAY, -33, @CurrentDate), DATEADD(DAY, -26, @CurrentDate));

-- 15. TẠO DỮ LIỆU CHI TIẾT CHO REGISTRATIONROUND (ĐƠN ĐĂNG KÝ THAM GIA VÒNG THI)
-- Trước tiên, lấy ID của các bể cá A1
DECLARE @TankA1_1Id UNIQUEIDENTIFIER, @TankA1_2Id UNIQUEIDENTIFIER, @TankA1_3Id UNIQUEIDENTIFIER;

SELECT TOP 1 @TankA1_1Id = Id FROM Tank WHERE Name = 'Tank A1-01';
SELECT TOP 1 @TankA1_2Id = Id FROM Tank WHERE Name = 'Tank A1-02';
SELECT TOP 1 @TankA1_3Id = Id FROM Tank WHERE Name = 'Tank A1-03';

-- Thêm dữ liệu cho RegistrationRound
INSERT INTO [dbo].[RegistrationRound]
([Id], [RegistrationId], [RoundId], [CheckInTime], [CheckOutTime], [TankId], [Status], [Notes], [CreatedAt], [UpdatedAt])
VALUES
    -- Vòng sơ khảo cho đăng ký Size A1
    (NEWID(), @RegA1_1Id, @RoundA1_Prelim, DATEADD(HOUR, 7, DATEADD(DAY, 30, @CurrentDate)), NULL, @TankA1_1Id, 'confirmed', N'Cá khỏe mạnh, đã kiểm tra kích thước chính xác: 19.8cm', DATEADD(DAY, -20, @CurrentDate), NULL),
    (NEWID(), @RegA1_2Id, @RoundA1_Prelim, DATEADD(HOUR, 7, DATEADD(DAY, 30, @CurrentDate)), NULL, @TankA1_2Id, 'confirmed', N'Cá khỏe mạnh, đã kiểm tra kích thước chính xác: 17.2cm', DATEADD(DAY, -20, @CurrentDate), NULL),
    (NEWID(), @RegA1_3Id, @RoundA1_Prelim, DATEADD(HOUR, 8, DATEADD(DAY, 30, @CurrentDate)), NULL, @TankA1_3Id, 'confirmed', N'Cá khỏe mạnh, đã kiểm tra kích thước chính xác: 18.5cm', DATEADD(DAY, -20, @CurrentDate), NULL),
    
    -- Vòng đánh giá chính cho đăng ký Size A1 (giả sử chỉ 2 cá vượt qua vòng sơ khảo)
    (NEWID(), @RegA1_1Id, @RoundA1_Main, DATEADD(HOUR, 14, DATEADD(DAY, 30, @CurrentDate)), NULL, @TankA1_1Id, 'pending', NULL, DATEADD(DAY, -20, @CurrentDate), NULL),
    (NEWID(), @RegA1_3Id, @RoundA1_Main, DATEADD(HOUR, 14, DATEADD(DAY, 30, @CurrentDate)), NULL, @TankA1_3Id, 'pending', NULL, DATEADD(DAY, -20, @CurrentDate), NULL),
    
    -- Vòng chung kết cho đăng ký Size A1 (giả sử chỉ 1 cá vượt qua vòng đánh giá chính)
    (NEWID(), @RegA1_1Id, @RoundA1_Final, DATEADD(HOUR, 9, DATEADD(DAY, 32, @CurrentDate)), NULL, @TankA1_1Id, 'pending', NULL, DATEADD(DAY, -20, @CurrentDate), NULL);

-- 16. TẠO DỮ LIỆU CHI TIẾT CHO REGISTRATIONPAYMENT (THANH TOÁN ĐĂNG KÝ)
INSERT INTO [dbo].[RegistrationPayment]
([Id], [RegistrationId], [QRCodeData], [PaidAmount], [PaymentDate], [TransactionCode], [PaymentMethod], [Status])
VALUES
    -- Thanh toán cho đăng ký Size A1
    (NEWID(), @RegA1_1Id, 'PAY-VN2025-A1-001', 450000.00, DATEADD(DAY, -33, @CurrentDate), 'TRX-A1-001', N'Banking', 'completed'),
    (NEWID(), @RegA1_2Id, 'PAY-VN2025-A1-002', 450000.00, DATEADD(DAY, -30, @CurrentDate), 'TRX-A1-002', N'Banking', 'completed'),
    (NEWID(), @RegA1_3Id, 'PAY-VN2025-A1-003', 450000.00, DATEADD(DAY, -29, @CurrentDate), 'TRX-A1-003', N'Banking', 'completed'),
    
    -- Thanh toán cho đăng ký Size A2
    (NEWID(), @RegA2_1Id, 'PAY-VN2025-A2-001', 500000.00, DATEADD(DAY, -31, @CurrentDate), 'TRX-A2-001', N'Banking', 'completed'),
    (NEWID(), @RegA2_2Id, 'PAY-VN2025-A2-002', 500000.00, DATEADD(DAY, -30, @CurrentDate), 'TRX-A2-002', N'Banking', 'completed');

-- 17. TẠO DỮ LIỆU CHI TIẾT CHO KOIMEDIAS (HÌNH ẢNH CÁ)
INSERT INTO [dbo].[KoiMedia]
([Id], [MediaUrl], [MediaType], [KoiProfileId], [RegistrationId])
VALUES
    -- Hình ảnh cho đăng ký Size A1
    (NEWID(), 'koimedia/registration_ruby_a1_1.jpg', 'Image', NULL, @RegA1_1Id),
    (NEWID(), 'koimedia/registration_ruby_a1_2.jpg', 'Image', NULL, @RegA1_1Id),
    (NEWID(), 'koimedia/registration_ruby_a1_3.jpg', 'Image', NULL, @RegA1_1Id),
    (NEWID(), 'koimedia/registration_ruby_a1_video.mp4', 'Video', NULL, @RegA1_1Id),
    
    (NEWID(), 'koimedia/registration_blue_a1_1.jpg', 'Image', NULL, @RegA1_2Id),
    (NEWID(), 'koimedia/registration_blue_a1_2.jpg', 'Image', NULL, @RegA1_2Id),
    (NEWID(), 'koimedia/registration_blue_a1_video.mp4', 'Video', NULL, @RegA1_2Id),
    
    (NEWID(), 'koimedia/registration_rainbow_a1_1.jpg', 'Image', NULL, @RegA1_3Id),
    (NEWID(), 'koimedia/registration_rainbow_a1_2.jpg', 'Image', NULL, @RegA1_3Id),
    (NEWID(), 'koimedia/registration_rainbow_a1_video.mp4', 'Video', NULL, @RegA1_3Id),
    
    -- Hình ảnh cho đăng ký Size A2
    (NEWID(), 'koimedia/registration_risingsun_a2_1.jpg', 'Image', NULL, @RegA2_1Id),
    (NEWID(), 'koimedia/registration_risingsun_a2_2.jpg', 'Image', NULL, @RegA2_1Id),
    (NEWID(), 'koimedia/registration_risingsun_a2_video.mp4', 'Video', NULL, @RegA2_1Id),
    
    (NEWID(), 'koimedia/registration_butterfly_a2_1.jpg', 'Image', NULL, @RegA2_2Id),
    (NEWID(), 'koimedia/registration_butterfly_a2_2.jpg', 'Image', NULL, @RegA2_2Id),
    (NEWID(), 'koimedia/registration_butterfly_a2_video.mp4', 'Video', NULL, @RegA2_2Id);

-- 18. TẠO DỮ LIỆU CHI TIẾT CHO VOTE (BÌNH CHỌN)
INSERT INTO [dbo].[Vote]
([Id], [AccountId], [RegistrationId], [Prediction], [CreatedAt], [UpdatedAt])
VALUES
    -- Bình chọn cho đăng ký Size A1
    (NEWID(), @UserId4, @RegA1_1Id, N'Cá Kohaku này có màu đỏ rất đẹp và sắc nét, tôi dự đoán sẽ đạt giải nhất trong hạng mục của nó!', DATEADD(DAY, -15, @CurrentDate), NULL),
    (NEWID(), @UserId5, @RegA1_1Id, N'Màu đỏ và trắng tương phản rõ rệt, mẫu phân bố cân đối. Là ứng viên sáng giá cho giải nhất.', DATEADD(DAY, -14, @CurrentDate), NULL),
    (NEWID(), @UserId3, @RegA1_1Id, N'Cá có tiềm năng nhưng tôi nghĩ viền màu đỏ còn chưa sắc nét lắm, có thể đạt giải nhì hoặc ba.', DATEADD(DAY, -13, @CurrentDate), NULL),
    
    (NEWID(), @UserId1, @RegA1_2Id, N'Màu xanh của Asagi này rất đặc trưng và đẹp, có thể cạnh tranh tốt cho giải nhất.', DATEADD(DAY, -15, @CurrentDate), NULL),
    (NEWID(), @UserId4, @RegA1_2Id, N'Cá có màu đẹp nhưng hình dáng chưa cân đối lắm, tôi dự đoán giải ba.', DATEADD(DAY, -12, @CurrentDate), NULL),
    
    (NEWID(), @UserId1, @RegA1_3Id, N'Goshiki với 5 màu rõ rệt, rất hiếm thấy ở cá nhỏ. Tiềm năng đạt giải cao.', DATEADD(DAY, -14, @CurrentDate), NULL),
    (NEWID(), @UserId2, @RegA1_3Id, N'Cá này thực sự nổi bật với các màu sắc rõ ràng, tôi nghĩ sẽ lọt vào top 3.', DATEADD(DAY, -12, @CurrentDate), NULL),
    
    -- Bình chọn cho đăng ký Size A2
    (NEWID(), @UserId1, @RegA2_1Id, N'Tancho với đốm đỏ tròn đều trên đầu, rất đẹp. Có cơ hội đạt giải cao.', DATEADD(DAY, -15, @CurrentDate), NULL),
    (NEWID(), @UserId2, @RegA2_1Id, N'Cá có vẻ đẹp truyền thống của Tancho, nhưng tôi thấy phần thân hơi mỏng, có thể đạt giải nhì.', DATEADD(DAY, -13, @CurrentDate), NULL),
    
    (NEWID(), @UserId1, @RegA2_2Id, N'Butterfly với vây dài và màu sắc đẹp, tuy nhiên không phải là giống truyền thống cao cấp, khó đạt giải cao.', DATEADD(DAY, -14, @CurrentDate), NULL),
    (NEWID(), @UserId3, @RegA2_2Id, N'Cá có màu sắc nổi bật và vây đẹp, nhưng giám khảo thường ưu tiên các giống truyền thống. Có thể lọt top 5.', DATEADD(DAY, -11, @CurrentDate), NULL);

-- 19. TẠO DỮ LIỆU CHI TIẾT CHO NOTIFICATION (THÔNG BÁO)
INSERT INTO [dbo].[Notification]
([Id], [AccountId], [Content], [SentDate], [Title], [Type], [IsRead])
VALUES
    -- Thông báo cho người đăng ký Size A1
    (NEWID(), @UserId1, N'Đăng ký của bạn cho cá "Ruby Dragon" trong hạng mục Size A1 đã được chấp nhận!', DATEADD(DAY, -30, @CurrentDate), N'Đăng ký thành công - Size A1', 'registration', 1),
    (NEWID(), @UserId1, N'Thanh toán phí đăng ký 450.000 VNĐ cho cá "Ruby Dragon" đã được xác nhận. Cảm ơn bạn!', DATEADD(DAY, -33, @CurrentDate), N'Xác nhận thanh toán - Size A1', 'payment', 1),
    (NEWID(), @UserId1, N'Vui lòng mang cá "Ruby Dragon" đến địa điểm thi đấu vào ngày 05/04/2025 từ 6h00 đến 8h00 sáng để check-in.', DATEADD(DAY, -10, @CurrentDate), N'Thông báo check-in - Size A1', 'event', 1),
    
    (NEWID(), @UserId2, N'Đăng ký của bạn cho cá "Blue Sapphire Jr" trong hạng mục Size A1 đã được chấp nhận!', DATEADD(DAY, -28, @CurrentDate), N'Đăng ký thành công - Size A1', 'registration', 1),
    (NEWID(), @UserId2, N'Thanh toán phí đăng ký 450.000 VNĐ cho cá "Blue Sapphire Jr" đã được xác nhận. Cảm ơn bạn!', DATEADD(DAY, -30, @CurrentDate), N'Xác nhận thanh toán - Size A1', 'payment', 1),
    (NEWID(), @UserId2, N'Vui lòng mang cá "Blue Sapphire Jr" đến địa điểm thi đấu vào ngày 05/04/2025 từ 6h00 đến 8h00 sáng để check-in.', DATEADD(DAY, -10, @CurrentDate), N'Thông báo check-in - Size A1', 'event', 1),
    
    -- Thông báo đặc biệt cho người tham gia
    (NEWID(), NULL, N'Lịch thi đấu chi tiết cho Koi Show Việt Nam 2025 đã được cập nhật. Vui lòng xem chi tiết tại website chính thức.', DATEADD(DAY, -15, @CurrentDate), N'Cập nhật lịch thi đấu', 'event', 0),
    (NEWID(), NULL, N'Hội thảo "Kỹ thuật nuôi cá Koi chất lượng cao" sẽ được tổ chức vào ngày 04/04/2025 lúc 14h00. Đăng ký tham gia ngay!', DATEADD(DAY, -20, @CurrentDate), N'Hội thảo đặc biệt', 'event', 0),
    (NEWID(), NULL, N'Vé tham dự Koi Show Việt Nam 2025 đã mở bán! Mua sớm để nhận ưu đãi và đảm bảo chỗ tham quan.', DATEADD(DAY, -45, @CurrentDate), N'Vé đã mở bán', 'ticket', 0);

-- 20. TẠO DỮ LIỆU CHI TIẾT CHO ĐÁNH GIÁ CỦA GIÁM KHẢO (JUDGING)
-- Tạo bảng tạm thời để chứa dữ liệu đánh giá (vì trong cơ sở dữ liệu gốc có thể chưa có bảng này)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'JudgingScore')
BEGIN
    CREATE TABLE [dbo].[JudgingScore] (
        [Id] UNIQUEIDENTIFIER PRIMARY KEY,
        [RegistrationRoundId] UNIQUEIDENTIFIER,
        [RefereeAccountId] UNIQUEIDENTIFIER,
        [CriteriaId] UNIQUEIDENTIFIER,
        [Score] DECIMAL(5,2),
        [Comment] NVARCHAR(MAX),
        [JudgedAt] DATETIME,
        [CreatedAt] DATETIME,
        [UpdatedAt] DATETIME
    );
END
ELSE
BEGIN
    -- Xóa dữ liệu cũ (nếu có)
    TRUNCATE TABLE [dbo].[JudgingScore];
END

-- Lấy ID của RegistrationRound đã tạo (để đơn giản chỉ lấy 1 ví dụ cho vòng sơ khảo của RegA1_1Id)
DECLARE @RegRoundA1_1Id UNIQUEIDENTIFIER;
SELECT TOP 1 @RegRoundA1_1Id = Id FROM [dbo].[RegistrationRound] WHERE RegistrationId = @RegA1_1Id AND RoundId = @RoundA1_Prelim;

-- Thêm dữ liệu điểm số của giám khảo
INSERT INTO [dbo].[JudgingScore]
([Id], [RegistrationRoundId], [RefereeAccountId], [CriteriaId], [Score], [Comment], [JudgedAt], [CreatedAt], [UpdatedAt])
VALUES
    -- Điểm của Giám khảo 1 cho đăng ký A1_1 trong vòng sơ khảo
    (NEWID(), @RegRoundA1_1Id, @RefereeId1, @CriteriaColorId, 8.5, N'Màu sắc đỏ và trắng rõ ràng, tương phản tốt, độ bóng cao', DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), NULL),
    (NEWID(), @RegRoundA1_1Id, @RefereeId1, @CriteriaPatternId, 8.0, N'Mẫu phân bố khá cân đối, đặc biệt là phần đầu và thân', DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), NULL),
    (NEWID(), @RegRoundA1_1Id, @RefereeId1, @CriteriaBodyId, 8.2, N'Hình dáng cân đối, tỷ lệ đầu-thân-đuôi hài hòa', DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), NULL),
    (NEWID(), @RegRoundA1_1Id, @RefereeId1, @CriteriaFinnageId, 7.8, N'Vây và đuôi khá tốt, nhưng vây ngực hơi nhỏ', DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), NULL),
    (NEWID(), @RegRoundA1_1Id, @RefereeId1, @CriteriaHealthId, 9.0, N'Cá khỏe mạnh, bơi năng động và cân bằng', DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), NULL),
    (NEWID(), @RegRoundA1_1Id, @RefereeId1, @CriteriaSkinId, 8.5, N'Da và vảy bóng đẹp, không có dấu hiệu tổn thương', DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), NULL),
    
    -- Điểm của Giám khảo 2 cho đăng ký A1_1 trong vòng sơ khảo
    (NEWID(), @RegRoundA1_1Id, @RefereeId2, @CriteriaColorId, 8.7, N'Màu sắc sống động và tươi, độ tương phản rất tốt', DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), NULL),
    (NEWID(), @RegRoundA1_1Id, @RefereeId2, @CriteriaPatternId, 8.3, N'Mẫu khá cân đối và đặc trưng cho giống Kohaku', DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), NULL),
    (NEWID(), @RegRoundA1_1Id, @RefereeId2, @CriteriaBodyId, 8.5, N'Hình dáng rất cân đối, lưng thẳng và đầu tròn đẹp', DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), NULL),
    (NEWID(), @RegRoundA1_1Id, @RefereeId2, @CriteriaFinnageId, 8.0, N'Vây đuôi phát triển tốt, vây ngực và lưng cân đối', DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), NULL),
    (NEWID(), @RegRoundA1_1Id, @RefereeId2, @CriteriaHealthId, 8.8, N'Cá rất khỏe mạnh, bơi tích cực và tự tin', DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), NULL),
    (NEWID(), @RegRoundA1_1Id, @RefereeId2, @CriteriaSkinId, 8.2, N'Da bóng đẹp, vảy sắp xếp gọn gàng, có một vài vảy nhỏ không đều', DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), NULL),
    
    -- Điểm của Giám khảo 3 cho đăng ký A1_1 trong vòng sơ khảo
    (NEWID(), @RegRoundA1_1Id, @RefereeId3, @CriteriaColorId, 8.8, N'Màu sắc xuất sắc, màu đỏ rất sáng và trắng tinh khiết', DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), NULL),
    (NEWID(), @RegRoundA1_1Id, @RefereeId3, @CriteriaPatternId, 8.5, N'Mẫu phân bố hài hòa, ranh giới giữa đỏ và trắng rõ ràng', DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), NULL),
    (NEWID(), @RegRoundA1_1Id, @RefereeId3, @CriteriaBodyId, 8.0, N'Hình dáng tốt, tương đối cân đối nhưng đầu hơi lớn so với thân', DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), NULL),
    (NEWID(), @RegRoundA1_1Id, @RefereeId3, @CriteriaFinnageId, 7.5, N'Vây và đuôi khá tốt, nhưng vây lưng hơi ngắn', DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), NULL),
    (NEWID(), @RegRoundA1_1Id, @RefereeId3, @CriteriaHealthId, 9.2, N'Cá rất khỏe mạnh, bơi linh hoạt và năng động', DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), NULL),
    (NEWID(), @RegRoundA1_1Id, @RefereeId3, @CriteriaSkinId, 8.7, N'Da và vảy rất bóng, sạch sẽ và không có khuyết điểm', DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), DATEADD(HOUR, 9, DATEADD(DAY, 30, @CurrentDate)), NULL);