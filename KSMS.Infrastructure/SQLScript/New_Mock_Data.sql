DECLARE @CurrentDate DATETIME = '2025-03-05';

-- 1. TẠO DỮ LIỆU CHO ACCOUNT
DECLARE @AdminId1 UNIQUEIDENTIFIER = NEWID();
DECLARE @AdminId2 UNIQUEIDENTIFIER = NEWID();
DECLARE @ManagerId1 UNIQUEIDENTIFIER = NEWID();
DECLARE @ManagerId2 UNIQUEIDENTIFIER = NEWID();
DECLARE @RefereeId1 UNIQUEIDENTIFIER = NEWID();
DECLARE @RefereeId2 UNIQUEIDENTIFIER = NEWID();
DECLARE @RefereeId3 UNIQUEIDENTIFIER = NEWID();
DECLARE @StaffId1 UNIQUEIDENTIFIER = NEWID();
DECLARE @StaffId2 UNIQUEIDENTIFIER = NEWID();
DECLARE @UserId1 UNIQUEIDENTIFIER = NEWID();
DECLARE @UserId2 UNIQUEIDENTIFIER = NEWID();
DECLARE @UserId3 UNIQUEIDENTIFIER = NEWID();
DECLARE @UserId4 UNIQUEIDENTIFIER = NEWID();
DECLARE @UserId5 UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[Account]
([Id], [Email], [HashedPassword], [Username], [FullName], [Phone], [Avatar], [Role], [Status], [ConfirmationToken], [IsConfirmed], [CreatedAt], [UpdatedAt])
VALUES
    -- Admin accounts
    (@AdminId1, 'admin@koishow.com', 'pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=', 'admin', N'Nguyễn Văn Admin', '0901234567', 'avatar1.jpg', 'Admin', 'active', NULL, 1, DATEADD(MONTH, -14, @CurrentDate), NULL),
    (@AdminId2, 'admin2@koishow.com', 'pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=', 'admin2', N'Trần Thị Admin', '0901234568', 'avatar2.jpg', 'Admin', 'active', NULL, 1, DATEADD(MONTH, -14, @CurrentDate), NULL),

    -- Manager accounts
    (@ManagerId1, 'manager@koishow.com', 'pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=', 'manager1', N'Lê Văn Manager', '0901234569', 'avatar3.jpg', 'Manager', 'active', NULL, 1, DATEADD(MONTH, -13, @CurrentDate), NULL),
    (@ManagerId2, 'manager2@koishow.com', 'pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=', 'manager2', N'Phạm Thị Manager', '0901234570', 'avatar4.jpg', 'Manager', 'active', NULL, 1, DATEADD(MONTH, -13, @CurrentDate), NULL),

    -- Referee accounts
    (@RefereeId1, 'referee1@koishow.com', 'pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=', 'referee1', N'Hoàng Văn Referee', '0901234571', 'avatar5.jpg', 'Referee', 'active', NULL, 1, DATEADD(MONTH, -12, @CurrentDate), NULL),
    (@RefereeId2, 'referee2@koishow.com', 'pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=', 'referee2', N'Ngô Thị Referee', '0901234572', 'avatar6.jpg', 'Referee', 'active', NULL, 1, DATEADD(MONTH, -12, @CurrentDate), NULL),
    (@RefereeId3, 'referee3@koishow.com', 'pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=', 'referee3', N'Đặng Văn Referee', '0901234573', 'avatar7.jpg', 'Referee', 'active', NULL, 1, DATEADD(MONTH, -12, @CurrentDate), NULL),

    -- Staff accounts
    (@StaffId1, 'staff1@koishow.com', 'pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=', 'staff1', N'Dương Văn Staff', '0901234574', 'avatar8.jpg', 'Staff', 'active', NULL, 1, DATEADD(MONTH, -11, @CurrentDate), NULL),
    (@StaffId2, 'staff2@koishow.com', 'pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=', 'staff2', N'Bùi Thị Staff', '0901234575', 'avatar9.jpg', 'Staff', 'active', NULL, 1, DATEADD(MONTH, -11, @CurrentDate), NULL),
    
    -- User accounts
    (@UserId1, 'user1@example.com', 'pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=', 'user1', N'Trần Văn User', '0901234576', 'avatar10.jpg', 'User', 'active', NULL, 1, DATEADD(MONTH, -10, @CurrentDate), NULL),
    (@UserId2, 'user2@example.com', 'pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=', 'user2', N'Lý Thị User', '0901234577', 'avatar11.jpg', 'User', 'active', NULL, 1, DATEADD(MONTH, -10, @CurrentDate), NULL),
    (@UserId3, 'user3@example.com', 'pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=', 'user3', N'Phan Văn User', '0901234578', 'avatar12.jpg', 'User', 'active', NULL, 1, DATEADD(MONTH, -9, @CurrentDate), NULL),
    (@UserId4, 'user4@example.com', 'pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=', 'user4', N'Võ Thị User', '0901234579', 'avatar13.jpg', 'User', 'active', NULL, 1, DATEADD(MONTH, -9, @CurrentDate), NULL),
    (@UserId5, 'user5@example.com', 'pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=', 'user5', N'Đỗ Văn User', '0901234580', 'avatar14.jpg', 'User', 'active', NULL, 1, DATEADD(MONTH, -8, @CurrentDate), NULL);

-- 2. TẠO DỮ LIỆU CHO VARIETY (GIỐNG CÁ)
DECLARE @KohakuId UNIQUEIDENTIFIER = NEWID();
DECLARE @ShowaId UNIQUEIDENTIFIER = NEWID();
DECLARE @SankeId UNIQUEIDENTIFIER = NEWID();
DECLARE @AsagiId UNIQUEIDENTIFIER = NEWID();
DECLARE @BekkoId UNIQUEIDENTIFIER = NEWID();
DECLARE @UtsurimonoId UNIQUEIDENTIFIER = NEWID();
DECLARE @GoshikiId UNIQUEIDENTIFIER = NEWID();
DECLARE @OgonId UNIQUEIDENTIFIER = NEWID();
DECLARE @ButterflyId UNIQUEIDENTIFIER = NEWID();
DECLARE @TanchoId UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[Variety]
([Id], [Name], [Description], [CreatedAt], [UpdatedAt])
VALUES
    (@KohakuId, N'Kohaku', N'Cá Koi trắng với các mảng đỏ', DATEADD(MONTH, -24, @CurrentDate), NULL),
    (@ShowaId, N'Showa', N'Cá Koi đen với các mảng trắng và đỏ', DATEADD(MONTH, -24, @CurrentDate), NULL),
    (@SankeId, N'Sanke', N'Cá Koi trắng với các mảng đỏ và đen', DATEADD(MONTH, -24, @CurrentDate), NULL),
    (@AsagiId, N'Asagi', N'Cá Koi xanh với mẫu vảy lưới', DATEADD(MONTH, -24, @CurrentDate), NULL),
    (@BekkoId, N'Bekko', N'Cá Koi trắng, đỏ hoặc vàng với đốm đen', DATEADD(MONTH, -24, @CurrentDate), NULL),
    (@UtsurimonoId, N'Utsurimono', N'Cá Koi đen với các mảng trắng, đỏ, hoặc vàng', DATEADD(MONTH, -24, @CurrentDate), NULL),
    (@GoshikiId, N'Goshiki', N'Cá Koi "năm màu"', DATEADD(MONTH, -24, @CurrentDate), NULL),
    (@OgonId, N'Ogon', N'Cá Koi metallic một màu', DATEADD(MONTH, -24, @CurrentDate), NULL),
    (@ButterflyId, N'Butterfly Koi', N'Cá Koi với vây dài', DATEADD(MONTH, -24, @CurrentDate), NULL),
    (@TanchoId, N'Tancho', N'Cá Koi trắng với đốm đỏ trên đầu', DATEADD(MONTH, -24, @CurrentDate), NULL);

