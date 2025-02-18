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

    public virtual DbSet<CheckInLog> CheckInLogs { get; set; }

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

    public virtual DbSet<PaymentType> PaymentTypes { get; set; }

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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(local);database=KoiShowManagementSystem;uid=sa;pwd=12345;TrustServerCertificate=True;Command Timeout=300;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Accounts__3214EC076F4F1AB7");

            entity.HasIndex(e => e.Username, "UQ__Accounts__536C85E43A7CFDC3").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Accounts__A9D105340FB7E3BA").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__Awards__3214EC07DB027F64");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AwardType).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.PrizeValue).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CompetitionCategories).WithMany(p => p.Awards)
                .HasForeignKey(d => d.CompetitionCategoriesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Awards__Competit__4F7CD00D");
        });

        modelBuilder.Entity<BlogCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BlogCate__3214EC07E16DCAD9");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<BlogsNews>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BlogsNew__3214EC078BD5376C");

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
                .HasConstraintName("FK__BlogsNews__Accou__3587F3E0");

            entity.HasOne(d => d.BlogCategory).WithMany(p => p.BlogsNews)
                .HasForeignKey(d => d.BlogCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BlogsNews__BlogC__367C1819");
        });

        modelBuilder.Entity<CategoryVariety>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3214EC075598F202");

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

        modelBuilder.Entity<CheckInLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CheckInL__3214EC07F176A18B");

            entity.HasIndex(e => e.TicketId, "UQ__CheckInL__712CC60671FD1AD1").IsUnique();

            entity.HasIndex(e => e.RegistrationPaymentId, "UQ__CheckInL__8285E2C4FF5E1BCB").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CheckInLocation).HasMaxLength(100);
            entity.Property(e => e.CheckInTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Notes).HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.CheckedInByNavigation).WithMany(p => p.CheckInLogs)
                .HasForeignKey(d => d.CheckedInBy)
                .HasConstraintName("FK__CheckInLo__Check__45BE5BA9");

            entity.HasOne(d => d.RegistrationPayment).WithOne(p => p.CheckInLog)
                .HasForeignKey<CheckInLog>(d => d.RegistrationPaymentId)
                .HasConstraintName("FK__CheckInLo__Regis__44CA3770");

            entity.HasOne(d => d.Ticket).WithOne(p => p.CheckInLog)
                .HasForeignKey<CheckInLog>(d => d.TicketId)
                .HasConstraintName("FK__CheckInLo__Ticke__43D61337");
        });

        modelBuilder.Entity<CheckOutLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CheckOut__3214EC0726984CFF");

            entity.HasIndex(e => e.RegistrationId, "UQ__CheckOut__6EF58811CB9CA704").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CheckOutTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImgCheckOut).HasMaxLength(255);
            entity.Property(e => e.Notes).HasMaxLength(255);

            entity.HasOne(d => d.CheckedOutByNavigation).WithMany(p => p.CheckOutLogs)
                .HasForeignKey(d => d.CheckedOutBy)
                .HasConstraintName("FK__CheckOutL__Check__4D5F7D71");

            entity.HasOne(d => d.Registration).WithOne(p => p.CheckOutLog)
                .HasForeignKey<CheckOutLog>(d => d.RegistrationId)
                .HasConstraintName("FK__CheckOutL__Regis__4C6B5938");
        });

        modelBuilder.Entity<CompetitionCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Competit__3214EC070CD8EA6D");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(100);
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
            entity.HasKey(e => e.Id).HasName("PK__Criteria__3214EC07AA16B9D3");

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
                .HasConstraintName("FK__CriteriaC__Crite__607251E5");
        });

        modelBuilder.Entity<Criterion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Criteria__3214EC0772BCCEEB");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<ErrorType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ErrorTyp__3214EC07EA072CF4");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Criteria).WithMany(p => p.ErrorTypes)
                .HasForeignKey(d => d.CriteriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ErrorType__Crite__74AE54BC");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Feedback__3214EC0736F164A9");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedbacks__Accou__1CBC4616");

            entity.HasOne(d => d.KoiShow).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.KoiShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedbacks__KoiSh__1DB06A4F");
        });

        modelBuilder.Entity<KoiMedium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__KoiMedia__3214EC07B1891B52");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.MediaType).HasMaxLength(10);

            entity.HasOne(d => d.KoiProfile).WithMany(p => p.KoiMedia)
                .HasForeignKey(d => d.KoiProfileId)
                .HasConstraintName("FK__KoiMedia__KoiPro__55009F39");

            entity.HasOne(d => d.Registration).WithMany(p => p.KoiMedia)
                .HasForeignKey(d => d.RegistrationId)
                .HasConstraintName("FK__KoiMedia__Regist__55F4C372");
        });

        modelBuilder.Entity<KoiProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__KoiProfi__3214EC07DBC2D315");

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
            entity.HasKey(e => e.Id).HasName("PK__KoiShows__3214EC0750C5F916");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.EndExhibitionDate).HasColumnType("datetime");
            entity.Property(e => e.HasBestInShow).HasDefaultValue(false);
            entity.Property(e => e.HasGrandChampion).HasDefaultValue(false);
            entity.Property(e => e.ImgUrl)
                .HasMaxLength(255)
                .HasColumnName("ImgURL");
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.RegistrationFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.StartExhibitionDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Livestream>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Livestre__3214EC07986701AC");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.StreamUrl).HasMaxLength(500);

            entity.HasOne(d => d.KoiShow).WithMany(p => p.Livestreams)
                .HasForeignKey(d => d.KoiShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Livestrea__KoiSh__2A164134");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3214EC0745164CDB");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.SentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK__Notificat__Accou__18EBB532");
        });

        modelBuilder.Entity<PaymentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PaymentT__3214EC0740588061");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<RefereeAssignment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RefereeA__3214EC076AA4ABF0");

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
            entity.HasKey(e => e.Id).HasName("PK__Registra__3214EC07E9E34336");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ApprovedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
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
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Registrat__Accou__5DCAEF64");

            entity.HasOne(d => d.CompetitionCategory).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.CompetitionCategoryId)
                .HasConstraintName("FK__Registrat__Compe__60A75C0F");

            entity.HasOne(d => d.KoiProfile).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.KoiProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Registrat__KoiPr__5FB337D6");

            entity.HasOne(d => d.KoiShow).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.KoiShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Registrat__KoiSh__5EBF139D");
        });

        modelBuilder.Entity<RegistrationPayment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Registra__3214EC07F9A506D8");

            entity.HasIndex(e => e.RegistrationId, "UQ__Registra__6EF58811216D4C71").IsUnique();

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

            entity.HasOne(d => d.PaymentType).WithMany(p => p.RegistrationPayments)
                .HasForeignKey(d => d.PaymentTypeId)
                .HasConstraintName("FK__Registrat__Payme__3D2915A8");

            entity.HasOne(d => d.Registration).WithOne(p => p.RegistrationPayment)
                .HasForeignKey<RegistrationPayment>(d => d.RegistrationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Registrat__Regis__3C34F16F");
        });

        modelBuilder.Entity<RegistrationRound>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Registra__3214EC07EB585968");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CheckInTime).HasColumnType("datetime");
            entity.Property(e => e.CheckOutTime).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Registration).WithMany(p => p.RegistrationRounds)
                .HasForeignKey(d => d.RegistrationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Registrat__Regis__693CA210");

            entity.HasOne(d => d.Round).WithMany(p => p.RegistrationRounds)
                .HasForeignKey(d => d.RoundId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Registrat__Round__6A30C649");

            entity.HasOne(d => d.Tank).WithMany(p => p.RegistrationRounds)
                .HasForeignKey(d => d.TankId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Registrat__TankI__6B24EA82");
        });

        modelBuilder.Entity<Round>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Rounds__3214EC079346BB66");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.MinScoreToAdvance).HasColumnType("decimal(5, 2)");
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
                .HasConstraintName("FK__Rounds__Competit__534D60F1");
        });

        modelBuilder.Entity<RoundResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RoundRes__3214EC07CD9450D4");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.IsPublic).HasDefaultValue(false);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.TotalScore).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.RegistrationRounds).WithMany(p => p.RoundResults)
                .HasForeignKey(d => d.RegistrationRoundsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RoundResu__Regis__7E37BEF6");
        });

        modelBuilder.Entity<ScoreDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ScoreDet__3214EC079B2AEF02");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.InitialScore).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.IsPublic).HasDefaultValue(false);
            entity.Property(e => e.TotalPointMinus).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.RefereeAccount).WithMany(p => p.ScoreDetails)
                .HasForeignKey(d => d.RefereeAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ScoreDeta__Refer__70DDC3D8");

            entity.HasOne(d => d.RegistrationRound).WithMany(p => p.ScoreDetails)
                .HasForeignKey(d => d.RegistrationRoundId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ScoreDeta__Regis__6FE99F9F");
        });

        modelBuilder.Entity<ScoreDetailError>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ScoreDet__3214EC0796A1F69F");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.PointMinus).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Severity).HasMaxLength(20);

            entity.HasOne(d => d.ErrorType).WithMany(p => p.ScoreDetailErrors)
                .HasForeignKey(d => d.ErrorTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ScoreDeta__Error__797309D9");

            entity.HasOne(d => d.ScoreDetail).WithMany(p => p.ScoreDetailErrors)
                .HasForeignKey(d => d.ScoreDetailId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ScoreDeta__Score__787EE5A0");
        });

        modelBuilder.Entity<ShowRule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ShowRule__3214EC0744C9904B");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.KoiShow).WithMany(p => p.ShowRules)
                .HasForeignKey(d => d.KoiShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ShowRules__KoiSh__51300E55");
        });

        modelBuilder.Entity<ShowStaff>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ShowStaf__3214EC074E0CFA56");

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
            entity.HasKey(e => e.Id).HasName("PK__ShowStat__3214EC073ED5BEFA");

            entity.ToTable("ShowStatus");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.StatusName).HasMaxLength(50);

            entity.HasOne(d => d.KoiShow).WithMany(p => p.ShowStatuses)
                .HasForeignKey(d => d.KoiShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ShowStatu__KoiSh__2645B050");
        });

        modelBuilder.Entity<Sponsor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Sponsors__3214EC071A28D094");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.InvestMoney).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.KoiShow).WithMany(p => p.Sponsors)
                .HasForeignKey(d => d.KoiShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Sponsors__KoiSho__2180FB33");
        });

        modelBuilder.Entity<Tank>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tanks__3214EC07AF7A97B5");

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

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Tanks)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tanks__CreatedBy__656C112C");

            entity.HasOne(d => d.KoiShow).WithMany(p => p.Tanks)
                .HasForeignKey(d => d.KoiShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tanks__KoiShowId__6477ECF3");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tickets__3214EC07F2FA17F1");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ExpiredDate).HasColumnType("datetime");
            entity.Property(e => e.IsUsed)
                .IsRequired()
                .HasDefaultValueSql("('0')");
            entity.Property(e => e.QrcodeData)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("QRCodeData");

            entity.HasOne(d => d.TicketOrderDetail).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.TicketOrderDetailId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tickets__TicketO__14270015");
        });

        modelBuilder.Entity<TicketOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TicketOr__3214EC07E245EFA2");

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

            entity.HasOne(d => d.Account).WithMany(p => p.TicketOrders)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TicketOrd__Accou__09A971A2");

            entity.HasOne(d => d.PaymentType).WithMany(p => p.TicketOrders)
                .HasForeignKey(d => d.PaymentTypeId)
                .HasConstraintName("FK__TicketOrd__Payme__0A9D95DB");
        });

        modelBuilder.Entity<TicketOrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TicketOr__3214EC0779673E6B");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
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
            entity.HasKey(e => e.Id).HasName("PK__TicketTy__3214EC07A6F02513");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TicketType1)
                .HasMaxLength(50)
                .HasColumnName("TicketType");

            entity.HasOne(d => d.KoiShow).WithMany(p => p.TicketTypes)
                .HasForeignKey(d => d.KoiShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TicketTyp__KoiSh__04E4BC85");
        });

        modelBuilder.Entity<Variety>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Varietie__3214EC071AD1104D");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Vote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Votes__3214EC07FB4DC04C");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.Votes)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Votes__AccountId__2DE6D218");

            entity.HasOne(d => d.Registration).WithMany(p => p.Votes)
                .HasForeignKey(d => d.RegistrationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Votes__Registrat__2EDAF651");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
