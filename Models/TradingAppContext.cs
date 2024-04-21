using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TradingApp.Models;

public partial class TradingAppContext : DbContext
{
    public TradingAppContext()
    {
    }

    public TradingAppContext(DbContextOptions<TradingAppContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppRole> AppRoles { get; set; }

    public virtual DbSet<AppRoleClaim> AppRoleClaims { get; set; }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<AppUserClaim> AppUserClaims { get; set; }

    public virtual DbSet<AppUserLogin> AppUserLogins { get; set; }

    public virtual DbSet<AppUserToken> AppUserTokens { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoiceLine> InvoiceLines { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<MeasureUnit> MeasureUnits { get; set; }

    public virtual DbSet<Organization> Organizations { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentReconciliation> PaymentReconciliations { get; set; }

    public virtual DbSet<Stakeholder> Stakeholders { get; set; }

    public virtual DbSet<StakeholderType> StakeholderTypes { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=Dev01-TradingApp;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AppRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AppRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AppRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AppUserRole",
                    r => r.HasOne<AppRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AppUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AppUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AppUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AppUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AppUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AppUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AppUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AppUserLogins_UserId");

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.ProviderKey).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AppUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AppUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.Name).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AppUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TradingDocument");

            entity.ToTable("Invoice");

            entity.Property(e => e.AccountTitle).HasMaxLength(50);
            entity.Property(e => e.BankName).HasMaxLength(50);
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DocDate).HasColumnType("datetime");
            entity.Property(e => e.DueDate).HasColumnType("datetime");
            entity.Property(e => e.EditedOn).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.Rfqid).HasColumnName("RFQId");
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UnreconciledAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InvoiceCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_TradingDocument_AppUsers");

            entity.HasOne(d => d.EditedByNavigation).WithMany(p => p.InvoiceEditedByNavigations)
                .HasForeignKey(d => d.EditedBy)
                .HasConstraintName("FK_TradingDocument_AppUsers1");

            entity.HasOne(d => d.Organization).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.OrganizationId)
                .HasConstraintName("FK_TradingDocument_Organization");

            entity.HasOne(d => d.PurchaseOrder).WithMany(p => p.InversePurchaseOrder)
                .HasForeignKey(d => d.PurchaseOrderId)
                .HasConstraintName("FK_TradingDocument_TradingDocument2");

            entity.HasOne(d => d.Quote).WithMany(p => p.InverseQuote)
                .HasForeignKey(d => d.QuoteId)
                .HasConstraintName("FK_TradingDocument_TradingDocument");

            entity.HasOne(d => d.Rfq).WithMany(p => p.InverseRfq)
                .HasForeignKey(d => d.Rfqid)
                .HasConstraintName("FK_TradingDocument_TradingDocument1");

            entity.HasOne(d => d.SalesOder).WithMany(p => p.InverseSalesOder)
                .HasForeignKey(d => d.SalesOderId)
                .HasConstraintName("FK_TradingDocument_TradingDocument3");

            entity.HasOne(d => d.Stakeholder).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.StakeholderId)
                .HasConstraintName("FK_TradingDocument_Stackholder");

            entity.HasOne(d => d.Transaction).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.TransactionId)
                .HasConstraintName("FK_Invoice_Transaction");
        });

        modelBuilder.Entity<InvoiceLine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TradingDocumentDetails");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CraetedOn).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.EditedOn).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Item).WithMany(p => p.InvoiceLines)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("FK_TradingDocumentDetails_Item");

            entity.HasOne(d => d.Master).WithMany(p => p.InvoiceLines)
                .HasForeignKey(d => d.MasterId)
                .HasConstraintName("FK_TradingDocumentDetails_TradingDocument");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.ToTable("Item");

            entity.Property(e => e.CraetedOn).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.EditedOn).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 0)");

            entity.HasOne(d => d.SaleUnitNavigation).WithMany(p => p.Items)
                .HasForeignKey(d => d.SaleUnit)
                .HasConstraintName("FK_Item_MeasureUnit");
        });

        modelBuilder.Entity<MeasureUnit>(entity =>
        {
            entity.ToTable("MeasureUnit");

            entity.Property(e => e.Name).HasMaxLength(20);
            entity.Property(e => e.Symbol)
                .HasMaxLength(20)
                .HasColumnName("symbol");
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.ToTable("Organization");

            entity.Property(e => e.CraetedOn).HasColumnType("datetime");
            entity.Property(e => e.DefaultCurrency)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.EditedOn).HasColumnType("datetime");
            entity.Property(e => e.FolderName).HasMaxLength(500);
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payment");

            entity.Property(e => e.AccountTitle).HasMaxLength(50);
            entity.Property(e => e.BankName).HasMaxLength(50);
            entity.Property(e => e.CraetedOn).HasColumnType("datetime");
            entity.Property(e => e.CustomId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DocDate).HasColumnType("datetime");
            entity.Property(e => e.DueDate).HasColumnType("datetime");
            entity.Property(e => e.EditedOn).HasColumnType("datetime");
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UnreconciledAmount).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<PaymentReconciliation>(entity =>
        {
            entity.ToTable("PaymentReconciliation");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CraetedOn).HasColumnType("datetime");
            entity.Property(e => e.EditedOn).HasColumnType("datetime");

            entity.HasOne(d => d.Payment).WithMany(p => p.PaymentReconciliations)
                .HasForeignKey(d => d.PaymentId)
                .HasConstraintName("FK_PaymentReconciliation_Payment");

            entity.HasOne(d => d.TradingDocument).WithMany(p => p.PaymentReconciliations)
                .HasForeignKey(d => d.TradingDocumentId)
                .HasConstraintName("FK_PaymentReconciliation_TradingDocument");
        });

        modelBuilder.Entity<Stakeholder>(entity =>
        {
            entity.ToTable("Stakeholder");

            entity.Property(e => e.CraetedOn).HasColumnType("datetime");
            entity.Property(e => e.EditedOn).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<StakeholderType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_StackholderType");

            entity.ToTable("StakeholderType");

            entity.Property(e => e.Name).HasMaxLength(500);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("Transaction");

            entity.HasIndex(e => e.CustomId, "Unique_Transaction").IsUnique();

            entity.Property(e => e.TransactionId).ValueGeneratedNever();
            entity.Property(e => e.CustomId)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