-- 3. TẠO DỮ LIỆU CHO KOISHOW (CÁC CUỘC THI)
DECLARE @KoiShowHNId UNIQUEIDENTIFIER = NEWID();
DECLARE @KoiShowHCMId UNIQUEIDENTIFIER = NEWID();
DECLARE @KoiShowDNId UNIQUEIDENTIFIER = NEWID();
DECLARE @KoiShowMekongId UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[KoiShow]
([Id], [Name], [StartDate], [EndDate], [StartExhibitionDate], [EndExhibitionDate], [Location], [Description], [RegistrationDeadline], [MinParticipants], [MaxParticipants], [HasGrandChampion], [HasBestInShow], [ImgURL], [Status], [CreatedAt], [UpdatedAt])
VALUES
    (@KoiShowHNId, N'Koi Show Việt Nam 2025', DATEADD(DAY, 30, @CurrentDate), DATEADD(DAY, 32, @CurrentDate), DATEADD(DAY, 30, @CurrentDate), DATEADD(DAY, 32, @CurrentDate), N'Hà Nội', N'Cuộc thi Koi lớn nhất Việt Nam năm 2025', DATEADD(DAY, 25, @CurrentDate), 50, 200, 1, 1, 'koishow1.jpg', 'upcoming', DATEADD(MONTH, -6, @CurrentDate), NULL),
    (@KoiShowHCMId, N'Koi Show Hồ Chí Minh 2025', DATEADD(DAY, 60, @CurrentDate), DATEADD(DAY, 62, @CurrentDate), DATEADD(DAY, 60, @CurrentDate), DATEADD(DAY, 62, @CurrentDate), N'TP. Hồ Chí Minh', N'Cuộc thi Koi lớn nhất miền Nam năm 2025', DATEADD(DAY, 55, @CurrentDate), 40, 150, 1, 1, 'koishow2.jpg', 'upcoming', DATEADD(MONTH, -5, @CurrentDate), NULL),
    (@KoiShowDNId, N'Koi Show Đà Nẵng 2025', DATEADD(DAY, 90, @CurrentDate), DATEADD(DAY, 92, @CurrentDate), DATEADD(DAY, 90, @CurrentDate), DATEADD(DAY, 92, @CurrentDate), N'Đà Nẵng', N'Cuộc thi Koi miền Trung năm 2025', DATEADD(DAY, 85, @CurrentDate), 30, 100, 1, 0, 'koishow3.jpg', 'upcoming', DATEADD(MONTH, -4, @CurrentDate), NULL),
    (@KoiShowMekongId, N'Koi Show Mekong 2024', DATEADD(MONTH, -3, @CurrentDate), DATEADD(MONTH, -3, @CurrentDate), DATEADD(MONTH, -3, @CurrentDate), DATEADD(MONTH, -3, @CurrentDate), N'Cần Thơ', N'Cuộc thi Koi vùng ĐBSCL 2024', DATEADD(MONTH, -4, @CurrentDate), 25, 80, 0, 1, 'koishow4.jpg', 'completed', DATEADD(MONTH, -8, @CurrentDate), DATEADD(MONTH, -3, @CurrentDate));

