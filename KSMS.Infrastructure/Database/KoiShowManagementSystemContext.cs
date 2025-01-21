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

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CategoryTank> CategoryTanks { get; set; }

    public virtual DbSet<CheckInLog> CheckInLogs { get; set; }

    public virtual DbSet<CriteriaGroup> CriteriaGroups { get; set; }

    public virtual DbSet<Criterion> Criteria { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<FishTankAssignment> FishTankAssignments { get; set; }

    public virtual DbSet<GrandChampionContender> GrandChampionContenders { get; set; }

    public virtual DbSet<KoiProfile> KoiProfiles { get; set; }

    public virtual DbSet<Livestream> Livestreams { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<PaymentType> PaymentTypes { get; set; }

    public virtual DbSet<Qrcode> Qrcodes { get; set; }

    public virtual DbSet<RefereeAssignment> RefereeAssignments { get; set; }

    public virtual DbSet<Registration> Registrations { get; set; }

    public virtual DbSet<RegistrationPayment> RegistrationPayments { get; set; }

    public virtual DbSet<Result> Results { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Round> Rounds { get; set; }

    public virtual DbSet<Score> Scores { get; set; }

    public virtual DbSet<Show> Shows { get; set; }

    public virtual DbSet<ShowRule> ShowRules { get; set; }

    public virtual DbSet<ShowStaff> ShowStaffs { get; set; }

    public virtual DbSet<ShowStatistic> ShowStatistics { get; set; }

    public virtual DbSet<ShowStatus> ShowStatuses { get; set; }

    public virtual DbSet<Sponsor> Sponsors { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TicketOrder> TicketOrders { get; set; }

    public virtual DbSet<TicketOrderDetail> TicketOrderDetails { get; set; }

    public virtual DbSet<Variety> Varieties { get; set; }

    public virtual DbSet<Vote> Votes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Accounts__3214EC07600245AE");

            entity.HasIndex(e => e.Username, "UQ__Accounts__536C85E474B2F955").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Accounts__A9D105345D2E116C").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Avatar).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.HashedPassword)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("active");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Role).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__Accounts__RoleId__2C3393D0");
        });

        modelBuilder.Entity<Award>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Awards__3214EC072F986F65");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AwardType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.PrizeValue).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Category).WithMany(p => p.Awards)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Awards__Category__412EB0B6");
        });

        modelBuilder.Entity<BlogCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BlogCate__3214EC07B64404EE");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<BlogsNews>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BlogsNew__3214EC079C6E2A00");

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
                .HasConstraintName("FK__BlogsNews__Accou__2739D489");

            entity.HasOne(d => d.BlogCategory).WithMany(p => p.BlogsNews)
                .HasForeignKey(d => d.BlogCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BlogsNews__BlogC__282DF8C2");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Categori__3214EC074EAA3ADD");

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

            entity.HasOne(d => d.Show).WithMany(p => p.Categories)
                .HasForeignKey(d => d.ShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Categorie__Updat__37A5467C");

            entity.HasOne(d => d.Variety).WithMany(p => p.Categories)
                .HasForeignKey(d => d.VarietyId)
                .HasConstraintName("FK__Categorie__Varie__38996AB5");
        });

        modelBuilder.Entity<CategoryTank>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3214EC070514BB8D");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Capacity).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Dimensions)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.PhLevel).HasColumnType("decimal(3, 1)");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Temperature).HasColumnType("decimal(4, 1)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Category).WithMany(p => p.CategoryTanks)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CategoryT__Categ__787EE5A0");
        });

        modelBuilder.Entity<CheckInLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CheckInL__3214EC078111DB00");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CheckInLocation).HasMaxLength(100);
            entity.Property(e => e.CheckInTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Notes).HasMaxLength(255);
            entity.Property(e => e.QrcodeId).HasColumnName("QRCodeId");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.CheckedInByNavigation).WithMany(p => p.CheckInLogs)
                .HasForeignKey(d => d.CheckedInBy)
                .HasConstraintName("FK__CheckInLo__Check__40058253");

            entity.HasOne(d => d.Qrcode).WithMany(p => p.CheckInLogs)
                .HasForeignKey(d => d.QrcodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CheckInLo__QRCod__3F115E1A");
        });

        modelBuilder.Entity<CriteriaGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Criteria__3214EC076A058E96");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.RoundType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Category).WithMany(p => p.CriteriaGroups)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CriteriaG__Categ__48CFD27E");
        });

        modelBuilder.Entity<Criterion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Criteria__3214EC076EC7FD76");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.MaxScore).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.Weight).HasColumnType("decimal(3, 2)");

            entity.HasOne(d => d.CriteriaGroup).WithMany(p => p.Criteria)
                .HasForeignKey(d => d.CriteriaGroupId)
                .HasConstraintName("FK__Criteria__Criter__4CA06362");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Feedback__3214EC07BE051035");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedbacks__Accou__0A9D95DB");

            entity.HasOne(d => d.Show).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.ShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedbacks__ShowI__0B91BA14");
        });

        modelBuilder.Entity<FishTankAssignment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__FishTank__3214EC07FDC281D2");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CheckInTime).HasColumnType("datetime");
            entity.Property(e => e.CheckOutTime).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Registration).WithMany(p => p.FishTankAssignments)
                .HasForeignKey(d => d.RegistrationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FishTankA__Regis__7C4F7684");

            entity.HasOne(d => d.Tank).WithMany(p => p.FishTankAssignments)
                .HasForeignKey(d => d.TankId)
                .HasConstraintName("FK__FishTankA__TankI__7D439ABD");
        });

        modelBuilder.Entity<GrandChampionContender>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GrandCha__3214EC07CCD76406");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.QualificationType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Registration).WithMany(p => p.GrandChampionContenders)
                .HasForeignKey(d => d.RegistrationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GrandCham__Regis__02084FDA");

            entity.HasOne(d => d.Round).WithMany(p => p.GrandChampionContenders)
                .HasForeignKey(d => d.RoundId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GrandCham__Round__01142BA1");
        });

        modelBuilder.Entity<KoiProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__KoiProfi__3214EC072402847B");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Bloodline).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.ImgUrl)
                .HasMaxLength(255)
                .HasColumnName("ImgURL");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Size).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.VideoUrl)
                .HasMaxLength(255)
                .HasColumnName("VideoURL");

            entity.HasOne(d => d.Owner).WithMany(p => p.KoiProfiles)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KoiProfil__Owner__3C69FB99");

            entity.HasOne(d => d.Variety).WithMany(p => p.KoiProfiles)
                .HasForeignKey(d => d.VarietyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KoiProfil__Varie__3D5E1FD2");
        });

        modelBuilder.Entity<Livestream>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Livestre__3214EC0790576285");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.StreamUrl).HasMaxLength(500);

            entity.HasOne(d => d.Show).WithMany(p => p.Livestreams)
                .HasForeignKey(d => d.ShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Livestrea__ShowI__1BC821DD");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3214EC07657E1E50");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.SentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK__Notificat__Accou__06CD04F7");
        });

        modelBuilder.Entity<PaymentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PaymentT__3214EC0797EF4DD0");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<Qrcode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__QRCodes__3214EC07442F46C4");

            entity.ToTable("QRCodes");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.QrcodeData)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("QRCodeData");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.RegistrationPayment).WithMany(p => p.Qrcodes)
                .HasForeignKey(d => d.RegistrationPaymentId)
                .HasConstraintName("FK__QRCodes__Registr__339FAB6E");

            entity.HasOne(d => d.TicketOrderDetail).WithMany(p => p.Qrcodes)
                .HasForeignKey(d => d.TicketOrderDetailId)
                .HasConstraintName("FK__QRCodes__TicketO__32AB8735");
        });

        modelBuilder.Entity<RefereeAssignment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RefereeA__3214EC078AA457D5");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.RefereeAssignmentAssignedByNavigations)
                .HasForeignKey(d => d.AssignedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RefereeAs__Assig__395884C4");

            entity.HasOne(d => d.Category).WithMany(p => p.RefereeAssignments)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RefereeAs__Categ__3A4CA8FD");

            entity.HasOne(d => d.RefereeAccount).WithMany(p => p.RefereeAssignmentRefereeAccounts)
                .HasForeignKey(d => d.RefereeAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RefereeAs__Refer__3864608B");
        });

        modelBuilder.Entity<Registration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Registra__3214EC077A85F7B5");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ApprovedAt).HasColumnType("datetime");
            entity.Property(e => e.Bloodline).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.ImgUrl)
                .HasMaxLength(255)
                .HasColumnName("ImgURL");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.RegistrationFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RegistrationNumber).HasMaxLength(100);
            entity.Property(e => e.Size).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.VideoUrl)
                .HasMaxLength(255)
                .HasColumnName("VideoURL");

            entity.HasOne(d => d.Account).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Registrat__Accou__5070F446");

            entity.HasOne(d => d.Category).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Registrat__Categ__5165187F");
        });

        modelBuilder.Entity<RegistrationPayment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Registra__3214EC079AD86D5B");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.PaidAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.PaymentType).WithMany(p => p.RegistrationPayments)
                .HasForeignKey(d => d.PaymentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Registrat__Payme__2DE6D218");

            entity.HasOne(d => d.Registration).WithMany(p => p.RegistrationPayments)
                .HasForeignKey(d => d.RegistrationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Registrat__Regis__2CF2ADDF");
        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Results__3214EC070283241D");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.FinalScore).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Award).WithMany(p => p.Results)
                .HasForeignKey(d => d.AwardId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Results__AwardId__5CD6CB2B");

            entity.HasOne(d => d.Registration).WithMany(p => p.Results)
                .HasForeignKey(d => d.RegistrationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Results__Registr__5BE2A6F2");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC076D31E9AB");

            entity.HasIndex(e => e.Name, "UQ__Roles__737584F62103FE87").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Round>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Rounds__3214EC07EC4E4ECC");

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

            entity.HasOne(d => d.Category).WithMany(p => p.Rounds)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Rounds__Category__44FF419A");
        });

        modelBuilder.Entity<Score>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Scores__3214EC071B244C20");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Score1)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("Score");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Criteria).WithMany(p => p.Scores)
                .HasForeignKey(d => d.CriteriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Scores__Criteria__5812160E");

            entity.HasOne(d => d.RefereeAccount).WithMany(p => p.Scores)
                .HasForeignKey(d => d.RefereeAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Scores__RefereeA__571DF1D5");

            entity.HasOne(d => d.Registration).WithMany(p => p.Scores)
                .HasForeignKey(d => d.RegistrationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Scores__Registra__5535A963");

            entity.HasOne(d => d.Round).WithMany(p => p.Scores)
                .HasForeignKey(d => d.RoundId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Scores__RoundId__5629CD9C");
        });

        modelBuilder.Entity<Show>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Shows__3214EC07B0326724");

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

        modelBuilder.Entity<ShowRule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ShowRule__3214EC07ED787D78");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Show).WithMany(p => p.ShowRules)
                .HasForeignKey(d => d.ShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ShowRules__ShowI__43D61337");
        });

        modelBuilder.Entity<ShowStaff>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ShowStaf__3214EC07C9B00CD8");

            entity.ToTable("ShowStaff");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.ShowStaffAccounts)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ShowStaff__Accou__628FA481");

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.ShowStaffAssignedByNavigations)
                .HasForeignKey(d => d.AssignedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ShowStaff__Assig__6383C8BA");

            entity.HasOne(d => d.Show).WithMany(p => p.ShowStaffs)
                .HasForeignKey(d => d.ShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ShowStaff__ShowI__619B8048");
        });

        modelBuilder.Entity<ShowStatistic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ShowStat__3214EC07B69251A0");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.MetricName).HasMaxLength(255);
            entity.Property(e => e.MetricValue).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Show).WithMany(p => p.ShowStatistics)
                .HasForeignKey(d => d.ShowId)
                .HasConstraintName("FK__ShowStati__ShowI__1332DBDC");
        });

        modelBuilder.Entity<ShowStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ShowStat__3214EC07F5C0F4FD");

            entity.ToTable("ShowStatus");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.StatusName).HasMaxLength(50);

            entity.HasOne(d => d.Show).WithMany(p => p.ShowStatuses)
                .HasForeignKey(d => d.ShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ShowStatu__ShowI__17F790F9");
        });

        modelBuilder.Entity<Sponsor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Sponsors__3214EC0746671BA4");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.Show).WithMany(p => p.Sponsors)
                .HasForeignKey(d => d.ShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Sponsors__ShowId__0F624AF8");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tickets__3214EC070F7C2CD6");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TicketType).HasMaxLength(50);

            entity.HasOne(d => d.Show).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.ShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tickets__ShowId__6A30C649");
        });

        modelBuilder.Entity<TicketOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TicketOr__3214EC07DB85FEAB");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Account).WithMany(p => p.TicketOrders)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TicketOrd__Accou__6EF57B66");

            entity.HasOne(d => d.PaymentType).WithMany(p => p.TicketOrders)
                .HasForeignKey(d => d.PaymentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TicketOrd__Payme__6FE99F9F");
        });

        modelBuilder.Entity<TicketOrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TicketOr__3214EC07880E061E");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketOrderDetails)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("FK__TicketOrd__Ticke__74AE54BC");

            entity.HasOne(d => d.TicketOrder).WithMany(p => p.TicketOrderDetails)
                .HasForeignKey(d => d.TicketOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TicketOrd__Ticke__73BA3083");
        });

        modelBuilder.Entity<Variety>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Varietie__3214EC07FC841E2E");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Vote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Votes__3214EC07E7239F76");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.Votes)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Votes__AccountId__1F98B2C1");

            entity.HasOne(d => d.Registration).WithMany(p => p.Votes)
                .HasForeignKey(d => d.RegistrationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Votes__Registrat__208CD6FA");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
