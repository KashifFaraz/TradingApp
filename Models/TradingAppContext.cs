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

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<MeasureUnit> MeasureUnits { get; set; }

    public virtual DbSet<Stakeholder> Stakeholders { get; set; }

    public virtual DbSet<StakeholderType> StakeholderTypes { get; set; }

    public virtual DbSet<TradingDocument> TradingDocuments { get; set; }

    public virtual DbSet<TradingDocumentDetail> TradingDocumentDetails { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=TradingApp;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
       

        modelBuilder.Entity<Item>(entity =>
        {
            entity.ToTable("Item");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.CraetedOn).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.EditedOn).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Item)
                .HasForeignKey<Item>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Item_MeasureUnit");
        });

        modelBuilder.Entity<MeasureUnit>(entity =>
        {
            entity.ToTable("MeasureUnit");

            entity.Property(e => e.Name)
                .HasMaxLength(10)
                .IsFixedLength();
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

        modelBuilder.Entity<TradingDocument>(entity =>
        {
            entity.ToTable("TradingDocument");

            entity.Property(e => e.AccountTitle).HasMaxLength(50);
            entity.Property(e => e.BankName).HasMaxLength(50);
            entity.Property(e => e.CraetedOn).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DocDate).HasColumnType("datetime");
            entity.Property(e => e.DueDate).HasColumnType("datetime");
            entity.Property(e => e.EditedOn).HasColumnType("datetime");
            entity.Property(e => e.Rfqid).HasColumnName("RFQId");
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Invoice).WithMany(p => p.InverseInvoice)
                .HasForeignKey(d => d.InvoiceId)
                .HasConstraintName("FK_TradingDocument_TradingDocument4");

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

            entity.HasOne(d => d.Stakeholder).WithMany(p => p.TradingDocuments)
                .HasForeignKey(d => d.StakeholderId)
                .HasConstraintName("FK_TradingDocument_StackholderType");
        });

        modelBuilder.Entity<TradingDocumentDetail>(entity =>
        {
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CraetedOn).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.EditedOn).HasColumnType("datetime");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Item).WithMany(p => p.TradingDocumentDetails)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("FK_TradingDocumentDetails_Item");

            entity.HasOne(d => d.Master).WithMany(p => p.TradingDocumentDetails)
                .HasForeignKey(d => d.MasterId)
                .HasConstraintName("FK_TradingDocumentDetails_TradingDocument");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
