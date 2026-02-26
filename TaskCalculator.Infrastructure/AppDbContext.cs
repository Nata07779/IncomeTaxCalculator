using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TaxCalculator.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<TaxBand> TaxBands { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seed default tax bands (acts as dictionary table values)
            builder.Entity<TaxBand>().HasData(
                new TaxBand { Id = 1, Band = "A", LowerLimit = 0, Rate = 0 },
                new TaxBand { Id = 2, Band = "B", LowerLimit = 5000, Rate = 20 },
                new TaxBand { Id = 3, Band = "C", LowerLimit = 20000, Rate = 40 }
            );
        }
    }
}
