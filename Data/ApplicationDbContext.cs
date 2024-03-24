using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TradingApp.Models;

namespace TradingApp.Data
{
    public class ApplicationDbContext  : IdentityDbContext<ApplicationUser,IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Customize the Identity table names
            modelBuilder.Entity<IdentityRole<int>>().ToTable("AppRoles");
            modelBuilder.Entity<ApplicationUser>().ToTable("AppUsers");
            modelBuilder.Entity<IdentityUserRole<int>>().ToTable("AppUserRoles");
            modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("AppUserClaims");
            modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("AppUserLogins");
            modelBuilder.Entity<IdentityUserToken<int>>().ToTable("AppUserTokens");
            modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("AppRoleClaims");
        }
    }
}