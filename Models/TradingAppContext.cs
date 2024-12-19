using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using TradingApp.Models.Config;

namespace TradingApp.Models;

public partial class TradingAppContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TradingAppContext()
    {
    }

    public TradingAppContext(DbContextOptions<TradingAppContext> options, IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;

    }

    public virtual DbSet<AccountType> AccountTypes { get; set; }

    public virtual DbSet<AppRole> AppRoles { get; set; }

    public virtual DbSet<AppRoleClaim> AppRoleClaims { get; set; }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<AppUserClaim> AppUserClaims { get; set; }

    public virtual DbSet<AppUserLogin> AppUserLogins { get; set; }

    public virtual DbSet<AppUserToken> AppUserTokens { get; set; }

    public virtual DbSet<ChartOfAccount> ChartOfAccounts { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoiceLine> InvoiceLines { get; set; }

    public virtual DbSet<Item> Items { get; set; }
    public virtual DbSet<Tax> Taxes { get; set; }

    public virtual DbSet<JournalEntry> JournalEntries { get; set; }

    public virtual DbSet<MeasureUnit> MeasureUnits { get; set; }

    public virtual DbSet<Organization> Organizations { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentReconciliation> PaymentReconciliations { get; set; }

    public virtual DbSet<ProductBrand> ProductBrands { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<Receipt> Receipts { get; set; }

    public virtual DbSet<Stakeholder> Stakeholders { get; set; }

    public virtual DbSet<StakeholderType> StakeholderTypes { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=Dev01-TradingApp;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");


    private static LambdaExpression CreateIsActiveFilterExpression(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "e");
        var property = Expression.Property(parameter, "IsActive");

        // Check if IsActive is true, handling null values correctly
        var trueConstant = Expression.Constant(true, typeof(bool));

        // Create an expression that checks if IsActive has a value and that value is true
        var hasValueExpression = Expression.Property(property, "HasValue");
        var valueExpression = Expression.Property(property, "Value");
        var isTrueExpression = Expression.Equal(valueExpression, trueConstant);
        var filterExpression = Expression.AndAlso(hasValueExpression, isTrueExpression);

        var filter = Expression.Lambda(filterExpression, parameter);
        return filter;
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder);
        // Configure shadow properties for tenant filtering
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.GetProperty("TenantId") != null)
            {
                modelBuilder.Entity(entityType.ClrType).Property<int>("TenantId");
            }
        }
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.GetProperty("IsActive") != null)
            {
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(CreateIsActiveFilterExpression(entityType.ClrType));
            }
        }

        // Automatically apply configurations for all entities inheriting from BaseEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType) && !e.ClrType.IsAbstract))
        {
            var entity = modelBuilder.Entity(entityType.ClrType);

            // Configure common properties for BaseEntity
            entity.HasKey(nameof(BaseEntity.Id)).HasName($"PK_{entityType.ClrType.Name}");

            entity.Property(nameof(BaseEntity.CreatedOn))
                .HasColumnType("datetime");

            entity.Property(nameof(BaseEntity.EditedOn))
                .HasColumnType("datetime");

            entity.Property(nameof(BaseEntity.IsActive))
                .HasDefaultValueSql("((1))");
        }

        modelBuilder.Entity<AccountType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AccountT__3214EC07A166E9D5");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(50);
        });

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

        modelBuilder.Entity<ChartOfAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChartOfA__3214EC0705ACA639");

            entity.Property(e => e.IsTransactionAccount).HasDefaultValueSql("((0))");
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.AccountType).WithMany(p => p.ChartOfAccounts)
                .HasForeignKey(d => d.AccountTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChartOfAc__Accou__0A688BB1");

            entity.HasOne(d => d.ParentAccount).WithMany(p => p.InverseParentAccount)
                .HasForeignKey(d => d.ParentAccountId)
                .HasConstraintName("FK__ChartOfAc__Paren__0B5CAFEA");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            

            entity.ToTable("Invoice");

            entity.Property(e => e.AccountTitle).HasMaxLength(50);
            entity.Property(e => e.BankName).HasMaxLength(50);
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.CustomId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Terms).HasMaxLength(1000);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.DocDate).HasColumnType("datetime");
            entity.Property(e => e.DueDate).HasColumnType("datetime");
            entity.Property(e => e.EditedOn).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.Rfqid).HasColumnName("RFQId");
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UnreconciledAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InvoiceCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Invoice_AppUsers");

            entity.HasOne(d => d.EditedByNavigation).WithMany(p => p.InvoiceEditedByNavigations)
                .HasForeignKey(d => d.EditedBy)
                .HasConstraintName("FK_Invoice_AppUsers1");

            entity.HasOne(d => d.Organization).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.OrganizationId)
                .HasConstraintName("FK_TradingDocument_Organization");

           

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

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TaxPercentage).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Description).HasMaxLength(500);
            
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DiscountPercentage).HasColumnType("decimal(18, 2)").HasDefaultValue(0);

            entity.HasOne(d => d.Item).WithMany()
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("FK_TradingDocumentDetails_Item");

            entity.HasOne(d => d.Master).WithMany(p => p.InvoiceLines)
                .HasForeignKey(d => d.MasterId)
                .HasConstraintName("FK_TradingDocumentDetails_TradingDocument");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.ToTable("Item");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Code).HasMaxLength(100);
            entity.Property(e => e.Barcode).HasMaxLength(50);
            entity.Property(e => e.QRCode).HasMaxLength(255);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.Weight).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.Width).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.Height).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.Length).HasColumnType("decimal(18, 0)");

            entity.HasOne(d => d.ProductBrand).WithMany(p => p.Items)
                .HasForeignKey(d => d.ProductBrandId)
                .HasConstraintName("FK_Item_ProductBrand");

            entity.HasOne(d => d.ProductCategory).WithMany(p => p.Items)
                .HasForeignKey(d => d.ProductCategoryId)
                .HasConstraintName("FK_Item_ProductCategory");

            entity.HasOne(d => d.SaleUnitNavigation).WithMany(p => p.Items)
                .HasForeignKey(d => d.SaleUnit)
                .HasConstraintName("FK_Item_MeasureUnit");
        });

        modelBuilder.Entity<Tax>(entity =>
        {
            entity.ToTable("Tax");
            entity.HasKey(e => e.Id).HasName("PK_Tax");
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.EditedOn).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Value).HasColumnType("decimal(18, 0)");
        });


        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__JournalE__3214EC07D5AC7FB7");

            entity.ToTable("JournalEntry");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.EditedOn).HasColumnType("datetime");
            entity.Property(e => e.Type)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
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

            entity.HasOne(d => d.TradingDocument).WithMany(p => p.PaymentReconciliations)
                .HasForeignKey(d => d.TradingDocumentId)
                .HasConstraintName("FK_PaymentReconciliation_TradingDocument");
        });

        modelBuilder.Entity<ProductBrand>(entity =>
        {
            entity.ToTable("ProductBrand");

            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.EditedOn).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.ToTable("ProductCategory");

            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.EditedOn).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK_ProductCategory_ProductCategory");
        });

        modelBuilder.Entity<Receipt>(entity =>
        {
            entity.ToTable("Receipt");

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

            entity.HasOne(d => d.Stakeholder).WithMany(p => p.Receipts)
                .HasForeignKey(d => d.StakeholderId)
                .HasConstraintName("FK_Receipt_Stakeholder");

            entity.HasOne(d => d.Transaction).WithMany(p => p.Receipts)
                .HasForeignKey(d => d.TransactionId)
                .HasConstraintName("FK_Receipt_Transaction");
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
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity &&
                        (e.State == EntityState.Added || e.State == EntityState.Modified));

        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        foreach (var entry in entries)
        {
            var auditableEntity = (BaseEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                auditableEntity.CreatedBy = Convert.ToInt32(userId);
                auditableEntity.CreatedOn = DateTime.Now;
                auditableEntity.IsActive = true;
            }

            auditableEntity.EditedBy = Convert.ToInt32(userId);
            auditableEntity.EditedOn = DateTime.Now;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity &&
                        (e.State == EntityState.Added || e.State == EntityState.Modified));

        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        foreach (var entry in entries)
        {
            var auditableEntity = (BaseEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                auditableEntity.CreatedBy = Convert.ToInt32(userId);
                auditableEntity.CreatedOn = DateTime.Now;
                auditableEntity.IsActive = true;
            }

            auditableEntity.EditedBy = Convert.ToInt32(userId);
            auditableEntity.EditedOn = DateTime.Now;
        }

        return base.SaveChanges();
    }
}
