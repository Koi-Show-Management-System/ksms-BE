using System;
using System.Collections.Generic;
using KSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KSMS.Infrastructure.Database;

public partial class KoiShowManagementSystemContext : DbContext
{
    public KoiShowManagementSystemContext()
    {
    }

    public KoiShowManagementSystemContext(DbContextOptions<KoiShowManagementSystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Award> Awards { get; set; }

    public virtual DbSet<BlogCategory> BlogCategories { get; set; }

    public virtual DbSet<BlogsNews> BlogsNews { get; set; }

    public virtual DbSet<CategoryVariety> CategoryVarieties { get; set; }

    public virtual DbSet<CheckOutLog> CheckOutLogs { get; set; }

    public virtual DbSet<CompetitionCategory> CompetitionCategories { get; set; }

    public virtual DbSet<CriteriaCompetitionCategory> CriteriaCompetitionCategories { get; set; }

    public virtual DbSet<Criterion> Criteria { get; set; }

    public virtual DbSet<ErrorType> ErrorTypes { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<KoiMedium> KoiMedia { get; set; }

    public virtual DbSet<KoiProfile> KoiProfiles { get; set; }

    public virtual DbSet<KoiShow> KoiShows { get; set; }

    public virtual DbSet<Livestream> Livestreams { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<RefereeAssignment> RefereeAssignments { get; set; }

    public virtual DbSet<Registration> Registrations { get; set; }

    public virtual DbSet<RegistrationPayment> RegistrationPayments { get; set; }

    public virtual DbSet<RegistrationRound> RegistrationRounds { get; set; }

    public virtual DbSet<Round> Rounds { get; set; }

    public virtual DbSet<RoundResult> RoundResults { get; set; }

    public virtual DbSet<ScoreDetail> ScoreDetails { get; set; }

    public virtual DbSet<ScoreDetailError> ScoreDetailErrors { get; set; }

    public virtual DbSet<ShowRule> ShowRules { get; set; }

    public virtual DbSet<ShowStaff> ShowStaffs { get; set; }

    public virtual DbSet<ShowStatus> ShowStatuses { get; set; }

    public virtual DbSet<Sponsor> Sponsors { get; set; }

    public virtual DbSet<Tank> Tanks { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TicketOrder> TicketOrders { get; set; }

    public virtual DbSet<TicketOrderDetail> TicketOrderDetails { get; set; }

    public virtual DbSet<TicketType> TicketTypes { get; set; }

    public virtual DbSet<Variety> Varieties { get; set; }

    public virtual DbSet<Vote> Votes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Account__3214EC072FF5396B");

            entity.ToTable("Account");

            entity.HasIndex(e => e.Username, "UQ__Account__536C85E47296FF1C").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Account__A9D10534D2D60135").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Avatar).HasMaxLength(255);
            entity.Property(e => e.ConfirmationToken).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.HashedPassword)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.IsConfirmed)
                .IsRequired()
                .HasDefaultValueSql("('0')");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ResetPasswordOtp)
                .HasMaxLength(10)
                .HasColumnName("ResetPasswordOTP");
            entity.Property(e => e.ResetPasswordOtpexpiry)
                .HasColumnType("datetime")
                .HasColumnName("ResetPasswordOTPExpiry");
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("active");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Award>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Award__3214EC07DB05D0D3");

            entity.ToTable("Award");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AwardType).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.PrizeValue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CompetitionCategories).WithMany(p => p.Awards)
                .HasForeignKey(d => d.CompetitionCategoriesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Award__Competiti__4F7CD00D");
        });

        modelBuilder.Entity<BlogCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BlogCate__3214EC07A9F230DC");

            entity.ToTable("BlogCategory");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<BlogsNews>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BlogsNew__3214EC07B48AD77F");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ImgUrl)
                .HasMaxLength(255)
                .HasColumnName("ImgURL");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.BlogsNews)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BlogsNews__Accou__367C1819");

            entity.HasOne(d => d.BlogCategory).WithMany(p => p.BlogsNews)
                .HasForeignKey(d => d.BlogCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BlogsNews__BlogC__37703C52");
        });

        modelBuilder.Entity<CategoryVariety>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3214EC07F6BEC2DA");

            entity.ToTable("CategoryVariety");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.CompetitionCategory).WithMany(p => p.CategoryVarieties)
                .HasForeignKey(d => d.CompetitionCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CategoryV__Compe__3F466844");

            entity.HasOne(d => d.Variety).WithMany(p => p.CategoryVarieties)
                .HasForeignKey(d => d.VarietyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CategoryV__Varie__3E52440B");
        });

        modelBuilder.Entity<CheckOutLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CheckOut__3214EC0773A54AA0");

            entity.ToTable("CheckOutLog");

            entity.HasIndex(e => e.RegistrationId, "UQ__CheckOut__6EF58811E93FC69C").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CheckOutTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImgCheckOut).HasMaxLength(255);
            entity.Property(e => e.Notes).HasMaxLength(255);

            entity.HasOne(d => d.CheckedOutByNavigation).WithMany(p => p.CheckOutLogs)
                .HasForeignKey(d => d.CheckedOutBy)
                .HasConstraintName("FK__CheckOutL__Check__44CA3770");

            entity.HasOne(d => d.Registration).WithOne(p => p.CheckOutLog)
                .HasForeignKey<CheckOutLog>(d => d.RegistrationId)
                .HasConstraintName("FK__CheckOutL__Regis__43D61337");
        });

        modelBuilder.Entity<CompetitionCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Competit__3214EC076273B726");

            entity.ToTable("CompetitionCategory");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.RegistrationFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.HasTank)
                .IsRequired()
                .HasDefaultValue(false);
            entity.Property(e => e.SizeMax).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.SizeMin).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.KoiShow).WithMany(p => p.CompetitionCategories)
                .HasForeignKey(d => d.KoiShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Competiti__Updat__3A81B327");
        });

        modelBuilder.Entity<CriteriaCompetitionCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Criteria__3214EC07EF8E67C1");

            entity.ToTable("CriteriaCompetitionCategory");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.RoundType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.Weight).HasColumnType("decimal(3, 2)");

            entity.HasOne(d => d.CompetitionCategory).WithMany(p => p.CriteriaCompetitionCategories)
                .HasForeignKey(d => d.CompetitionCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CriteriaC__Compe__59FA5E80");

            entity.HasOne(d => d.Criteria).WithMany(p => p.CriteriaCompetitionCategories)
                .HasForeignKey(d => d.CriteriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CriteriaC__Crite__5AEE82B9");
        });

        modelBuilder.Entity<Criterion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Criteria__3214EC07AEC9FEE4");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<ErrorType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ErrorTyp__3214EC070559F39E");

            entity.ToTable("ErrorType");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Criteria).WithMany(p => p.ErrorTypes)
                .HasForeignKey(d => d.CriteriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ErrorType__Crite__778AC167");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Feedback__3214EC075C0344A8");

            entity.ToTable("Feedback");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedback__Accoun__1DB06A4F");

            entity.HasOne(d => d.KoiShow).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.KoiShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedback__KoiSho__1EA48E88");
        });

        modelBuilder.Entity<KoiMedium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__KoiMedia__3214EC07BE2032DE");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.MediaType).HasMaxLength(10);

            entity.HasOne(d => d.KoiProfile).WithMany(p => p.KoiMedia)
                .HasForeignKey(d => d.KoiProfileId)
                .HasConstraintName("FK__KoiMedia__KoiPro__4C6B5938");

            entity.HasOne(d => d.Registration).WithMany(p => p.KoiMedia)
                .HasForeignKey(d => d.RegistrationId)
                .HasConstraintName("FK__KoiMedia__Regist__4D5F7D71");
        });

        modelBuilder.Entity<KoiProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__KoiProfi__3214EC07338B74C9");

            entity.ToTable("KoiProfile");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Bloodline).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.IsPublic).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Size).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Owner).WithMany(p => p.KoiProfiles)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KoiProfil__Owner__4AB81AF0");

            entity.HasOne(d => d.Variety).WithMany(p => p.KoiProfiles)
                .HasForeignKey(d => d.VarietyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KoiProfil__Varie__4BAC3F29");
        });

        modelBuilder.Entity<KoiShow>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__KoiShow__3214EC071877E35F");

            entity.ToTable("KoiShow");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.EndExhibitionDate).HasColumnType("datetime");
            entity.Property(e => e.EnableVoting)
                .IsRequired()
                .HasDefaultValue(false);
            entity.Property(e => e.HasBestInShow).HasDefaultValue(false);
            entity.Property(e => e.HasGrandChampion).HasDefaultValue(false);
            entity.Property(e => e.ImgUrl)
                .HasMaxLength(255)
                .HasColumnName("ImgURL");
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.StartExhibitionDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Livestream>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Livestre__3214EC0775801D20");

            entity.ToTable("Livestream");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.StreamUrl).HasMaxLength(500);

            entity.HasOne(d => d.KoiShow).WithMany(p => p.Livestreams)
                .HasForeignKey(d => d.KoiShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Livestrea__KoiSh__2B0A656D");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3214EC07D6739686");

            entity.ToTable("Notification");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.Type)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            
            entity.Property(e => e.IsRead)
                .IsRequired()
                .HasDefaultValue(false);
            
            entity.Property(e => e.SentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK__Notificat__Accou__19DFD96B");
        });

        modelBuilder.Entity<RefereeAssignment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RefereeA__3214EC0797CC40FF");

            entity.ToTable("RefereeAssignment");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RoundType)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.RefereeAssignmentAssignedByNavigations)
                .HasForeignKey(d => d.AssignedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RefereeAs__Assig__44FF419A");

            entity.HasOne(d => d.CompetitionCategory).WithMany(p => p.RefereeAssignments)
                .HasForeignKey(d => d.CompetitionCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RefereeAs__Compe__45F365D3");

            entity.HasOne(d => d.RefereeAccount).WithMany(p => p.RefereeAssignmentRefereeAccounts)
                .HasForeignKey(d => d.RefereeAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RefereeAs__Refer__440B1D61");
        });

        modelBuilder.Entity<Registration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Registra__3214EC07CFA8E495");

            entity.ToTable("Registration");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ApprovedAt).HasColumnType("datetime");
            entity.Property(e => e.CheckInExpiredDate).HasColumnType("datetime");
            entity.Property(e => e.CheckInLocation).HasMaxLength(100);
            entity.Property(e => e.CheckInTime).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.IsCheckedIn)
                .IsRequired()
                .HasDefaultValueSql("('0')");
            entity.Property(e => e.KoiSize).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.QrcodeData)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("QRCodeData");
            entity.Property(e => e.RegisterName).HasMaxLength(100);
            entity.Property(e => e.RegistrationFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RegistrationNumber).HasMaxLength(100);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.RefundType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.RegistrationAccounts)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Registrat__Accou__5FB337D6");

            entity.HasOne(d => d.CheckedInByNavigation).WithMany(p => p.RegistrationCheckedInByNavigations)
                .HasForeignKey(d => d.CheckedInBy)
                .HasConstraintName("FK__Registrat__Check__6383C8BA");

            entity.HasOne(d => d.CompetitionCategory).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.CompetitionCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Registrat__Compe__628FA481");

            entity.HasOne(d => d.KoiProfile).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.KoiProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Registrat__KoiPr__619B8048");

            entity.HasOne(d => d.KoiShow).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.KoiShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Registrat__KoiSh__60A75C0F");
        });

        modelBuilder.Entity<RegistrationPayment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Registra__3214EC07FA3FAA1A");

            entity.ToTable("RegistrationPayment");

            entity.HasIndex(e => e.RegistrationId, "UQ__Registra__6EF588116CDF8401").IsUnique();

            entity.HasIndex(e => e.TransactionCode, "UQ__Registra__D85E7026F92481A2").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.PaidAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.QrcodeData)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("QRCodeData");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TransactionCode).HasMaxLength(50);

            entity.HasOne(d => d.Registration).WithOne(p => p.RegistrationPayment)
                .HasForeignKey<RegistrationPayment>(d => d.RegistrationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Registrat__Regis__3E1D39E1");
        });

        modelBuilder.Entity<RegistrationRound>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Registra__3214EC07B0377112");

            entity.ToTable("RegistrationRound");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CheckInTime).HasColumnType("datetime");
            entity.Property(e => e.CheckOutTime).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Rank);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Registration).WithMany(p => p.RegistrationRounds)
                .HasForeignKey(d => d.RegistrationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Registrat__Regis__6C190EBB");

            entity.HasOne(d => d.Round).WithMany(p => p.RegistrationRounds)
                .HasForeignKey(d => d.RoundId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Registrat__Round__6D0D32F4");

            entity.HasOne(d => d.Tank).WithMany(p => p.RegistrationRounds)
                .HasForeignKey(d => d.TankId)
                .HasConstraintName("FK__Registrat__TankI__6E01572D");
        });

        modelBuilder.Entity<Round>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Round__3214EC075365BB0F");

            entity.ToTable("Round");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.RoundType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CompetitionCategories).WithMany(p => p.Rounds)
                .HasForeignKey(d => d.CompetitionCategoriesId)
                .HasConstraintName("FK__Round__Competiti__534D60F1");
        });

        modelBuilder.Entity<RoundResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RoundRes__3214EC07B2DA2A62");

            entity.ToTable("RoundResult");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.IsPublic).HasDefaultValue(false);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.TotalScore).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.RegistrationRounds).WithMany(p => p.RoundResults)
                .HasForeignKey(d => d.RegistrationRoundsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RoundResu__Regis__01142BA1");
        });

        modelBuilder.Entity<ScoreDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ScoreDet__3214EC07A30A38BA");

            entity.ToTable("ScoreDetail");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.InitialScore).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.IsPublic).HasDefaultValue(false);
            entity.Property(e => e.TotalPointMinus).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.RefereeAccount).WithMany(p => p.ScoreDetails)
                .HasForeignKey(d => d.RefereeAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ScoreDeta__Refer__73BA3083");

            entity.HasOne(d => d.RegistrationRound).WithMany(p => p.ScoreDetails)
                .HasForeignKey(d => d.RegistrationRoundId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ScoreDeta__Regis__72C60C4A");
        });

        modelBuilder.Entity<ScoreDetailError>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ScoreDet__3214EC07ABB01A62");

            entity.ToTable("ScoreDetailError");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.PointMinus).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Severity).HasMaxLength(20);
            entity.Property(e => e.Weight).HasColumnType("decimal(3, 2)");

            entity.HasOne(d => d.ErrorType).WithMany(p => p.ScoreDetailErrors)
                .HasForeignKey(d => d.ErrorTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ScoreDeta__Error__7C4F7684");

            entity.HasOne(d => d.ScoreDetail).WithMany(p => p.ScoreDetailErrors)
                .HasForeignKey(d => d.ScoreDetailId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ScoreDeta__Score__7B5B524B");
        });

        modelBuilder.Entity<ShowRule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ShowRule__3214EC0737B5208D");

            entity.ToTable("ShowRule");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.KoiShow).WithMany(p => p.ShowRules)
                .HasForeignKey(d => d.KoiShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ShowRule__KoiSho__489AC854");
        });

        modelBuilder.Entity<ShowStaff>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ShowStaf__3214EC072A3D8805");

            entity.ToTable("ShowStaff");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.ShowStaffAccounts)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ShowStaff__Accou__35BCFE0A");

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.ShowStaffAssignedByNavigations)
                .HasForeignKey(d => d.AssignedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ShowStaff__Assig__36B12243");

            entity.HasOne(d => d.KoiShow).WithMany(p => p.ShowStaffs)
                .HasForeignKey(d => d.KoiShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ShowStaff__KoiSh__34C8D9D1");
        });

        modelBuilder.Entity<ShowStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ShowStat__3214EC0701A70B36");

            entity.ToTable("ShowStatus");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.StatusName).HasMaxLength(50);

            entity.HasOne(d => d.KoiShow).WithMany(p => p.ShowStatuses)
                .HasForeignKey(d => d.KoiShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ShowStatu__KoiSh__2739D489");
        });

        modelBuilder.Entity<Sponsor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Sponsor__3214EC0793F77809");

            entity.ToTable("Sponsor");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.InvestMoney).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.KoiShow).WithMany(p => p.Sponsors)
                .HasForeignKey(d => d.KoiShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Sponsor__KoiShow__22751F6C");
        });

        modelBuilder.Entity<Tank>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tank__3214EC07AB944F16");

            entity.ToTable("Tank");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Phlevel)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("PHLevel");
            entity.Property(e => e.Size).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Temperature).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.WaterType).HasMaxLength(50);

            entity.HasOne(d => d.CompetitionCategory).WithMany(p => p.Tanks)
                .HasForeignKey(d => d.CompetitionCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tank__Competitio__6754599E");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Tanks)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tank__CreatedBy__68487DD7");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Ticket__3214EC07BE9CB4EE");

            entity.ToTable("Ticket");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CheckInLocation).HasMaxLength(100);
            entity.Property(e => e.CheckInTime).HasColumnType("datetime");
            entity.Property(e => e.ExpiredDate).HasColumnType("datetime");
            entity.Property(e => e.IsCheckedIn)
                .IsRequired()
                .HasDefaultValueSql("('0')");
            entity.Property(e => e.QrcodeData)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("QRCodeData");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.CheckedInByNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.CheckedInBy)
                .HasConstraintName("FK__Ticket__CheckedI__151B244E");

            entity.HasOne(d => d.TicketOrderDetail).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.TicketOrderDetailId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ticket__TicketOr__14270015");
        });

        modelBuilder.Entity<TicketOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TicketOr__3214EC07472DA5DD");

            entity.ToTable("TicketOrder");

            entity.HasIndex(e => e.TransactionCode, "UQ__TicketOr__D85E70266DDA0F9F").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TransactionCode).HasMaxLength(50);

            entity.HasOne(d => d.Account).WithMany(p => p.TicketOrders)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TicketOrd__Accou__0A9D95DB");
        });

        modelBuilder.Entity<TicketOrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TicketOr__3214EC07F29F1962");

            entity.ToTable("TicketOrderDetail");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.TicketOrder).WithMany(p => p.TicketOrderDetails)
                .HasForeignKey(d => d.TicketOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TicketOrd__Ticke__0E6E26BF");

            entity.HasOne(d => d.TicketType).WithMany(p => p.TicketOrderDetails)
                .HasForeignKey(d => d.TicketTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TicketOrd__Ticke__0F624AF8");
        });

        modelBuilder.Entity<TicketType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TicketTy__3214EC07A2EE1EF8");

            entity.ToTable("TicketType");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.KoiShow).WithMany(p => p.TicketTypes)
                .HasForeignKey(d => d.KoiShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TicketTyp__KoiSh__04E4BC85");
        });

        modelBuilder.Entity<Variety>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Variety__3214EC07A475F8CF");

            entity.ToTable("Variety");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Vote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Vote__3214EC0789ACCAEA");

            entity.ToTable("Vote");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.Votes)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Vote__AccountId__2EDAF651");

            entity.HasOne(d => d.Registration).WithMany(p => p.Votes)
                .HasForeignKey(d => d.RegistrationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Vote__Registrati__2FCF1A8A");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
