using Microsoft.EntityFrameworkCore;

namespace CastingBase.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            foreach (var entry in ChangeTracker.Entries<BaseEntity>()
                         .Where(e => e.State is EntityState.Added or EntityState.Modified))
            {
                entry.Entity.UpdatedAt = now;
                if (entry.State == EntityState.Added)
                    entry.Entity.CreatedAt = now;
            }
            return await base.SaveChangesAsync(cancellationToken);
        }
        public override int SaveChanges() => SaveChangesAsync().GetAwaiter().GetResult();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var user = modelBuilder.Entity<User>();

            user.HasIndex(u => u.Username)
                .IsUnique()
                .HasDatabaseName("IX_User_Username");

            user.HasIndex(u => u.EMail)
                .IsUnique()
                .HasDatabaseName("IX_User_Email");

            user.HasIndex(u => u.PhoneNumber)
                .IsUnique()
                .HasDatabaseName("IX_User_PhoneNumber");

        }
    }
}
