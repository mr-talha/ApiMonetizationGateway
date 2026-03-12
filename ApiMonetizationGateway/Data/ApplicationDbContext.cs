using ApiMonetizationGateway.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiMonetizationGateway.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Tier> Tiers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<ApiUsageLog> ApiUsageLogs { get; set; }
        public DbSet<MonthlyUsageSummary> MonthlyUsageSummaries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tier
            modelBuilder.Entity<Tier>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Name).IsRequired().HasMaxLength(50);
                entity.Property(t => t.Description).HasMaxLength(255);
                entity.Property(t => t.Price).HasColumnType("decimal(10,2)");

                entity.HasData(
                    new Tier { Id = 1, Name = "Free", MonthlyQuota = 100, RateLimit = 2, Price = 0.00m, Description = "Free tier with basic limits", CreatedAt = new DateTime(2025, 9, 19) },
                    new Tier { Id = 2, Name = "Pro", MonthlyQuota = 100000, RateLimit = 10, Price = 50.00m, Description = "Professional tier with higher limits", CreatedAt = new DateTime(2025, 9, 19) }
                );
            });

            // Customer
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Email).IsRequired().HasMaxLength(255);
                entity.Property(c => c.ApiKey).IsRequired().HasMaxLength(100);

                entity.HasIndex(c => c.ApiKey).IsUnique();

                entity.HasOne(c => c.Tier)
                    .WithMany(t => t.Customers)
                    .HasForeignKey(c => c.TierId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasData(
                    new Customer { Id = 1, Name = "Free Tier Customer", Email = "free@tier.com", TierId = 1, ApiKey = "free_key", IsActive = true, CreatedAt = new DateTime(2026, 3, 10) },
                    new Customer { Id = 2, Name = "Pro Tier Customer", Email = "pro@tier.com", TierId = 2, ApiKey = "pro_key", IsActive = true, CreatedAt = new DateTime(2026, 3, 10) }
                );
            });

            // ApiUsageLog
            modelBuilder.Entity<ApiUsageLog>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Endpoint).IsRequired().HasMaxLength(255);
                entity.Property(u => u.Timestamp).IsRequired();
                entity.Property(u => u.UserId).HasMaxLength(128);

                entity.HasOne(u => u.Customer)
                    .WithMany(c => c.ApiUsageLogs)
                    .HasForeignKey(u => u.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(u => u.CustomerId);
                entity.HasIndex(u => u.Timestamp);
                entity.HasIndex(u => new { u.CustomerId, u.Timestamp });
            });

            // MonthlyUsageSummary
            modelBuilder.Entity<MonthlyUsageSummary>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.MonthYear).IsRequired().HasMaxLength(7);
                entity.Property(m => m.TotalCost).HasColumnType("decimal(10,2)");

                entity.HasOne(m => m.Customer)
                    .WithMany(c => c.MonthlySummaries)
                    .HasForeignKey(m => m.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.Tier)
                    .WithMany()
                    .HasForeignKey(m => m.TierId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(m => new { m.CustomerId, m.MonthYear }).IsUnique();
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