-- 4. TẠO DỮ LIỆU CHO COMPETITIONCATEGORY (DANH MỤC THI ĐẤU)
DECLARE @Category1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Category2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Category3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Category4Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Category5Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Category6Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Category7Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Category8Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Category9Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[CompetitionCategory]
([Id], [KoiShowId], [Name], [SizeMin], [SizeMax], [Description], [MaxEntries], [HasTank], [RegistrationFee], [StartTime], [EndTime], [Status], [CreatedAt], [UpdatedAt])
VALUES
    -- Cho Koi Show Việt Nam 2025
    (@Category1Id, @KoiShowHNId, N'Size 1 (15-25cm)', 15.00, 25.00, N'Hạng mục cho cá kích thước 15-25cm', 40, 1, 500000.00, DATEADD(DAY, 30, @CurrentDate), DATEADD(DAY, 30, @CurrentDate), 'active', DATEADD(MONTH, -6, @CurrentDate), NULL),
    (@Category2Id, @KoiShowHNId, N'Size 2 (25-35cm)', 25.01, 35.00, N'Hạng mục cho cá kích thước 25-35cm', 40, 1, 700000.00, DATEADD(DAY, 30, @CurrentDate), DATEADD(DAY, 30, @CurrentDate), 'active', DATEADD(MONTH, -6, @CurrentDate), NULL),
    (@Category3Id, @KoiShowHNId, N'Size 3 (35-45cm)', 35.01, 45.00, N'Hạng mục cho cá kích thước 35-45cm', 35, 1, 900000.00, DATEADD(DAY, 30, @CurrentDate), DATEADD(DAY, 31, @CurrentDate), 'active', DATEADD(MONTH, -6, @CurrentDate), NULL),
    (@Category4Id, @KoiShowHNId, N'Size 4 (45-55cm)', 45.01, 55.00, N'Hạng mục cho cá kích thước 45-55cm', 30, 1, 1100000.00, DATEADD(DAY, 31, @CurrentDate), DATEADD(DAY, 31, @CurrentDate), 'active', DATEADD(MONTH, -6, @CurrentDate), NULL),
    (@Category5Id, @KoiShowHNId, N'Size 5 (55-65cm)', 55.01, 65.00, N'Hạng mục cho cá kích thước 55-65cm', 25, 1, 1300000.00, DATEADD(DAY, 31, @CurrentDate), DATEADD(DAY, 31, @CurrentDate), 'active', DATEADD(MONTH, -6, @CurrentDate), NULL),
    (@Category6Id, @KoiShowHNId, N'Size 6 (65cm+)', 65.01, 100.00, N'Hạng mục cho cá kích thước trên 65cm', 20, 1, 1500000.00, DATEADD(DAY, 31, @CurrentDate), DATEADD(DAY, 32, @CurrentDate), 'active', DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    -- Cho Koi Show Hồ Chí Minh 2025
    (@Category7Id, @KoiShowHCMId, N'Junior (15-30cm)', 15.00, 30.00, N'Hạng mục cho cá kích thước 15-30cm', 30, 1, 600000.00, DATEADD(DAY, 60, @CurrentDate), DATEADD(DAY, 60, @CurrentDate), 'active', DATEADD(MONTH, -5, @CurrentDate), NULL),
    (@Category8Id, @KoiShowHCMId, N'Medium (30-50cm)', 30.01, 50.00, N'Hạng mục cho cá kích thước 30-50cm', 30, 1, 800000.00, DATEADD(DAY, 60, @CurrentDate), DATEADD(DAY, 61, @CurrentDate), 'active', DATEADD(MONTH, -5, @CurrentDate), NULL),
    (@Category9Id, @KoiShowHCMId, N'Large (50cm+)', 50.01, 100.00, N'Hạng mục cho cá kích thước trên 50cm', 25, 1, 1000000.00, DATEADD(DAY, 61, @CurrentDate), DATEADD(DAY, 62, @CurrentDate), 'active', DATEADD(MONTH, -5, @CurrentDate), NULL);

-- 5. TẠO DỮ LIỆU CHO CATEGORYVARIETY (LIÊN KẾT GIỮA DANH MỤC VÀ GIỐNG CÁ)
INSERT INTO [dbo].[CategoryVariety]
([Id], [VarietyId], [CompetitionCategoryId])
VALUES
    -- Size 1 (15-25cm) - Tất cả các giống
    (NEWID(), @KohakuId, @Category1Id),
    (NEWID(), @ShowaId, @Category1Id),
    (NEWID(), @SankeId, @Category1Id),
    (NEWID(), @AsagiId, @Category1Id),
    (NEWID(), @BekkoId, @Category1Id),
    
    -- Size 2 (25-35cm) - Tất cả các giống
    (NEWID(), @KohakuId, @Category2Id),
    (NEWID(), @ShowaId, @Category2Id),
    (NEWID(), @SankeId, @Category2Id),
    (NEWID(), @UtsurimonoId, @Category2Id),
    (NEWID(), @GoshikiId, @Category2Id),
    
    -- Junior (15-30cm) - HCM
    (NEWID(), @KohakuId, @Category7Id),
    (NEWID(), @ShowaId, @Category7Id),
    (NEWID(), @SankeId, @Category7Id);

-- 6. TẠO DỮ LIỆU CHO KOIPROFILE (HỒ SƠ CÁ)
DECLARE @Koi1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Koi2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Koi3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Koi4Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Koi5Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Koi6Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Koi7Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Koi8Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Koi9Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Koi10Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[KoiProfile]
([Id], [OwnerId], [VarietyId], [Name], [Size], [Age], [Gender], [Bloodline], [Status], [IsPublic], [CreatedAt], [UpdatedAt])
VALUES
    -- User 1's Koi
    (@Koi1Id, @UserId1, @KohakuId, N'Ruby Dragon', 22.50, 2, N'Female', N'Dainichi', 'active', 1, DATEADD(MONTH, -9, @CurrentDate), NULL),
    (@Koi2Id, @UserId1, @ShowaId, N'Midnight Shadow', 38.20, 3, N'Male', N'Sakai', 'active', 1, DATEADD(MONTH, -9, @CurrentDate), NULL),
    (@Koi3Id, @UserId1, @SankeId, N'Tricolor Beauty', 52.70, 4, N'Female', N'Marudo', 'active', 1, DATEADD(MONTH, -9, @CurrentDate), NULL),
    
    -- User 2's Koi
    (@Koi4Id, @UserId2, @AsagiId, N'Blue Sapphire', 28.40, 2, N'Male', N'Isa', 'active', 1, DATEADD(MONTH, -8, @CurrentDate), NULL),
    (@Koi5Id, @UserId2, @BekkoId, N'Spotted Pearl', 44.10, 3, N'Female', N'Marusaka', 'active', 1, DATEADD(MONTH, -8, @CurrentDate), NULL),
    
    -- User 3's Koi
    (@Koi6Id, @UserId3, @UtsurimonoId, N'Striking Contrast', 58.90, 5, N'Male', N'Omosako', 'active', 1, DATEADD(MONTH, -7, @CurrentDate), NULL),
    (@Koi7Id, @UserId3, @GoshikiId, N'Rainbow Jewel', 32.60, 2, N'Female', N'Konguryu', 'active', 1, DATEADD(MONTH, -7, @CurrentDate), NULL),
    
    -- User 4's Koi
    (@Koi8Id, @UserId4, @OgonId, N'Golden Emperor', 68.30, 6, N'Male', N'Uedera', 'active', 1, DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    -- User 5's Koi
    (@Koi9Id, @UserId5, @ButterflyId, N'Dancing Butterfly', 42.70, 3, N'Female', N'Hirasawa', 'active', 1, DATEADD(MONTH, -5, @CurrentDate), NULL),
    (@Koi10Id, @UserId5, @TanchoId, N'Rising Sun', 36.40, 2, N'Male', N'Ogata', 'active', 1, DATEADD(MONTH, -5, @CurrentDate), NULL);

-- 7. TẠO DỮ LIỆU CHO SHOWSTAFF (NHÂN VIÊN CUỘC THI)
INSERT INTO [dbo].[ShowStaff]
([Id], [KoiShowId], [AccountId], [AssignedBy], [AssignedAt])
VALUES
    -- Koi Show Việt Nam 2025
    (NEWID(), @KoiShowHNId, @RefereeId1, @ManagerId1, DATEADD(MONTH, -5, @CurrentDate)),
    (NEWID(), @KoiShowHNId, @RefereeId2, @ManagerId1, DATEADD(MONTH, -5, @CurrentDate)),
    (NEWID(), @KoiShowHNId, @RefereeId3, @ManagerId1, DATEADD(MONTH, -5, @CurrentDate)),
    (NEWID(), @KoiShowHNId, @StaffId1, @ManagerId1, DATEADD(MONTH, -5, @CurrentDate)),
    (NEWID(), @KoiShowHNId, @StaffId2, @ManagerId1, DATEADD(MONTH, -5, @CurrentDate)),
    
    -- Koi Show Hồ Chí Minh 2025
    (NEWID(), @KoiShowHCMId, @RefereeId1, @ManagerId2, DATEADD(MONTH, -4, @CurrentDate)),
    (NEWID(), @KoiShowHCMId, @RefereeId2, @ManagerId2, DATEADD(MONTH, -4, @CurrentDate)),
    (NEWID(), @KoiShowHCMId, @RefereeId3, @ManagerId2, DATEADD(MONTH, -4, @CurrentDate)),
    (NEWID(), @KoiShowHCMId, @StaffId1, @ManagerId2, DATEADD(MONTH, -4, @CurrentDate));

-- 8. TẠO DỮ LIỆU CHO ROUND (CÁC VÒNG THI)
DECLARE @Round1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Round2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Round3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Round4Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Round5Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Round6Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Round7Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Round8Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[Round]
([Id], [CompetitionCategoriesId], [Name], [RoundOrder], [RoundType], [StartTime], [EndTime], [NumberOfRegistrationToAdvance], [Status], [CreatedAt], [UpdatedAt])
VALUES
    -- Size 1 (15-25cm) - Vòng sơ khảo và chung kết
    (@Round1Id, @Category1Id, N'Vòng sơ khảo Size 1', 1, 'preliminary', DATEADD(DAY, 30, @CurrentDate), DATEADD(DAY, 30, @CurrentDate), 10, 'upcoming', DATEADD(MONTH, -6, @CurrentDate), NULL),
    (@Round2Id, @Category1Id, N'Chung kết Size 1', 2, 'final', DATEADD(DAY, 30, @CurrentDate), DATEADD(DAY, 30, @CurrentDate), NULL, 'upcoming', DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    -- Size 2 (25-35cm) - Vòng sơ khảo và chung kết
    (@Round3Id, @Category2Id, N'Vòng sơ khảo Size 2', 1, 'preliminary', DATEADD(DAY, 30, @CurrentDate), DATEADD(DAY, 30, @CurrentDate), 10, 'upcoming', DATEADD(MONTH, -6, @CurrentDate), NULL),
    (@Round4Id, @Category2Id, N'Chung kết Size 2', 2, 'final', DATEADD(DAY, 30, @CurrentDate), DATEADD(DAY, 30, @CurrentDate), NULL, 'upcoming', DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    -- Junior (15-30cm) - Vòng sơ khảo, bán kết và chung kết
    (@Round6Id, @Category7Id, N'Vòng sơ khảo Junior', 1, 'preliminary', DATEADD(DAY, 60, @CurrentDate), DATEADD(DAY, 60, @CurrentDate), 15, 'upcoming', DATEADD(MONTH, -5, @CurrentDate), NULL),
    (@Round7Id, @Category7Id, N'Vòng bán kết Junior', 2, 'semifinal', DATEADD(DAY, 60, @CurrentDate), DATEADD(DAY, 60, @CurrentDate), 5, 'upcoming', DATEADD(MONTH, -5, @CurrentDate), NULL),
    (@Round8Id, @Category7Id, N'Chung kết Junior', 3, 'final', DATEADD(DAY, 60, @CurrentDate), DATEADD(DAY, 60, @CurrentDate), NULL, 'upcoming', DATEADD(MONTH, -5, @CurrentDate), NULL);

-- 9. TẠO DỮ LIỆU CHO CRITERIA (TIÊU CHÍ CHẤM ĐIỂM)
DECLARE @Criteria1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Criteria2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Criteria3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Criteria4Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Criteria5Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[Criteria]
([Id], [Name], [Description], [Order], [CreatedAt], [UpdatedAt])
VALUES
    (@Criteria1Id, N'Màu sắc', N'Độ sắc nét và tương phản của màu sắc', 1, DATEADD(MONTH, -12, @CurrentDate), NULL),
    (@Criteria2Id, N'Hình dạng', N'Dáng vẻ tổng thể của cá', 2, DATEADD(MONTH, -12, @CurrentDate), NULL),
    (@Criteria3Id, N'Mẫu mã', N'Kiểu mẫu và sự đối xứng', 3, DATEADD(MONTH, -12, @CurrentDate), NULL),
    (@Criteria4Id, N'Kích thước', N'Kích thước tương đối với độ tuổi', 4, DATEADD(MONTH, -12, @CurrentDate), NULL),
    (@Criteria5Id, N'Sức khỏe', N'Trạng thái tổng thể và hoạt động', 5, DATEADD(MONTH, -12, @CurrentDate), NULL);

-- 10. TẠO DỮ LIỆU CHO CRITERIACOMPETITIONCATEGORY (LIÊN KẾT TIÊU CHÍ VỚI DANH MỤC)
INSERT INTO [dbo].[CriteriaCompetitionCategory]
([Id], [CompetitionCategoryId], [CriteriaId], [RoundType], [Weight], [Order], [CreatedAt], [UpdatedAt])
VALUES
    -- Size 1 (15-25cm) - Tiêu chí vòng sơ khảo
    (NEWID(), @Category1Id, @Criteria1Id, 'preliminary', 0.30, 1, DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @Category1Id, @Criteria2Id, 'preliminary', 0.20, 2, DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @Category1Id, @Criteria3Id, 'preliminary', 0.25, 3, DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @Category1Id, @Criteria4Id, 'preliminary', 0.10, 4, DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @Category1Id, @Criteria5Id, 'preliminary', 0.15, 5, DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    -- Size 1 (15-25cm) - Tiêu chí vòng chung kết
    (NEWID(), @Category1Id, @Criteria1Id, 'final', 0.35, 1, DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @Category1Id, @Criteria2Id, 'final', 0.25, 2, DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @Category1Id, @Criteria3Id, 'final', 0.30, 3, DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @Category1Id, @Criteria5Id, 'final', 0.10, 4, DATEADD(MONTH, -6, @CurrentDate), NULL);

-- 11. TẠO DỮ LIỆU CHO ERRORTYPE (LOẠI LỖI)
DECLARE @Error1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Error2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Error3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Error4Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Error5Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[ErrorType]
([Id], [CriteriaId], [Name], [CreatedAt], [UpdatedAt])
VALUES
    (@Error1Id, @Criteria1Id, N'Màu không đều', DATEADD(MONTH, -12, @CurrentDate), NULL),
    (@Error2Id, @Criteria1Id, N'Màu nhạt', DATEADD(MONTH, -12, @CurrentDate), NULL),
    (@Error3Id, @Criteria2Id, N'Dáng không cân đối', DATEADD(MONTH, -12, @CurrentDate), NULL),
    (@Error4Id, @Criteria3Id, N'Mẫu không đối xứng', DATEADD(MONTH, -12, @CurrentDate), NULL),
    (@Error5Id, @Criteria5Id, N'Vẩy hư hại', DATEADD(MONTH, -12, @CurrentDate), NULL);

-- 12. TẠO DỮ LIỆU CHO REFEREEASSIGNMENT (PHÂN CÔNG GIÁM KHẢO)
INSERT INTO [dbo].[RefereeAssignment]
([Id], [CompetitionCategoryId], [RefereeAccountId], [RoundType], [AssignedAt], [AssignedBy])
VALUES
    -- Size 1 (15-25cm) - Phân công giám khảo
    (NEWID(), @Category1Id, @RefereeId1, 'preliminary', DATEADD(MONTH, -5, @CurrentDate), @ManagerId1),
    (NEWID(), @Category1Id, @RefereeId2, 'preliminary', DATEADD(MONTH, -5, @CurrentDate), @ManagerId1),
    (NEWID(), @Category1Id, @RefereeId3, 'preliminary', DATEADD(MONTH, -5, @CurrentDate), @ManagerId1),
    
    (NEWID(), @Category1Id, @RefereeId1, 'final', DATEADD(MONTH, -5, @CurrentDate), @ManagerId1),
    (NEWID(), @Category1Id, @RefereeId2, 'final', DATEADD(MONTH, -5, @CurrentDate), @ManagerId1),
    (NEWID(), @Category1Id, @RefereeId3, 'final', DATEADD(MONTH, -5, @CurrentDate), @ManagerId1);

-- 13. TẠO DỮ LIỆU CHO TANK (BỂ CÁ)
DECLARE @Tank1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Tank2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Tank3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Tank4Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Tank5Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[Tank]
([Id], [CompetitionCategoryId], [Name], [Capacity], [WaterType], [Temperature], [PHLevel], [Size], [Location], [Status], [CreatedBy], [CreatedAt], [UpdatedAt])
VALUES
    -- Size 1 (15-25cm) - Bể
    (@Tank1Id, @Category1Id, N'Tank S1-01', 5, N'Fresh', 24.50, 7.20, 100.00, N'Khu A - Hàng 1', 'active', @StaffId1, DATEADD(MONTH, -5, @CurrentDate), NULL),
    (@Tank2Id, @Category1Id, N'Tank S1-02', 5, N'Fresh', 24.50, 7.20, 100.00, N'Khu A - Hàng 1', 'active', @StaffId1, DATEADD(MONTH, -5, @CurrentDate), NULL),
    (@Tank3Id, @Category1Id, N'Tank S1-03', 5, N'Fresh', 24.50, 7.20, 100.00, N'Khu A - Hàng 1', 'active', @StaffId1, DATEADD(MONTH, -5, @CurrentDate), NULL),
    
    -- Size 2 (25-35cm) - Bể
    (@Tank4Id, @Category2Id, N'Tank S2-01', 4, N'Fresh', 24.50, 7.20, 120.00, N'Khu A - Hàng 2', 'active', @StaffId1, DATEADD(MONTH, -5, @CurrentDate), NULL),
    (@Tank5Id, @Category2Id, N'Tank S2-02', 4, N'Fresh', 24.50, 7.20, 120.00, N'Khu A - Hàng 2', 'active', @StaffId1, DATEADD(MONTH, -5, @CurrentDate), NULL);

-- 14. TẠO DỮ LIỆU CHO REGISTRATION (ĐĂNG KÝ)
DECLARE @Reg1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Reg2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Reg3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Reg4Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Reg5Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[Registration]
([Id], [KoiShowId], [KoiProfileId], [RegistrationNumber], [RegisterName], [KoiSize], [KoiAge], [CompetitionCategoryId], [AccountId], [RegistrationFee], [Rank], [Status], [QRCodeData], [Notes], [ApprovedAt], [CreatedAt], [UpdatedAt])
VALUES
    -- Đăng ký cho cuộc thi Koi Show Việt Nam 2025
    (@Reg1Id, @KoiShowHNId, @Koi1Id, 'VN2025-001', N'Ruby Dragon', 22.50, 2, @Category1Id, @UserId1, 500000.00, NULL, 'approved', 'REG-VN2025-001', NULL, DATEADD(DAY, -40, @CurrentDate), DATEADD(DAY, -45, @CurrentDate), DATEADD(DAY, -40, @CurrentDate)),
    (@Reg2Id, @KoiShowHNId, @Koi2Id, 'VN2025-002', N'Midnight Shadow', 38.20, 3, @Category3Id, @UserId1, 900000.00, NULL, 'approved', 'REG-VN2025-002', NULL, DATEADD(DAY, -40, @CurrentDate), DATEADD(DAY, -45, @CurrentDate), DATEADD(DAY, -40, @CurrentDate)),
    (@Reg3Id, @KoiShowHNId, @Koi4Id, 'VN2025-003', N'Blue Sapphire', 28.40, 2, @Category2Id, @UserId2, 700000.00, NULL, 'approved', 'REG-VN2025-003', NULL, DATEADD(DAY, -39, @CurrentDate), DATEADD(DAY, -44, @CurrentDate), DATEADD(DAY, -39, @CurrentDate)),
    (@Reg4Id, @KoiShowHNId, @Koi7Id, 'VN2025-004', N'Rainbow Jewel', 32.60, 2, @Category2Id, @UserId3, 700000.00, NULL, 'approved', 'REG-VN2025-004', NULL, DATEADD(DAY, -38, @CurrentDate), DATEADD(DAY, -43, @CurrentDate), DATEADD(DAY, -38, @CurrentDate)),
    (@Reg5Id, @KoiShowHNId, @Koi10Id, 'VN2025-005', N'Rising Sun', 36.40, 2, @Category3Id, @UserId5, 900000.00, NULL, 'approved', 'REG-VN2025-005', NULL, DATEADD(DAY, -37, @CurrentDate), DATEADD(DAY, -42, @CurrentDate), DATEADD(DAY, -37, @CurrentDate));

-- 15. TẠO DỮ LIỆU CHO REGISTRATIONROUND (ĐĂNG KÝ CHO VÒNG THI)
INSERT INTO [dbo].[RegistrationRound]
([Id], [RegistrationId], [RoundId], [CheckInTime], [CheckOutTime], [TankId], [Status], [Notes], [CreatedAt], [UpdatedAt])
VALUES
    -- Vòng sơ khảo Size 1
    (NEWID(), @Reg1Id, @Round1Id, NULL, NULL, @Tank1Id, 'pending', NULL, DATEADD(DAY, -37, @CurrentDate), NULL),
    -- Vòng sơ khảo Size 2
    (NEWID(), @Reg3Id, @Round3Id, NULL, NULL, @Tank4Id, 'pending', NULL, DATEADD(DAY, -36, @CurrentDate), NULL),
    (NEWID(), @Reg4Id, @Round3Id, NULL, NULL, @Tank5Id, 'pending', NULL, DATEADD(DAY, -36, @CurrentDate), NULL);

-- 16. TẠO DỮ LIỆU CHO AWARD (GIẢI THƯỞNG)
INSERT INTO [dbo].[Award]
([Id], [CompetitionCategoriesId], [Name], [AwardType], [PrizeValue], [Description], [CreatedAt], [UpdatedAt])
VALUES
    -- Giải thưởng cho Size 1 (15-25cm)
    (NEWID(), @Category1Id, N'Giải nhất Size 1', 'first', 5000000.00, N'Giải nhất cho hạng mục Size 1', DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @Category1Id, N'Giải nhì Size 1', 'second', 3000000.00, N'Giải nhì cho hạng mục Size 1', DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @Category1Id, N'Giải ba Size 1', 'third', 2000000.00, N'Giải ba cho hạng mục Size 1', DATEADD(MONTH, -6, @CurrentDate), NULL),
    
    -- Giải thưởng cho Size 2 (25-35cm)
    (NEWID(), @Category2Id, N'Giải nhất Size 2', 'first', 7000000.00, N'Giải nhất cho hạng mục Size 2', DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @Category2Id, N'Giải nhì Size 2', 'second', 4000000.00, N'Giải nhì cho hạng mục Size 2', DATEADD(MONTH, -6, @CurrentDate), NULL);

-- 17. TẠO DỮ LIỆU CHO REGISTRATIONPAYMENT (THANH TOÁN ĐĂNG KÝ)
INSERT INTO [dbo].[RegistrationPayment]
([Id], [RegistrationId], [QRCodeData], [PaidAmount], [PaymentDate], [TransactionCode], [PaymentMethod], [Status])
VALUES
    (NEWID(), @Reg1Id, 'PAY-VN2025-001', 500000.00, DATEADD(DAY, -42, @CurrentDate), 'TRX001', N'Banking', 'completed'),
    (NEWID(), @Reg2Id, 'PAY-VN2025-002', 900000.00, DATEADD(DAY, -41, @CurrentDate), 'TRX002', N'Banking', 'completed'),
    (NEWID(), @Reg3Id, 'PAY-VN2025-003', 700000.00, DATEADD(DAY, -40, @CurrentDate), 'TRX003', N'Banking', 'completed'),
    (NEWID(), @Reg4Id, 'PAY-VN2025-004', 700000.00, DATEADD(DAY, -39, @CurrentDate), 'TRX004', N'Banking', 'completed'),
    (NEWID(), @Reg5Id, 'PAY-VN2025-005', 900000.00, DATEADD(DAY, -38, @CurrentDate), 'TRX005', N'Banking', 'completed');

-- 18. TẠO DỮ LIỆU CHO KOIMEDIAS (HÌNH ẢNH CÁ)
INSERT INTO [dbo].[KoiMedia]
([Id], [MediaUrl], [MediaType], [KoiProfileId], [RegistrationId])
VALUES
    -- Hình ảnh cho KoiProfile
    (NEWID(), 'koimedia/koiprofile_ruby_1.jpg', 'Image', @Koi1Id, NULL),
    (NEWID(), 'koimedia/koiprofile_ruby_2.jpg', 'Image', @Koi1Id, NULL),
    (NEWID(), 'koimedia/koiprofile_midnight_1.jpg', 'Image', @Koi2Id, NULL),
    (NEWID(), 'koimedia/koiprofile_blue_1.jpg', 'Image', @Koi4Id, NULL),
    
    -- Hình ảnh cho Registration
    (NEWID(), 'koimedia/registration_ruby_1.jpg', 'Image', NULL, @Reg1Id),
    (NEWID(), 'koimedia/registration_midnight_1.jpg', 'Image', NULL, @Reg2Id),
    (NEWID(), 'koimedia/registration_blue_1.jpg', 'Image', NULL, @Reg3Id),
    (NEWID(), 'koimedia/registration_blue_video.mp4', 'Video', NULL, @Reg3Id);

-- 19. TẠO DỮ LIỆU CHO TICKETTYPE (LOẠI VÉ)
DECLARE @TicketType1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @TicketType2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @TicketType3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @TicketType4Id UNIQUEIDENTIFIER = NEWID();
DECLARE @TicketType5Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[TicketType]
([Id], [KoiShowId], [Name], [Price], [AvailableQuantity])
VALUES
    -- Vé cho Koi Show Việt Nam 2025
    (@TicketType1Id, @KoiShowHNId, N'Vé thường - 1 ngày', 150000.00, 500),
    (@TicketType2Id, @KoiShowHNId, N'Vé VIP - Trọn gói', 350000.00, 200),
    (@TicketType3Id, @KoiShowHNId, N'Vé gia đình (4 người)', 500000.00, 100),
    
    -- Vé cho Koi Show Hồ Chí Minh 2025
    (@TicketType4Id, @KoiShowHCMId, N'Vé thường - 1 ngày', 120000.00, 400),
    (@TicketType5Id, @KoiShowHCMId, N'Vé VIP - Trọn gói', 300000.00, 150);

-- 20. TẠO DỮ LIỆU CHO TICKETORDER (ĐƠN HÀNG VÉ)
DECLARE @TicketOrder1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @TicketOrder2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @TicketOrder3Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[TicketOrder]
([Id], [FullName], [Email], [AccountId], [OrderDate], [TransactionCode], [TotalAmount], [PaymentMethod], [Status], [Notes])
VALUES
    (@TicketOrder1Id, N'Trần Văn User', 'user1@example.com', @UserId1, DATEADD(DAY, -30, @CurrentDate), 'TKT-TRX001', 350000.00, 'Banking', 'completed', NULL),
    (@TicketOrder2Id, N'Lý Thị User', 'user2@example.com', @UserId2, DATEADD(DAY, -28, @CurrentDate), 'TKT-TRX002', 150000.00, 'Banking', 'completed', NULL),
    (@TicketOrder3Id, N'Phan Văn User', 'user3@example.com', @UserId3, DATEADD(DAY, -25, @CurrentDate), 'TKT-TRX003', 500000.00, 'Banking', 'completed', NULL);

-- 21. TẠO DỮ LIỆU CHO TICKETORDERDETAIL (CHI TIẾT ĐƠN HÀNG VÉ)
DECLARE @TicketOrderDetail1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @TicketOrderDetail2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @TicketOrderDetail3Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[TicketOrderDetail]
([Id], [TicketOrderId], [TicketTypeId], [Quantity], [UnitPrice])
VALUES
    (@TicketOrderDetail1Id, @TicketOrder1Id, @TicketType2Id, 1, 350000.00),
    (@TicketOrderDetail2Id, @TicketOrder2Id, @TicketType1Id, 1, 150000.00),
    (@TicketOrderDetail3Id, @TicketOrder3Id, @TicketType3Id, 1, 500000.00);

-- 22. TẠO DỮ LIỆU CHO TICKET (VÉ)
INSERT INTO [dbo].[Ticket]
([Id], [TicketOrderDetailId], [QRCodeData], [ExpiredDate], [IsCheckedIn], [CheckInTime], [CheckInLocation], [CheckedInBy], [Status])
VALUES
    -- Vé cho đơn hàng 1
    (NEWID(), @TicketOrderDetail1Id, 'TICKET-001', DATEADD(DAY, 32, @CurrentDate), 0, NULL, NULL, NULL, 'active'),
    
    -- Vé cho đơn hàng 2
    (NEWID(), @TicketOrderDetail2Id, 'TICKET-002', DATEADD(DAY, 30, @CurrentDate), 0, NULL, NULL, NULL, 'active'),
    
    -- Vé cho đơn hàng 3 (vé gia đình có 4 vé con)
    (NEWID(), @TicketOrderDetail3Id, 'TICKET-003-1', DATEADD(DAY, 32, @CurrentDate), 0, NULL, NULL, NULL, 'active'),
    (NEWID(), @TicketOrderDetail3Id, 'TICKET-003-2', DATEADD(DAY, 32, @CurrentDate), 0, NULL, NULL, NULL, 'active'),
    (NEWID(), @TicketOrderDetail3Id, 'TICKET-003-3', DATEADD(DAY, 32, @CurrentDate), 0, NULL, NULL, NULL, 'active'),
    (NEWID(), @TicketOrderDetail3Id, 'TICKET-003-4', DATEADD(DAY, 32, @CurrentDate), 0, NULL, NULL, NULL, 'active');

-- 23. TẠO DỮ LIỆU CHO BLOGCATEGORY (DANH MỤC BÀI VIẾT)
DECLARE @BlogCat1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @BlogCat2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @BlogCat3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @BlogCat4Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[BlogCategory]
([Id], [Name], [Description])
VALUES
    (@BlogCat1Id, N'Tin tức', N'Tin tức mới nhất về cá Koi và sự kiện'),
    (@BlogCat2Id, N'Hướng dẫn', N'Hướng dẫn chăm sóc và nuôi cá Koi'),
    (@BlogCat3Id, N'Sự kiện', N'Thông tin về các sự kiện Koi sắp tới'),
    (@BlogCat4Id, N'Giống cá', N'Thông tin về các giống cá Koi phổ biến');

-- 24. TẠO DỮ LIỆU CHO BLOGSNEWS (BÀI VIẾT)
INSERT INTO [dbo].[BlogsNews]
([Id], [Title], [Content], [BlogCategoryId], [AccountId], [ImgURL], [CreatedAt], [UpdatedAt])
VALUES
    (NEWID(), N'Chuẩn bị cho Koi Show Việt Nam 2025', N'<p>Cuộc thi Koi Show Việt Nam 2025 sẽ được tổ chức tại Hà Nội vào tháng 3 năm 2025. Đây là sự kiện lớn nhất trong năm dành cho những người yêu thích cá Koi ở Việt Nam.</p><p>Các tham dự viên sẽ có cơ hội trưng bày những con cá Koi đẹp nhất của mình và tham gia vào cuộc thi với nhiều hạng mục khác nhau.</p>', @BlogCat3Id, @ManagerId1, 'blog/blog1.jpg', DATEADD(MONTH, -3, @CurrentDate), NULL),
    (NEWID(), N'Cách chăm sóc cá Koi trong mùa đông', N'<p>Mùa đông là thời điểm khó khăn cho việc chăm sóc cá Koi. Nhiệt độ thấp có thể ảnh hưởng đến sức khỏe và sự phát triển của cá.</p><p>Bài viết này sẽ hướng dẫn bạn cách chăm sóc cá Koi trong mùa đông, từ việc điều chỉnh nhiệt độ nước đến chế độ ăn uống phù hợp.</p>', @BlogCat2Id, @ManagerId2, 'blog/blog2.jpg', DATEADD(MONTH, -2, @CurrentDate), NULL),
    (NEWID(), N'10 giống cá Koi phổ biến nhất tại Việt Nam', N'<p>Cá Koi có nhiều giống khác nhau, mỗi giống có đặc điểm và vẻ đẹp riêng. Tại Việt Nam, có một số giống cá Koi được ưa chuộng hơn cả.</p><p>Bài viết này sẽ giới thiệu 10 giống cá Koi phổ biến nhất tại Việt Nam, từ Kohaku, Sanke, Showa đến Asagi và Bekko.</p>', @BlogCat4Id, @ManagerId1, 'blog/blog3.jpg', DATEADD(MONTH, -1, @CurrentDate), NULL);

-- 25. TẠO DỮ LIỆU CHO NOTIFICATION (THÔNG BÁO)
INSERT INTO [dbo].[Notification]
([Id], [AccountId], [Content], [SentDate], [Title], [Type], [IsRead])
VALUES
    -- Thông báo cho User 1
    (NEWID(), @UserId1, N'Đăng ký của bạn cho cuộc thi Koi Show Việt Nam 2025 đã được chấp nhận!', DATEADD(DAY, -40, @CurrentDate), N'Đăng ký thành công', 'registration', 1),
    (NEWID(), @UserId1, N'Thanh toán của bạn đã được xác nhận. Cảm ơn bạn đã đăng ký tham gia!', DATEADD(DAY, -39, @CurrentDate), N'Xác nhận thanh toán', 'payment', 1),
    
    -- Thông báo cho User 2
    (NEWID(), @UserId2, N'Đăng ký của bạn cho cuộc thi Koi Show Việt Nam 2025 đã được chấp nhận!', DATEADD(DAY, -38, @CurrentDate), N'Đăng ký thành công', 'registration', 1),
    
    -- Thông báo chung cho tất cả người dùng
    (NEWID(), NULL, N'Koi Show Việt Nam 2025 sẽ diễn ra từ ngày 05/04/2025 đến 07/04/2025 tại Hà Nội. Hãy mua vé sớm để không bỏ lỡ!', DATEADD(DAY, -35, @CurrentDate), N'Thông báo sự kiện', 'event', 0);

-- 26. TẠO DỮ LIỆU CHO FEEDBACK (PHẢN HỒI)
INSERT INTO [dbo].[Feedback]
([Id], [AccountId], [KoiShowId], [Content], [CreatedAt], [UpdatedAt])
VALUES
    -- Phản hồi về Koi Show Mekong 2024 đã diễn ra
    (NEWID(), @UserId1, @KoiShowMekongId, N'Sự kiện được tổ chức rất chuyên nghiệp. Tôi rất ấn tượng với chất lượng cá tham gia và cách sắp xếp các khu vực trưng bày.', DATEADD(MONTH, -2, @CurrentDate), NULL),
    (NEWID(), @UserId2, @KoiShowMekongId, N'Hệ thống đăng ký và chấm điểm rất tốt. Tuy nhiên, tôi nghĩ nên có thêm nhiều giải thưởng cho các hạng mục nhỏ hơn.', DATEADD(MONTH, -2, @CurrentDate), NULL);

-- 27. TẠO DỮ LIỆU CHO SPONSOR (NHÀ TÀI TRỢ)
INSERT INTO [dbo].[Sponsor]
([Id], [Name], [LogoUrl], [InvestMoney], [KoiShowId])
VALUES
    -- Nhà tài trợ cho Koi Show Việt Nam 2025
    (NEWID(), N'Công ty TNHH Thức ăn cá ABC', 'sponsors/abc_logo.png', 50000000.00, @KoiShowHNId),
    (NEWID(), N'Tập đoàn Thiết bị hồ cá XYZ', 'sponsors/xyz_logo.png', 30000000.00, @KoiShowHNId),
    (NEWID(), N'Chuỗi cửa hàng Koi Farm', 'sponsors/koifarm_logo.png', 20000000.00, @KoiShowHNId),
    
    -- Nhà tài trợ cho Koi Show Hồ Chí Minh 2025
    (NEWID(), N'Công ty CP Thủy sản Việt', 'sponsors/vietfish_logo.png', 40000000.00, @KoiShowHCMId),
    (NEWID(), N'Nhà máy Lọc nước Aqua Pro', 'sponsors/aquapro_logo.png', 25000000.00, @KoiShowHCMId);

-- 28. TẠO DỮ LIỆU CHO SHOWSTATUS (TRẠNG THÁI CUỘC THI)
INSERT INTO [dbo].[ShowStatus]
([Id], [KoiShowId], [StatusName], [Description], [StartDate], [EndDate], [IsActive])
VALUES
    -- Trạng thái cho Koi Show Việt Nam 2025
    (NEWID(), @KoiShowHNId, N'Đăng ký', N'Giai đoạn đăng ký tham gia', DATEADD(DAY, -60, @CurrentDate), DATEADD(DAY, 25, @CurrentDate), 1),
    (NEWID(), @KoiShowHNId, N'Triển lãm & Thi đấu', N'Giai đoạn trưng bày và thi đấu', DATEADD(DAY, 30, @CurrentDate), DATEADD(DAY, 32, @CurrentDate), 0),
    (NEWID(), @KoiShowHNId, N'Kết thúc', N'Cuộc thi đã kết thúc', DATEADD(DAY, 32, @CurrentDate), DATEADD(DAY, 33, @CurrentDate), 0);

-- 29. TẠO DỮ LIỆU CHO LIVESTREAM (PHÁT TRỰC TIẾP)
INSERT INTO [dbo].[Livestream]
([Id], [KoiShowId], [StartTime], [EndTime], [StreamUrl])
VALUES
    -- Lịch phát trực tiếp cho Koi Show Việt Nam 2025
    (NEWID(), @KoiShowHNId, DATEADD(DAY, 30, @CurrentDate), DATEADD(DAY, 30, @CurrentDate), 'https://livestream.koishow.vn/vn2025-day1'),
    (NEWID(), @KoiShowHNId, DATEADD(DAY, 31, @CurrentDate), DATEADD(DAY, 31, @CurrentDate), 'https://livestream.koishow.vn/vn2025-day2'),
    (NEWID(), @KoiShowHNId, DATEADD(DAY, 32, @CurrentDate), DATEADD(DAY, 32, @CurrentDate), 'https://livestream.koishow.vn/vn2025-day3');

-- 30. TẠO DỮ LIỆU CHO SHOWRULE (QUY TẮC CUỘC THI)
INSERT INTO [dbo].[ShowRule]
([Id], [KoiShowId], [Title], [Content], [CreatedAt], [UpdatedAt])
VALUES
    -- Quy tắc cho Koi Show Việt Nam 2025
    (NEWID(), @KoiShowHNId, N'Quy tắc đăng ký', N'<p>1. Mỗi người tham gia có thể đăng ký tối đa 5 con cá cho mỗi hạng mục.</p><p>2. Cá phải được đăng ký đúng hạng mục theo kích thước.</p><p>3. Phí đăng ký phải được thanh toán trong vòng 3 ngày sau khi đăng ký.</p>', DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @KoiShowHNId, N'Quy tắc chấm điểm', N'<p>1. Mỗi con cá sẽ được chấm điểm bởi ít nhất 3 giám khảo.</p><p>2. Điểm số cuối cùng là trung bình cộng của tất cả các giám khảo.</p><p>3. Quyết định của ban tổ chức là quyết định cuối cùng.</p>', DATEADD(MONTH, -6, @CurrentDate), NULL),
    (NEWID(), @KoiShowHNId, N'Quy tắc an toàn', N'<p>1. Cá phải được kiểm tra sức khỏe trước khi tham gia.</p><p>2. Ban tổ chức không chịu trách nhiệm nếu cá bị bệnh hoặc chết trong quá trình thi đấu.</p><p>3. Chủ sở hữu phải tuân theo hướng dẫn của nhân viên kỹ thuật về việc vận chuyển và bảo quản cá.</p>', DATEADD(MONTH, -6, @CurrentDate), NULL);

-- 31. TẠO DỮ LIỆU CHO VOTE (BÌNH CHỌN)
INSERT INTO [dbo].[Vote]
([Id], [AccountId], [RegistrationId], [Prediction], [CreatedAt], [UpdatedAt])
VALUES
    -- Bình chọn cho các cá đăng ký
    (NEWID(), @UserId3, @Reg1Id, N'Tôi nghĩ con cá này sẽ giành giải nhất trong hạng mục của nó!', DATEADD(DAY, -30, @CurrentDate), NULL),
    (NEWID(), @UserId4, @Reg1Id, N'Màu sắc rất đẹp, nhưng hình dáng còn hơi thiếu cân đối.', DATEADD(DAY, -29, @CurrentDate), NULL),
    (NEWID(), @UserId5, @Reg3Id, N'Đây là con cá Koi đẹp nhất mà tôi từng thấy trong hạng mục này!', DATEADD(DAY, -28, @CurrentDate), NULL);