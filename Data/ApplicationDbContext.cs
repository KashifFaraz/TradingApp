using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TradingApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Customize the Identity table names
            modelBuilder.Entity<IdentityRole>().ToTable("AppRoles");
            modelBuilder.Entity<IdentityUser>().ToTable("AppUsers");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("AppUserRoles");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("AppUserClaims");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("AppUserLogins");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("AppUserTokens");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("AppRoleClaims");
        }
    }
}